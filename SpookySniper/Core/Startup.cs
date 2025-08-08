using System;
using System.Threading.Tasks;
using System.Windows;

namespace SpookySniper.Core
{
    public static class Startup
    {
        public static async Task InitializeAsync()
        {
            try
            {
                // Initialize logging
                LogManager.Initialize();
                LogManager.Log("👻 SpookySniper v2.0 starting up...", LogLevel.Info);

                // Load configuration
                var settings = ConfigurationManager.LoadSettings();
                LogManager.Log("Configuration loaded", LogLevel.Info);

                // Auto-detect Chrome
                string chromePath = ChromeDetector.DetectChromePath();
                if (!string.IsNullOrEmpty(chromePath))
                {
                    settings.ChromeProfilePath = chromePath;
                    LogManager.Log($"Chrome auto-detected: {chromePath}", LogLevel.Success);
                }

                // Initialize Chrome automation
                bool chromeInitialized = await ChromeAutomation.InitializeAsync(
                    settings.ChromeProfilePath, 
                    settings.WebDriverPort);

                if (!chromeInitialized)
                {
                    LogManager.Log("Chrome automation initialization failed - will retry on first task", LogLevel.Warning);
                }

                // Initialize webhook manager (Discord only)
                var webhookSettings = ConfigurationManager.LoadWebhookSettings();
                WebhookManager.UpdateSettings(webhookSettings);
                LogManager.Log("Discord webhook manager initialized", LogLevel.Info);

                // Setup event handlers
                SetupEventHandlers();

                LogManager.Log("👻 SpookySniper initialized successfully - Ready to haunt Magic Eden!", LogLevel.Success);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Startup error: {ex.Message}", LogLevel.Error);
            }
        }

        private static void SetupEventHandlers()
        {
            // Handle application shutdown
            Application.Current.Exit += (s, e) =>
            {
                LogManager.Log("👻 SpookySniper shutting down...", LogLevel.Info);
                ChromeAutomation.Dispose();
                LogManager.Log("Shutdown complete - Ghost has left the machine", LogLevel.Info);
            };

            // Handle unhandled exceptions
            Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                LogManager.Log($"Unhandled exception: {e.Exception.Message}", LogLevel.Error);
                e.Handled = true;
            };
        }
    }
}
