using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeroDrafter.Models;

namespace HeroDrafter.Business
{
    public class DraftAnalyzer
    {
        public int CalculateTeamBalance(List<Character> team)
        {
            int tankCount = team.Count(c => c.PrimaryRole == PrimaryRole.Танк);
            int supportCount = team.Count(c => c.PrimaryRole == PrimaryRole.Підтримка);
            int damageCount = team.Count(c => c.PrimaryRole == PrimaryRole.Маг || c.PrimaryRole == PrimaryRole.Стрілець || c.PrimaryRole == PrimaryRole.Вбивця);
            int hybridCount = team.Count(c => c.PrimaryRole == PrimaryRole.Гібрид);
            int assassinCount = team.Count(c => c.PrimaryRole == PrimaryRole.Вбивця);

            int bonus = 0;

            bool hasTank = tankCount > 0 || hybridCount > 0;
            bool hasSupport = supportCount > 0 || hybridCount > 0;
            bool hasDamage = damageCount > 0;

            // Core balance: tank + support + damage = ideal
            if (hasTank && hasSupport && hasDamage)
                bonus += 25;
            else if (hasTank && hasDamage)
                bonus += 10;
            else if (hasSupport && hasDamage)
                bonus += 10;
            else if (!hasTank && !hasSupport)
                bonus -= 20;

            // Role count bonuses
            if (tankCount >= 1) bonus += 5;
            if (supportCount >= 1) bonus += 5;
            if (assassinCount >= 1) bonus += 3;

            // Synergy: tank + support = frontline protection
            if (tankCount >= 1 && supportCount >= 1)
                bonus += 8;

            // Too many of same role penalty
            if (team.Count(c => c.PrimaryRole == PrimaryRole.Вбивця) >= 2)
                bonus -= 8;
            if (team.Count(c => c.PrimaryRole == PrimaryRole.Маг) >= 2)
                bonus -= 5;

            return bonus;
        }

        public int CalculateCounterPicks(List<Character> myTeam, List<Character> enemyTeam)
        {
            if (myTeam.Count == 0 || enemyTeam.Count == 0) return 0;

            int total = 0;

            foreach (Character ally in myTeam)
            {
                foreach (Character enemy in enemyTeam)
                {
                    // Assassins counter Supports and Mages
                    if (ally.PrimaryRole == PrimaryRole.Вбивця &&
                        (enemy.PrimaryRole == PrimaryRole.Підтримка || enemy.PrimaryRole == PrimaryRole.Маг))
                        total += 12;

                    // Tanks counter Assassins
                    if ((ally.PrimaryRole == PrimaryRole.Танк || ally.PrimaryRole == PrimaryRole.Гібрид) &&
                        enemy.PrimaryRole == PrimaryRole.Вбивця)
                        total += 12;

                    // Ranged (Стрілець, Маг) counter Tanks
                    if ((ally.PrimaryRole == PrimaryRole.Стрілець || ally.PrimaryRole == PrimaryRole.Маг) &&
                        enemy.PrimaryRole == PrimaryRole.Танк)
                        total += 12;

                    // Support counter Mages and Strelts (healing vs sustained)
                    if (ally.PrimaryRole == PrimaryRole.Підтримка &&
                        enemy.PrimaryRole == PrimaryRole.Маг)
                        total += 8;

                    // Assassins counter each other (duel)
                    if (ally.PrimaryRole == PrimaryRole.Вбивця && enemy.PrimaryRole == PrimaryRole.Вбивця)
                        total -= 5;
                }
            }

            return total;
        }

        public int CalculateRarityBonus(List<Character> team)
        {
            int total = 0;
            foreach (Character c in team)
            {
                switch (c.Rarity)
                {
                    case Rarity.Рідкісний: total += 5; break;
                    case Rarity.Епічний: total += 10; break;
                    case Rarity.Легендарний: total += 18; break;
                    case Rarity.Королівський: total += 25; break;
                }
            }
            return total;
        }

