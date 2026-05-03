using System.Collections.Generic;

namespace HeroDrafter.Models
{
    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Rarity Rarity { get; set; }
        public PrimaryRole PrimaryRole { get; set; }
        public int BasePower { get; set; }
        public string Skill { get; set; }
        public string Ultimate { get; set; }
        public string ShortDescription { get; set; }
        public string Lore { get; set; }
        public List<string> CombatStyles { get; set; } = new List<string>();
        public List<string> Energies { get; set; } = new List<string>();
        public List<string> Passives { get; set; } = new List<string>();
    }
}
