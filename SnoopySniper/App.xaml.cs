using System.Windows;
using SnoopySniper.Core;
using System.Threading.Tasks;
using System;

namespace SnoopySniper.App
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize the application
            await Startup.InitializeAsync();
            
            // Show main window
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
