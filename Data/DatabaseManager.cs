using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using HeroDrafter.Models;

namespace HeroDrafter.Data
{
    public class DatabaseManager
    {
        private string GetConnectionString()
        {
            string connStr = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connStr))
            {
                connStr = "Server=(localdb)\\mssqllocaldb;Integrated Security=true;Database=HeroDrafterDB;";
                Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", connStr);
            }
            return connStr;
        }

        public SqlConnection CreateOpenConnection()
        {
            var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            return conn;
        }

        public void InitializeDatabase()
        {
            if (!DatabaseExists())
            {
                CreateDatabase();
            }
            CreateTablesIfNotExist();
        }

        private bool DatabaseExists()
        {
            using (var masterConn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Integrated Security=true;Database=master;"))
            {
                masterConn.Open();
                string checkDbSql = "SELECT COUNT(*) FROM master..sysdatabases WHERE name = 'HeroDrafterDB'";
                using (var cmd = new SqlCommand(checkDbSql, masterConn))
                {
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private void CreateDatabase()
        {
            string baseDir = AppContext.BaseDirectory;
            string mdfPath = Path.Combine(baseDir, "HeroDrafterDB.mdf");
            string ldfPath = Path.Combine(baseDir, "HeroDrafterDB_log.ldf");

            using (var masterConn = new SqlConnection("Server=(localdb)\\mssqllocaldb;Integrated Security=true;Database=master;"))
            {
                masterConn.Open();
                string createDbSql = $@"
                    CREATE DATABASE HeroDrafterDB
                    ON (NAME = HeroDrafterDB, FILENAME = '{mdfPath}')
                    LOG ON (NAME = HeroDrafterDB_log, FILENAME = '{ldfPath}')";
                using (var cmd = new SqlCommand(createDbSql, masterConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CreateTablesIfNotExist()
        {
            using (var conn = CreateOpenConnection())
            {
                string script = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Characters') AND type in (N'U'))
                    CREATE TABLE Characters (
                        Id INT PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Rarity NVARCHAR(50) NOT NULL,
                        PrimaryRole NVARCHAR(50) NOT NULL,
                        BasePower INT NOT NULL,
                        Skill NVARCHAR(MAX) NOT NULL,
                        Ultimate NVARCHAR(MAX) NOT NULL,
                        ShortDescription NVARCHAR(MAX) NOT NULL,
                        Lore NVARCHAR(MAX) NOT NULL
                    );

                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'CharacterCombatStyles') AND type in (N'U'))
                    CREATE TABLE CharacterCombatStyles (
                        CharId INT NOT NULL,
                        StyleName NVARCHAR(50) NOT NULL,
                        CONSTRAINT PK_CharacterCombatStyles PRIMARY KEY (CharId, StyleName),
                        CONSTRAINT FK_CharacterCombatStyles_Characters FOREIGN KEY (CharId) REFERENCES Characters(Id) ON DELETE CASCADE
                    );

                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'CharacterEnergies') AND type in (N'U'))
                    CREATE TABLE CharacterEnergies (
                        CharId INT NOT NULL,
                        EnergyType NVARCHAR(50) NOT NULL,
                        CONSTRAINT PK_CharacterEnergies PRIMARY KEY (CharId, EnergyType),
                        CONSTRAINT FK_CharacterEnergies_Characters FOREIGN KEY (CharId) REFERENCES Characters(Id) ON DELETE CASCADE
                    );

                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'CharacterPassives') AND type in (N'U'))
                    CREATE TABLE CharacterPassives (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        CharId INT NOT NULL,
                        PassiveDescription NVARCHAR(MAX) NOT NULL,
                        CONSTRAINT FK_CharacterPassives_Characters FOREIGN KEY (CharId) REFERENCES Characters(Id) ON DELETE CASCADE
                    );";
                using (var cmd = new SqlCommand(script, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetCharacterCount()
        {
            using (var conn = CreateOpenConnection())
            {
                string sql = "SELECT COUNT(*) FROM Characters";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public List<Character> GetAllCharacters()
        {
            List<Character> list = new List<Character>();

            using (SqlConnection conn = CreateOpenConnection())
            {
                string sql = "SELECT Id, Name, Rarity, PrimaryRole, BasePower, Skill, Ultimate, ShortDescription, Lore FROM Characters";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Character c = new Character();
                            c.Id = reader.GetInt32(0);
                            c.Name = reader.GetString(1);
                            c.Rarity = (Rarity)Enum.Parse(typeof(Rarity), reader.GetString(2));
                            c.PrimaryRole = (PrimaryRole)Enum.Parse(typeof(PrimaryRole), reader.GetString(3));
                            c.BasePower = reader.GetInt32(4);
                            c.Skill = reader.GetString(5);
                            c.Ultimate = reader.GetString(6);
                            c.ShortDescription = reader.GetString(7);
                            c.Lore = reader.GetString(8);

                            list.Add(c);
                        }
                    }

                    // Now load related data after the DataReader is closed
                    foreach (Character c in list)
                    {
                        LoadCombatStyles(conn, c);
                        LoadEnergies(conn, c);
                        LoadPassives(conn, c);
                    }
                }
            }

            return list;
        }

        private void LoadCombatStyles(SqlConnection conn, Character c)
        {
            string sql = "SELECT StyleName FROM CharacterCombatStyles WHERE CharId = @Id";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", c.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        c.CombatStyles.Add(reader.GetString(0));
                    }
                }
            }
        }

        private void LoadEnergies(SqlConnection conn, Character c)
        {
            string sql = "SELECT EnergyType FROM CharacterEnergies WHERE CharId = @Id";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", c.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        c.Energies.Add(reader.GetString(0));
                    }
                }
            }
        }

        private void LoadPassives(SqlConnection conn, Character c)
        {
            string sql = "SELECT PassiveDescription FROM CharacterPassives WHERE CharId = @Id";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", c.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        c.Passives.Add(reader.GetString(0));
                    }
                }
            }
        }

        public void ExportDraftReport(List<Character> allies, List<Character> enemies, int totalScore)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DraftReport.txt");

            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("ЗВІТ ПО ДРАФТУ");
                writer.WriteLine("===================");
                writer.WriteLine();
                writer.WriteLine("СОЮЗНИКИ:");
                foreach (Character c in allies)
                {
                    writer.WriteLine("- " + c.Name + " (Сила: " + c.BasePower + ")");
                }
                writer.WriteLine();
                writer.WriteLine("ВОРОГИ:");
                foreach (Character c in enemies)
                {
                    writer.WriteLine("- " + c.Name + " (Сила: " + c.BasePower + ")");
                }
                writer.WriteLine();
                writer.WriteLine("ЗАГАЛЬНА ЕФЕКТИВНІСТЬ: " + totalScore);
            }
        }

        public int InsertCharacter(Character c)
        {
            using (var conn = CreateOpenConnection())
            {
                int newId = GetNextCharacterId();
                string sql = @"INSERT INTO Characters (Id, Name, Rarity, PrimaryRole, BasePower, Skill, Ultimate, ShortDescription, Lore)
                              VALUES (@Id, @Name, @Rarity, @PrimaryRole, @BasePower, @Skill, @Ultimate, @ShortDescription, @Lore)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", newId);
                    cmd.Parameters.AddWithValue("@Name", c.Name);
                    cmd.Parameters.AddWithValue("@Rarity", c.Rarity.ToString());
                    cmd.Parameters.AddWithValue("@PrimaryRole", c.PrimaryRole.ToString());
                    cmd.Parameters.AddWithValue("@BasePower", c.BasePower);
                    cmd.Parameters.AddWithValue("@Skill", c.Skill ?? "");
                    cmd.Parameters.AddWithValue("@Ultimate", c.Ultimate ?? "");
                    cmd.Parameters.AddWithValue("@ShortDescription", c.ShortDescription ?? "");
                    cmd.Parameters.AddWithValue("@Lore", c.Lore ?? "");
                    cmd.ExecuteNonQuery();
                }

                foreach (var style in c.CombatStyles)
                {
                    InsertCombatStyle(conn, newId, style);
                }
                foreach (var energy in c.Energies)
                {
                    InsertEnergy(conn, newId, energy);
                }
                foreach (var passive in c.Passives)
                {
                    InsertPassive(conn, newId, passive);
                }

                return newId;
            }
        }

        private int GetNextCharacterId()
        {
            using (var conn = CreateOpenConnection())
            {
                string sql = "SELECT ISNULL(MAX(Id), 0) + 1 FROM Characters";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 1 : Convert.ToInt32(result);
                }
            }
        }

        private void InsertCombatStyle(SqlConnection conn, int charId, string styleName)
        {
            string sql = "INSERT INTO CharacterCombatStyles (CharId, StyleName) VALUES (@CharId, @StyleName)";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CharId", charId);
                cmd.Parameters.AddWithValue("@StyleName", styleName);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertEnergy(SqlConnection conn, int charId, string energyType)
        {
            string sql = "INSERT INTO CharacterEnergies (CharId, EnergyType) VALUES (@CharId, @EnergyType)";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CharId", charId);
                cmd.Parameters.AddWithValue("@EnergyType", energyType);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertPassive(SqlConnection conn, int charId, string passiveDesc)
        {
            string sql = "INSERT INTO CharacterPassives (CharId, PassiveDescription) VALUES (@CharId, @PassiveDescription)";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CharId", charId);
                cmd.Parameters.AddWithValue("@PassiveDescription", passiveDesc);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateCharacter(Character c)
        {
            using (var conn = CreateOpenConnection())
            {
                string sql = @"UPDATE Characters SET Name = @Name, Rarity = @Rarity, PrimaryRole = @PrimaryRole,
                              BasePower = @BasePower, Skill = @Skill, Ultimate = @Ultimate,
                              ShortDescription = @ShortDescription, Lore = @Lore WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", c.Id);
                    cmd.Parameters.AddWithValue("@Name", c.Name);
                    cmd.Parameters.AddWithValue("@Rarity", c.Rarity.ToString());
                    cmd.Parameters.AddWithValue("@PrimaryRole", c.PrimaryRole.ToString());
                    cmd.Parameters.AddWithValue("@BasePower", c.BasePower);
                    cmd.Parameters.AddWithValue("@Skill", c.Skill ?? "");
                    cmd.Parameters.AddWithValue("@Ultimate", c.Ultimate ?? "");
                    cmd.Parameters.AddWithValue("@ShortDescription", c.ShortDescription ?? "");
                    cmd.Parameters.AddWithValue("@Lore", c.Lore ?? "");
                    cmd.ExecuteNonQuery();
                }

                ClearCombatStyles(conn, c.Id);
                ClearEnergies(conn, c.Id);
                ClearPassives(conn, c.Id);

                foreach (var style in c.CombatStyles)
                {
                    InsertCombatStyle(conn, c.Id, style);
                }
                foreach (var energy in c.Energies)
                {
                    InsertEnergy(conn, c.Id, energy);
                }
                foreach (var passive in c.Passives)
                {
                    InsertPassive(conn, c.Id, passive);
                }
            }
        }

        private void ClearCombatStyles(SqlConnection conn, int charId)
        {
            string sql = "DELETE FROM CharacterCombatStyles WHERE CharId = @CharId";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CharId", charId);
                cmd.ExecuteNonQuery();
            }
        }

        private void ClearEnergies(SqlConnection conn, int charId)
        {
            string sql = "DELETE FROM CharacterEnergies WHERE CharId = @CharId";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CharId", charId);
                cmd.ExecuteNonQuery();
            }
        }

        private void ClearPassives(SqlConnection conn, int charId)
        {
            string sql = "DELETE FROM CharacterPassives WHERE CharId = @CharId";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CharId", charId);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteCharacter(int charId)
        {
            using (var conn = CreateOpenConnection())
            {
                string sql = "DELETE FROM Characters WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", charId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
