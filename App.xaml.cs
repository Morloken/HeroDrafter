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
            System.IO.File.AppendAllText("app_debug.txt", "OnStartup: Character count = " + count + "\n");

            if (count == 0)
            {
                System.IO.File.AppendAllText("app_debug.txt", "OnStartup: Starting import...\n");
                var importer = new DataImporter();
                string sourcePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Бойцы_описание.txt");
                System.IO.File.AppendAllText("app_debug.txt", "OnStartup: Source path = " + sourcePath + "\n");
                System.IO.File.AppendAllText("app_debug.txt", "OnStartup: File exists = " + System.IO.File.Exists(sourcePath) + "\n");

                if (System.IO.File.Exists(sourcePath))
                {
                    using (var conn = dbManager.CreateOpenConnection())
                    {
                        importer.ImportFromFile(sourcePath, conn);
                    }
                }
            }

            var mainWindow = new MainWindow();
            mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
    }
}
