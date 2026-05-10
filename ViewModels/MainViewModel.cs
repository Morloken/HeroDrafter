using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

        private int _selectedAllySlot = -1;
        public int SelectedAllySlot
        {
            get => _selectedAllySlot;
            set { _selectedAllySlot = value; OnPropertyChanged(nameof(SelectedAllySlot)); }
        }

        private int _selectedEnemySlot = -1;
        public int SelectedEnemySlot
        {
            get => _selectedEnemySlot;
            set { _selectedEnemySlot = value; OnPropertyChanged(nameof(SelectedEnemySlot)); }
        }

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

        private int _enemyTotalBasePower;
        public int EnemyTotalBasePower
        {
            get => _enemyTotalBasePower;
            set { _enemyTotalBasePower = value; OnPropertyChanged(nameof(EnemyTotalBasePower)); }
        }

        private int _enemyBalanceBonus;
        public int EnemyBalanceBonus
        {
            get => _enemyBalanceBonus;
            set { _enemyBalanceBonus = value; OnPropertyChanged(nameof(EnemyBalanceBonus)); }
        }

        private int _enemyCounterPickBonus;
        public int EnemyCounterPickBonus
        {
            get => _enemyCounterPickBonus;
            set { _enemyCounterPickBonus = value; OnPropertyChanged(nameof(EnemyCounterPickBonus)); }
        }

        private int _enemyRarityBonus;
        public int EnemyRarityBonus
        {
            get => _enemyRarityBonus;
            set { _enemyRarityBonus = value; OnPropertyChanged(nameof(EnemyRarityBonus)); }
        }

        private int _enemyTotalEfficiency;
        public int EnemyTotalEfficiency
        {
            get => _enemyTotalEfficiency;
            set { _enemyTotalEfficiency = value; OnPropertyChanged(nameof(EnemyTotalEfficiency)); }
        }

        private string _strongerParty = "";
        public string StrongerParty
        {
            get => _strongerParty;
            set { _strongerParty = value; OnPropertyChanged(nameof(StrongerParty)); }
        }

        private string _allyRoleDist = "";
        public string AllyRoleDist
        {
            get => _allyRoleDist;
            set { _allyRoleDist = value; OnPropertyChanged(nameof(AllyRoleDist)); }
        }

        private string _enemyRoleDist = "";
        public string EnemyRoleDist
        {
            get => _enemyRoleDist;
            set { _enemyRoleDist = value; OnPropertyChanged(nameof(EnemyRoleDist)); }
        }

        private string _allyEnergySum = "";
        public string AllyEnergySum
        {
            get => _allyEnergySum;
            set { _allyEnergySum = value; OnPropertyChanged(nameof(AllyEnergySum)); }
        }

        private string _enemyEnergySum = "";
        public string EnemyEnergySum
        {
            get => _enemyEnergySum;
            set { _enemyEnergySum = value; OnPropertyChanged(nameof(EnemyEnergySum)); }
        }

        private int _allyEnergyBonus;
        public int AllyEnergyBonus
        {
            get => _allyEnergyBonus;
            set { _allyEnergyBonus = value; OnPropertyChanged(nameof(AllyEnergyBonus)); }
        }

        private int _enemyEnergyBonus;
        public int EnemyEnergyBonus
        {
            get => _enemyEnergyBonus;
            set { _enemyEnergyBonus = value; OnPropertyChanged(nameof(EnemyEnergyBonus)); }
        }

        private int _allyRolePenalty;
        public int AllyRolePenalty
        {
            get => _allyRolePenalty;
            set { _allyRolePenalty = value; OnPropertyChanged(nameof(AllyRolePenalty)); }
        }

        private int _enemyRolePenalty;
        public int EnemyRolePenalty
        {
            get => _enemyRolePenalty;
            set { _enemyRolePenalty = value; OnPropertyChanged(nameof(EnemyRolePenalty)); }
        }

        public ICommand AssignAllyCommand { get; }
        public ICommand AssignEnemyCommand { get; }
        public ICommand SelectAllySlotCommand { get; }
        public ICommand SelectEnemySlotCommand { get; }
        public ICommand RemoveSelectedAllyCommand { get; }
        public ICommand RemoveSelectedEnemyCommand { get; }
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
            
            SelectAllySlotCommand = new RelayCommand(param => 
            {
                int index = ParseInt(param);
                if (index >= 0 && index < AllySlots.Count && AllySlots[index] != null)
                {
                    SelectedAllySlot = index;
                    SelectedEnemySlot = -1;
                }
            });
            
            SelectEnemySlotCommand = new RelayCommand(param => 
            {
                int index = ParseInt(param);
                if (index >= 0 && index < EnemySlots.Count && EnemySlots[index] != null)
                {
                    SelectedEnemySlot = index;
                    SelectedAllySlot = -1;
                }
            });
            
            RemoveSelectedAllyCommand = new RelayCommand(_ => 
            {
                if (SelectedAllySlot >= 0 && SelectedAllySlot < AllySlots.Count)
                {
                    AllySlots[SelectedAllySlot] = null;
                    SelectedAllySlot = -1;
                }
            });
            
            RemoveSelectedEnemyCommand = new RelayCommand(_ => 
            {
                if (SelectedEnemySlot >= 0 && SelectedEnemySlot < EnemySlots.Count)
                {
                    EnemySlots[SelectedEnemySlot] = null;
                    SelectedEnemySlot = -1;
                }
            });
            
            ExportReportCommand = new RelayCommand(_ => ExportReport());
            AddCharacterCommand = new RelayCommand(() => OpenAddEditWindow?.Invoke(null));
            EditCharacterCommand = new RelayCommand(() => OpenAddEditWindow?.Invoke(SelectedCharacter));
            DeleteCharacterCommand = new RelayCommand(() => DeleteCharacter(SelectedCharacter));
            RefreshCommand = new RelayCommand(() => RefreshCharacters());

            AllySlots.CollectionChanged += (s, e) => UpdateAnalytics();
            EnemySlots.CollectionChanged += (s, e) => UpdateAnalytics();
        }

        private int ParseInt(object param)
        {
            if (param is int i) return i;
            if (param is string s && int.TryParse(s, out int result)) return result;
            return -1;
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

            var result = MessageBox.Show(
                $"Видалити персонажа '{character.Name}'?",
                "Підтвердження",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                for (int i = 0; i < AllySlots.Count; i++)
                    if (AllySlots[i]?.Id == character.Id) AllySlots[i] = null;
                for (int i = 0; i < EnemySlots.Count; i++)
                    if (EnemySlots[i]?.Id == character.Id) EnemySlots[i] = null;

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

        private void UpdateAnalytics()
        {
            var allyList = AllySlots.Where(c => c != null).ToList();
            var enemyList = EnemySlots.Where(c => c != null).ToList();

            TotalBasePower = allyList.Sum(c => c.BasePower);
            BalanceBonus = _analyzer.CalculateTeamBalance(allyList);
            CounterPickBonus = _analyzer.CalculateCounterPicks(allyList, enemyList);
            RarityBonus = _analyzer.CalculateRarityBonus(allyList);
            AllyEnergyBonus = _analyzer.CalculateEnergySynergy(allyList);
            AllyRolePenalty = _analyzer.CalculateMissingRolePenalty(allyList);
            AllyRoleDist = _analyzer.GetRoleDistribution(allyList);
            AllyEnergySum = _analyzer.GetEnergySummary(allyList);
            TotalEfficiency = _analyzer.CalculateTotalEfficiency(allyList, enemyList);

            EnemyTotalBasePower = enemyList.Sum(c => c.BasePower);
            EnemyBalanceBonus = _analyzer.CalculateTeamBalance(enemyList);
            EnemyCounterPickBonus = _analyzer.CalculateCounterPicks(enemyList, allyList);
            EnemyRarityBonus = _analyzer.CalculateRarityBonus(enemyList);
            EnemyEnergyBonus = _analyzer.CalculateEnergySynergy(enemyList);
            EnemyRolePenalty = _analyzer.CalculateMissingRolePenalty(enemyList);
            EnemyRoleDist = _analyzer.GetRoleDistribution(enemyList);
            EnemyEnergySum = _analyzer.GetEnergySummary(enemyList);
            EnemyTotalEfficiency = _analyzer.CalculateTotalEfficiency(enemyList, allyList);

            if (TotalEfficiency > EnemyTotalEfficiency)
                StrongerParty = "СОЮЗНИКИ";
            else if (EnemyTotalEfficiency > TotalEfficiency)
                StrongerParty = "ВОРОГИ";
            else
                StrongerParty = "РІВНІ";
        }

        private void ExportReport()
        {
            var allyList = AllySlots.Where(c => c != null).ToList();
            var enemyList = EnemySlots.Where(c => c != null).ToList();

            if (allyList.Count == 0 && enemyList.Count == 0)
            {
                MessageBox.Show("Додайте хоча б одного персонажа в Союзники або Вороги!", 
                    "ПОМИЛКА", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "DraftReport.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            bool? result = dialog.ShowDialog();
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                _dbManager.ExportDraftReport(allyList, enemyList, TotalEfficiency, dialog.FileName);
                MessageBox.Show($"Звіт збережено:\n{dialog.FileName}", 
                    "УСПІХ", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
