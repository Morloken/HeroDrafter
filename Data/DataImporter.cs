using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;

namespace HeroDrafter.Data
{
    public class DataImporter
    {
        private string ReadFileWithCyrillicEncoding(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath, System.Text.Encoding.GetEncoding(1251));
            }
            catch
            {
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
        }

        private Dictionary<string, string> rarityTranslation = new Dictionary<string, string>
        {
            { "обычный", "Звичайний" }, { "редкий", "Рідкісний" }, { "Редкий", "Рідкісний" },
            { "эпический", "Епічний" }, { "Эпический", "Епічний" },
            { "легендарный", "Легендарний" }, { "Легендарный", "Легендарний" },
            { "Королевский", "Королівський" }, { "королевский", "Королівський" }
        };

        private Dictionary<string, string> roleTranslation = new Dictionary<string, string>
        {
            { "Маг", "Маг" }, { "маг", "Маг" },
            { "Стрелок", "Стрілець" }, { "стрелок", "Стрілець" },
            { "Ассасин", "Вбивця" }, { "ассасин", "Вбивця" },
            { "Поддержка", "Підтримка" }, { "поддержка", "Підтримка" },
            { "Танк", "Танк" }, { "танк", "Танк" },
            { "Гибрид", "Гібрид" }, { "гибрид", "Гібрид" }
        };

        private Dictionary<string, string> energyTranslation = new Dictionary<string, string>
        {
            { "Огня", "Вогонь" }, { "Электричества", "Електрика" }, { "Ветра", "Вітер" },
            { "Льда", "Лід" }, { "Природы", "Природа" }, { "Тьмы", "Темрява" },
            { "Силовой энергии", "Силові щити" }, { "Силовых щитов", "Силові щити" },
            { "Созидания", "Творення" }, { "сотворения", "Творення" },
            { "Света", "Світло" },
            { "Звука", "Звук" }, { "Взрывов", "Вибухи" },
            { "Некромантии", "Некромантія" }, { "Железа", "Залізо" },
            { "Гравитации", "Гравітація" }, { "Воды", "Вода" },
            { "Времени", "Час" }, { "Пространства", "Простір" },
            { "Радиации", "Радіація" }, { "Пара", "Пара" },
            { "Температуры", "Температура" }, { "Копирования", "Копіювання" },
            { "Блискавки", "Блискавка" }, { "Земли", "Земля" }, { "Камня", "Камінь" },
            { "Сейсмические волны", "Сейсмічні хвилі" }, { "Удачи", "Удача" },
            { "Призрачной материализации", "Привиділова матеріалізація" },
            { "---", "Відсутня" }, { "Молнии", "Блискавка" }
        };

