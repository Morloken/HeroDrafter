using System.Collections.Generic;
using HeroDrafter.Models;

namespace HeroDrafter.Business
{
    public class DraftAnalyzer
    {
        public int CalculateTeamBalance(List<Character> myTeam)
        {
            int bonus = 0;
            bool hasTankBruiser = false;
            bool hasSupport = false;
            bool hasDamageDealer = false;

            foreach (Character c in myTeam)
            {
                if (c.PrimaryRole == PrimaryRole.Танк || c.PrimaryRole == PrimaryRole.Гібрид)
                    hasTankBruiser = true;
                if (c.PrimaryRole == PrimaryRole.Підтримка)
                    hasSupport = true;
                if (c.PrimaryRole == PrimaryRole.Маг || c.PrimaryRole == PrimaryRole.Стрілець || c.PrimaryRole == PrimaryRole.Вбивця)
                    hasDamageDealer = true;
            }

            if (hasTankBruiser && hasSupport && hasDamageDealer)
                bonus += 20;
            else if (!hasTankBruiser && !hasSupport)
                bonus -= 15;

            return bonus;
        }

        public int CalculateCounterPicks(List<Character> myTeam, List<Character> enemyTeam)
        {
            int total = 0;
            foreach (Character ally in myTeam)
            {
                foreach (Character enemy in enemyTeam)
                {
                    if (ally.PrimaryRole == PrimaryRole.Вбивця &&
                        (enemy.PrimaryRole == PrimaryRole.Підтримка || enemy.PrimaryRole == PrimaryRole.Стрілець))
                        total += 10;
                    if ((ally.PrimaryRole == PrimaryRole.Танк || ally.PrimaryRole == PrimaryRole.Гібрид) &&
                        enemy.PrimaryRole == PrimaryRole.Вбивця)
                        total += 10;
                    if ((ally.PrimaryRole == PrimaryRole.Стрілець || ally.PrimaryRole == PrimaryRole.Маг) &&
                        enemy.PrimaryRole == PrimaryRole.Танк)
                        total += 10;
                }
            }
            return total;
        }

        public int CalculateRarityBonus(List<Character> myTeam)
        {
            int total = 0;
            foreach (Character c in myTeam)
            {
                switch (c.Rarity)
                {
                    case Rarity.Рідкісний: total += 1; break;
                    case Rarity.Епічний: total += 3; break;
                    case Rarity.Легендарний: total += 5; break;
                    case Rarity.Королівський: total += 8; break;
                }
            }
            return total;
        }

        public int CalculateTotalEfficiency(List<Character> myTeam, List<Character> enemyTeam)
        {
            int baseSum = 0;
            foreach (Character c in myTeam) baseSum += c.BasePower;
            return baseSum + CalculateTeamBalance(myTeam) + CalculateCounterPicks(myTeam, enemyTeam) + CalculateRarityBonus(myTeam);
        }
    }
}
