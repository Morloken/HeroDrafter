using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HeroDrafter.Data;
using HeroDrafter.Business;
using HeroDrafter.Models;
using HeroDrafter.Commands;
using System.ComponentModel;

namespace HeroDrafter.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager;
        private readonly DraftAnalyzer _analyzer = new DraftAnalyzer();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ObservableCollection<Character> AllCharacters { get; } = new ObservableCollection<Character>();
        
        private Character _selectedCharacter;
        public Character SelectedCharacter
        {
            get => _selectedCharacter;
            set { _selectedCharacter = value; OnPropertyChanged(nameof(SelectedCharacter)); }
        }

        public ObservableCollection<Character> AllySlots { get; } = new ObservableCollection<Character>(new Character[3]);
        public ObservableCollection<Character> EnemySlots { get; } = new ObservableCollection<Character>(new Character[3]);

        private int _totalBasePower;
        public int TotalBasePower
        {
            get => _totalBasePower;
            set { _totalBasePower = value; OnPropertyChanged(nameof(TotalBasePower)); }
        }

        private int _balanceBonus;
        public int BalanceBonus
        {
            get => _balanceBonus;
            set { _balanceBonus = value; OnPropertyChanged(nameof(BalanceBonus)); }
        }

        private int _counterPickBonus;
        public int CounterPickBonus
        {
            get => _counterPickBonus;
            set { _counterPickBonus = value; OnPropertyChanged(nameof(CounterPickBonus)); }
        }

        private int _rarityBonus;
        public int RarityBonus
        {
            get => _rarityBonus;
            set { _rarityBonus = value; OnPropertyChanged(nameof(RarityBonus)); }
        }

        private int _totalEfficiency;
        public int TotalEfficiency
        {
            get => _totalEfficiency;
            set { _totalEfficiency = value; OnPropertyChanged(nameof(TotalEfficiency)); }
        }

        public ICommand AssignAllyCommand { get; }
        public ICommand AssignEnemyCommand { get; }
        public ICommand ClearAllyCommand { get; }
        public ICommand ClearEnemyCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand AddCharacterCommand { get; }
        public ICommand EditCharacterCommand { get; }
        public ICommand DeleteCharacterCommand { get; }
        public ICommand RefreshCommand { get; }

        public event Action<Character> OpenAddEditWindow;

        public MainViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            LoadCharacters();

            AssignAllyCommand = new RelayCommand(_ => AssignToSlot(AllySlots, SelectedCharacter));
            AssignEnemyCommand = new RelayCommand(_ => AssignToSlot(EnemySlots, SelectedCharacter));
            ClearAllyCommand = new RelayCommand(_ => ClearSlot(AllySlots, null));
            ClearEnemyCommand = new RelayCommand(_ => ClearSlot(EnemySlots, null));
            ExportReportCommand = new RelayCommand(_ => ExportReport());
            AddCharacterCommand = new RelayCommand(() => OpenAddEditWindow?.Invoke(null));
            EditCharacterCommand = new RelayCommand(() => OpenAddEditWindow?.Invoke(SelectedCharacter));
            DeleteCharacterCommand = new RelayCommand(() => DeleteCharacter(SelectedCharacter));
            RefreshCommand = new RelayCommand(() => RefreshCharacters());

            AllySlots.CollectionChanged += (s, e) => UpdateAnalytics();
            EnemySlots.CollectionChanged += (s, e) => UpdateAnalytics();
        }

        private void LoadCharacters()
        {
            var chars = _dbManager.GetAllCharacters();
            AllCharacters.Clear();
            foreach (var c in chars) AllCharacters.Add(c);
        }

        private void RefreshCharacters()
        {
            LoadCharacters();
        }

        private void DeleteCharacter(Character character)
        {
            if (character == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Видалити персонажа '{character.Name}'?",
                "Підтвердження",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _dbManager.DeleteCharacter(character.Id);
                RefreshCharacters();
            }
        }

        private void AssignToSlot(ObservableCollection<Character> slots, Character character)
        {
            if (character == null) return;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = character;
                    break;
                }
            }
        }

        private void ClearSlot(ObservableCollection<Character> slots, object param)
        {
            if (param is int index && index >= 0 && index < slots.Count)
            {
                slots[index] = null;
            }
        }

        private void UpdateAnalytics()
        {
            var allies = AllySlots.Where(c => c != null).ToList();
            var enemies = EnemySlots.Where(c => c != null).ToList();

            TotalBasePower = allies.Sum(c => c.BasePower);
            BalanceBonus = _analyzer.CalculateTeamBalance(allies);
            CounterPickBonus = _analyzer.CalculateCounterPicks(allies, enemies);
            RarityBonus = _analyzer.CalculateRarityBonus(allies);
            TotalEfficiency = _analyzer.CalculateTotalEfficiency(allies, enemies);
        }

        private void ExportReport()
        {
            var allies = AllySlots.Where(c => c != null).ToList();
            var enemies = EnemySlots.Where(c => c != null).ToList();
            _dbManager.ExportDraftReport(allies, enemies, TotalEfficiency);
        }
    }
}