        public int CalculateEnergySynergy(List<Character> team)
        {
            if (team.Count == 0) return 0;

            var energies = team.Where(c => c.Energies != null)
                               .SelectMany(c => c.Energies)
                               .Where(e => !string.IsNullOrWhiteSpace(e))
                               .Distinct()
                               .ToList();

            int bonus = 0;
            // Having 2+ different energy types = versatility bonus
            if (energies.Count >= 2) bonus += 5 + energies.Count * 2;
            if (energies.Count >= 3) bonus += 5;
            if (energies.Count >= 4) bonus += 5;

            return bonus;
        }

        public string GetRoleDistribution(List<Character> team)
        {
            if (team.Count == 0) return "—";

            var counts = new Dictionary<string, int>();
            foreach (var c in team)
            {
                string role = c.PrimaryRole.ToString();
                if (counts.ContainsKey(role)) counts[role]++;
                else counts[role] = 1;
            }

            var parts = counts.OrderByDescending(kv => kv.Value)
                              .Select(kv => $"{kv.Key} x{kv.Value}");
            return string.Join(", ", parts);
        }

        public string GetEnergySummary(List<Character> team)
        {
            var energies = team.Where(c => c.Energies != null)
                               .SelectMany(c => c.Energies)
                               .Where(e => !string.IsNullOrWhiteSpace(e))
                               .Distinct()
                               .ToList();

            return energies.Count > 0 ? string.Join(", ", energies) : "Відсутні";
        }

        public int CalculateMissingRolePenalty(List<Character> team)
        {
            int penalty = 0;
            bool hasTank = team.Any(c => c.PrimaryRole == PrimaryRole.Танк || c.PrimaryRole == PrimaryRole.Гібрид);
            bool hasSupport = team.Any(c => c.PrimaryRole == PrimaryRole.Підтримка);
            bool hasDamage = team.Any(c => c.PrimaryRole == PrimaryRole.Маг || c.PrimaryRole == PrimaryRole.Стрілець || c.PrimaryRole == PrimaryRole.Вбивця);

            if (!hasTank) penalty -= 15;
            if (!hasSupport) penalty -= 10;
            if (!hasDamage) penalty -= 20;

            return penalty;
        }

        public int CalculateTotalEfficiency(List<Character> myTeam, List<Character> enemyTeam)
        {
            int baseSum = myTeam.Sum(c => c.BasePower);
            return baseSum
                   + CalculateTeamBalance(myTeam)
                   + CalculateCounterPicks(myTeam, enemyTeam)
                   + CalculateRarityBonus(myTeam)
                   + CalculateEnergySynergy(myTeam)
                   + CalculateMissingRolePenalty(myTeam);
        }

        public string GetTeamBreakdown(List<Character> team, List<Character> enemyTeam)
        {
            if (team.Count == 0) return "—";

            var sb = new StringBuilder();

            int basePower = team.Sum(c => c.BasePower);
            int balance = CalculateTeamBalance(team);
            int counter = CalculateCounterPicks(team, enemyTeam);
            int rarity = CalculateRarityBonus(team);
            int energy = CalculateEnergySynergy(team);
            int penalty = CalculateMissingRolePenalty(team);

            sb.AppendLine($"📊 БАЗОВА СИЛА: +{basePower}");
            sb.AppendLine($"⚖️  БАЛАНС КОМАНДИ: {(balance >= 0 ? "+" : "")}{balance}");
            sb.AppendLine($"🎯 КОНТР-ПІКИ: {(counter >= 0 ? "+" : "")}{counter}");
            sb.AppendLine($"💎 РІДКІСТЬ: +{rarity}");
            sb.AppendLine($"⚡ ЕНЕРГІЇ: +{energy}");
            sb.AppendLine($"⚠️  ШТРАФИ: {penalty}");

            int total = basePower + balance + counter + rarity + energy + penalty;
            sb.AppendLine($"🏆 ЗАГАЛОМ: {total}");

            return sb.ToString();
        }
    }
}
