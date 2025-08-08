using Newtonsoft.Json;
using System;
using System.IO;

namespace SpookySniper.Core
{
    public static class ConfigurationManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SpookySniper", "config.json");

        private static readonly string WebhookConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SpookySniper", "webhooks.json");

        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading settings: {ex.Message}", LogLevel.Error);
            }

            return new AppSettings();
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
                LogManager.Log("Settings saved successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error saving settings: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        public static WebhookSettings LoadWebhookSettings()
        {
            try
            {
                if (File.Exists(WebhookConfigPath))
                {
                    string json = File.ReadAllText(WebhookConfigPath);
                    return JsonConvert.DeserializeObject<WebhookSettings>(json) ?? new WebhookSettings();
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error loading webhook settings: {ex.Message}", LogLevel.Error);
            }

            return new WebhookSettings();
        }

        public static void SaveWebhookSettings(WebhookSettings settings)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(WebhookConfigPath));
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(WebhookConfigPath, json);
                LogManager.Log("Webhook settings saved successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error saving webhook settings: {ex.Message}", LogLevel.Error);
                throw;
            }
        }
    }

    public class AppSettings
    {
        public string ChromeProfilePath { get; set; } = "";
        public int WebDriverPort { get; set; } = 9515;
        public int DefaultBidInterval { get; set; } = 30;
        public int MaxConcurrentTasks { get; set; } = 5;
        public int BidTimeout { get; set; } = 60;
        public decimal HighValueThreshold { get; set; } = 1.0m;
        public bool AutoDetectChrome { get; set; } = true;
        public bool EnableStealth { get; set; } = true;
        public bool NotifyOnSuccess { get; set; } = true;
        public bool NotifyOnFailure { get; set; } = false;
        public bool NotifyOnTaskStart { get; set; } = true;
    }

    public class WebhookSettings
    {
        public bool DiscordEnabled { get; set; } = false;
        public string DiscordWebhookUrl { get; set; } = "";
        public bool NotifySuccessfulBids { get; set; } = true;
        public bool NotifyFailedBids { get; set; } = false;
        public bool NotifyTaskActions { get; set; } = true;
        public bool NotifyHighValueBids { get; set; } = true;
        public decimal HighValueThreshold { get; set; } = 1.0m;
    }
}
