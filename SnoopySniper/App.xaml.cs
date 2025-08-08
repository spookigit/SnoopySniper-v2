using System.Windows;
using SpookySniper.Core;
using System.Threading.Tasks;
using System;

namespace SpookySniper.App
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Check license first
            if (!await LicenseManager.ValidateLicenseAsync())
            {
                var licenseWindow = new LicenseWindow();
                if (licenseWindow.ShowDialog() != true)
                {
                    Shutdown();
                    return;
                }
            }
            
            // Initialize the application
            await Startup.InitializeAsync();
            
            // Show main window
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