        public void ImportFromFile(string filePath, SqlConnection connection)
        {
            string content = ReadFileWithCyrillicEncoding(filePath);
            string[] lines = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            SqlTransaction transaction = connection.BeginTransaction();
            int foundCount = 0;
            HashSet<int> insertedIds = new HashSet<int>();

            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    
                    if (line.Length > 0 && char.IsDigit(line[0]))
                    {
                        // Format: 5)Громовержец(thunderbolt). (Эпический){Стрелок}.
                        int closeParen = line.IndexOf(')');
                        if (closeParen < 0) continue;

                        string idStr = line.Substring(0, closeParen);
                        int id;
                        if (!int.TryParse(idStr, out id)) continue;

                        // Find '.' after ) to get name
                        int dotIndex = line.IndexOf('.', closeParen);
                        if (dotIndex < 0) continue;

                        string name = line.Substring(closeParen + 1, dotIndex - closeParen - 1).Trim();

                        // Remove any (English) part from name
                        int engParen = name.IndexOf('(');
                        if (engParen > 0) name = name.Substring(0, engParen).Trim();

                        // Find (rarity) after the dot
                        int openParen = line.IndexOf('(', dotIndex);
                        if (openParen < 0) continue;

                        int closeParen2 = line.IndexOf(')', openParen);
                        if (closeParen2 < 0) continue;

                        string rarityRaw = line.Substring(openParen + 1, closeParen2 - openParen - 1).Trim();

                        // Find {role} after rarity
                        int openBrace = line.IndexOf('{', closeParen2);
                        string roleRaw = "";
                        if (openBrace >= 0)
                        {
                            int closeBrace = line.IndexOf('}', openBrace);
                            if (closeBrace >= 0)
                            {
                                roleRaw = line.Substring(openBrace + 1, closeBrace - openBrace - 1).Trim();
                            }
                        }

                        File.AppendAllText("import_log.txt", "Found: ID=" + id + ", Name=" + name + ", Rarity=" + rarityRaw + ", Role=" + roleRaw + "\n");

                        string rarityUa = Translate(rarityRaw, rarityTranslation);
                        string roleUa = Translate(roleRaw, roleTranslation);
                        int basePower = CalculateBasePower(rarityUa);

                        // Find energy line "[Магия: ...]" in next lines
                        string energyRaw = "";
                        for (int j = i + 1; j < Math.Min(i + 15, lines.Length); j++)
                        {
                            string nextLine = lines[j].Trim();
                            if (nextLine.StartsWith("[Маг") || nextLine.StartsWith("[маг"))
                            {
                                int startBracket = nextLine.IndexOf('[');
                                int endBracket = nextLine.IndexOf(']');
                                if (startBracket >= 0 && endBracket > startBracket)
                                {
                                    string bracketContent = nextLine.Substring(startBracket + 1, endBracket - startBracket - 1);
                                    int colonIdx = bracketContent.IndexOf(':');
                                    if (colonIdx >= 0)
                                    {
                                        energyRaw = bracketContent.Substring(colonIdx + 1).Trim();
                                    }
                                    else
                                    {
                                        energyRaw = bracketContent.Trim();
                                    }
                                }
                                break;
                            }
                        }

                        File.AppendAllText("import_log.txt", "  Energy raw: " + energyRaw + "\n");

                        if (!insertedIds.Contains(id))
                        {
                            InsertCharacter(connection, transaction, id, name, rarityUa, roleUa, basePower);

                            if (!string.IsNullOrEmpty(energyRaw))
                            {
                                string[] energies = energyRaw.Split(',');
                                foreach (string en in energies)
                                {
                                    string enClean = en.Trim();
                                    string enUa = Translate(enClean, energyTranslation);
                                    File.AppendAllText("import_log.txt", "  Translated: '" + enClean + "' -> '" + enUa + "'\n");
                                    InsertEnergy(connection, transaction, id, enUa);
                                }
                            }

                            insertedIds.Add(id);
                            foundCount++;
                        }
                        else
                        {
                            File.AppendAllText("import_log.txt", "  Skipping duplicate ID=" + id + "\n");
                        }
                    }
                }

                File.AppendAllText("import_log.txt", "Found " + foundCount + " character entries in file\n");

                transaction.Commit();
                File.AppendAllText("import_log.txt", "Import completed successfully\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText("import_log.txt", "Error during import: " + ex.Message + "\n");
                transaction.Rollback();
                throw;
            }
        }

        private string Translate(string input, Dictionary<string, string> dict)
        {
            string inputLower = input.ToLower();
            foreach (var kvp in dict)
            {
                if (kvp.Key.ToLower() == inputLower)
                {
                    return kvp.Value;
                }
            }
            return input;
        }

        private int CalculateBasePower(string rarity)
        {
            switch (rarity)
            {
                case "Рідкісний": return 55;
                case "Епічний": return 65;
                case "Легендарний": return 72;
                case "Королівський": return 78;
                default: return 50;
            }
        }

        private void InsertCharacter(SqlConnection conn, SqlTransaction tran, int id, string name, string rarity, string role, int basePower)
        {
            File.AppendAllText("import_log.txt", "Inserting character: ID=" + id + ", Name=" + name + ", Rarity=" + rarity + ", Role=" + role + "\n");
            string sql = "INSERT INTO Characters (Id, Name, Rarity, PrimaryRole, BasePower, Skill, Ultimate, ShortDescription, Lore) " +
                        "VALUES (@Id, @Name, @Rarity, @Role, @BP, @Skill, @Ult, @Short, @Lore)";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Rarity", rarity);
                cmd.Parameters.AddWithValue("@Role", role);
                cmd.Parameters.AddWithValue("@BP", basePower);
                cmd.Parameters.AddWithValue("@Skill", "Parsed from source");
                cmd.Parameters.AddWithValue("@Ult", "Parsed from source");
                cmd.Parameters.AddWithValue("@Short", "Character " + name);
                cmd.Parameters.AddWithValue("@Lore", "Lore for " + name);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertEnergy(SqlConnection conn, SqlTransaction tran, int charId, string energy)
        {
            File.AppendAllText("import_log.txt", "Inserting energy: '" + energy + "' for charId=" + charId + "\n");
            string sql = "INSERT INTO CharacterEnergies (CharId, EnergyType) VALUES (@CharId, @Energy)";
            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@CharId", charId);
                cmd.Parameters.AddWithValue("@Energy", energy);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
