using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HeroDrafter.Commands;
using HeroDrafter.Data;
using HeroDrafter.Models;

namespace HeroDrafter.ViewModels
{
    public class AddEditCharacterViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager;
        private readonly Character _originalCharacter;
        private readonly bool _isEditMode;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public event Action RequestClose;

        public AddEditCharacterViewModel(DatabaseManager dbManager, Character characterToEdit = null)
        {
            _dbManager = dbManager;
            _originalCharacter = characterToEdit;
            _isEditMode = characterToEdit != null;

            AllRarities = Enum.GetValues(typeof(Rarity)).Cast<Rarity>().ToList();
            AllRoles = Enum.GetValues(typeof(PrimaryRole)).Cast<PrimaryRole>().ToList();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);

            if (_isEditMode)
            {
                LoadCharacter(characterToEdit);
            }
        }

        public string WindowTitle => _isEditMode ? "Редагувати Персонажа" : "Додати Нового Персонажа";

        public List<Rarity> AllRarities { get; }
        public List<PrimaryRole> AllRoles { get; }

        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private Rarity _selectedRarity = Rarity.Звичайний;
        public Rarity SelectedRarity
        {
            get => _selectedRarity;
            set { _selectedRarity = value; OnPropertyChanged(); }
        }

        private PrimaryRole _selectedRole = PrimaryRole.Гібрид;
        public PrimaryRole SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        private int _basePower = 50;
        public int BasePower
        {
            get => _basePower;
            set { _basePower = value; OnPropertyChanged(); }
        }

        private string _combatStylesText = "";
        public string CombatStylesText
        {
            get => _combatStylesText;
            set { _combatStylesText = value; OnPropertyChanged(); }
        }

        private string _energiesText = "";
        public string EnergiesText
        {
            get => _energiesText;
            set { _energiesText = value; OnPropertyChanged(); }
        }

        private string _skill = "";
        public string Skill
        {
            get => _skill;
            set { _skill = value; OnPropertyChanged(); }
        }

        private string _ultimate = "";
        public string Ultimate
        {
            get => _ultimate;
            set { _ultimate = value; OnPropertyChanged(); }
        }

        private string _shortDescription = "";
        public string ShortDescription
        {
            get => _shortDescription;
            set { _shortDescription = value; OnPropertyChanged(); }
        }

        private string _lore = "";
        public string Lore
        {
            get => _lore;
            set { _lore = value; OnPropertyChanged(); }
        }

        private string _passivesText = "";
        public string PassivesText
        {
            get => _passivesText;
            set { _passivesText = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void LoadCharacter(Character c)
        {
            Name = c.Name;
            SelectedRarity = c.Rarity;
            SelectedRole = c.PrimaryRole;
            BasePower = c.BasePower;
            CombatStylesText = string.Join(", ", c.CombatStyles.Where(s => !string.IsNullOrWhiteSpace(s)));
            EnergiesText = string.Join(", ", c.Energies.Where(e => !string.IsNullOrWhiteSpace(e)));
            Skill = c.Skill;
            Ultimate = c.Ultimate;
            ShortDescription = c.ShortDescription;
            Lore = c.Lore;
            PassivesText = string.Join(", ", c.Passives.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                System.Windows.MessageBox.Show("Введіть назву персонажа!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var character = new Character
            {
                Name = Name,
                Rarity = SelectedRarity,
                PrimaryRole = SelectedRole,
                BasePower = BasePower,
                Skill = Skill,
                Ultimate = Ultimate,
                ShortDescription = ShortDescription,
                Lore = Lore,
                CombatStyles = ParseList(CombatStylesText),
                Energies = ParseList(EnergiesText),
                Passives = ParseList(PassivesText)
            };

            if (_isEditMode)
            {
                character.Id = _originalCharacter.Id;
                _dbManager.UpdateCharacter(character);
            }
            else
            {
                _dbManager.InsertCharacter(character);
            }

            RequestClose?.Invoke();
        }

        private List<string> ParseList(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim())
                      .Where(s => !string.IsNullOrWhiteSpace(s))
                      .ToList();
        }

        private void Cancel()
        {
            RequestClose?.Invoke();
        }
    }
}