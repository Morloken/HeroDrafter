using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using HeroDrafter.Data;
using HeroDrafter.Business;
using HeroDrafter.ViewModels;
using HeroDrafter.Views;

namespace HeroDrafter
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddSingleton<DatabaseManager>();
            services.AddSingleton<DraftAnalyzer>();
            services.AddTransient<MainViewModel>();

            ServiceProvider = services.BuildServiceProvider();

            var dbManager = ServiceProvider.GetRequiredService<DatabaseManager>();
            dbManager.InitializeDatabase();

            int count = dbManager.GetCharacterCount();

            var mainWindow = new MainWindow();
            mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
    }
}
