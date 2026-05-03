using System.Windows;
using HeroDrafter.Data;
using HeroDrafter.ViewModels;

namespace HeroDrafter.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.OpenAddEditWindow += OnOpenAddEditWindow;
            }
        }

        private void OnOpenAddEditWindow(Models.Character characterToEdit)
        {
            var dbManager = App.ServiceProvider.GetService(typeof(DatabaseManager)) as DatabaseManager;
            var window = new AddEditCharacterWindow();
            var vm = new AddEditCharacterViewModel(dbManager, characterToEdit);
            vm.RequestClose += () =>
            {
                window.DialogResult = true;
                window.Close();
            };
            window.DataContext = vm;
            window.Owner = this;
            window.ShowDialog();

            if (DataContext is MainViewModel mainVm)
            {
                mainVm.RefreshCommand.Execute(null);
            }
        }
    }
}