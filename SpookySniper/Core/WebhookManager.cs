using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpookySniper.App;

namespace SpookySniper.Core
{
    public static class WebhookManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static WebhookSettings _settings;

        static WebhookManager()
        {
            _settings = ConfigurationManager.LoadWebhookSettings();
        }

        public static void UpdateSettings(WebhookSettings settings)
        {
            _settings = settings;
        }

        public static async Task SendSuccessNotification(BidTask task)
        {
            if (_settings.DiscordEnabled && !string.IsNullOrEmpty(_settings.DiscordWebhookUrl))
            {
                await SendDiscordNotification(task, "success");
            }
        }

        public static async Task SendFailureNotification(BidTask task, string error)
        {
            if (_settings.DiscordEnabled && !string.IsNullOrEmpty(_settings.DiscordWebhookUrl))
            {
                await SendDiscordNotification(task, "failure", error);
            }
        }

        public static async Task SendTaskNotification(BidTask task, string action)
        {
            if (_settings.DiscordEnabled && !string.IsNullOrEmpty(_settings.DiscordWebhookUrl))
            {
                await SendDiscordNotification(task, "task", action);
            }
        }

        private static async Task SendDiscordNotification(BidTask task, string type, string extra = null)
        {
            try
            {
                var embed = type switch
                {
                    "success" => new
                    {
                        title = "👻 SpookySniper - SUCCESSFUL BID! 🎉",
                        color = 0x10B981, // Green
                        fields = new[]
                        {
                            new { name = "Collection", value = task.TaskName, inline = true },
                            new { name = "Blockchain", value = task.BlockchainIcon == "₿" ? "Bitcoin" : "Solana", inline = true },
                            new { name = "Bid Amount", value = $"{task.MaxBidAmount} {(task.BlockchainIcon == "₿" ? "BTC" : "SOL")}", inline = true },
                            new { name = "Collection URL", value = $"[View on Magic Eden]({task.CollectionUrl})", inline = false },
                            new { name = "Timestamp", value = $"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>", inline = false }
                        },
                        footer = new { text = "👻 SpookySniper v2.0 - The Ghost in the Machine", icon_url = "https://cdn.discordapp.com/emojis/ghost.png" },
                        thumbnail = new { url = "https://i.imgur.com/spooky-success.gif" }
                    },
                    "failure" => new
                    {
                        title = "👻 SpookySniper - Bid Failed ❌",
                        color = 0xEF4444, // Red
                        fields = new[]
                        {
                            new { name = "Collection", value = task.TaskName, inline = true },
                            new { name = "Error", value = extra ?? "Unknown error", inline = false },
                            new { name = "Timestamp", value = $"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>", inline = false }
                        },
                        footer = new { text = "👻 SpookySniper v2.0", icon_url = "https://cdn.discordapp.com/emojis/ghost.png" }
                    },
                    "task" => new
                    {
                        title = $"👻 SpookySniper - Task {extra}",
                        color = 0x8B5CF6, // Purple
                        fields = new[]
                        {
                            new { name = "Collection", value = task.TaskName, inline = true },
                            new { name = "Action", value = extra, inline = true },
                            new { name = "Max Bid", value = $"{task.MaxBidAmount} {(task.BlockchainIcon == "₿" ? "BTC" : "SOL")}", inline = true },
                            new { name = "Interval", value = $"{task.IntervalMs / 1000}s", inline = true },
                            new { name = "Timestamp", value = $"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>", inline = false }
                        },
                        footer = new { text = "👻 SpookySniper v2.0 - Premium Edition" }
                    },
                    _ => throw new ArgumentException("Invalid notification type")
                };

                var payload = new 
                { 
                    embeds = new[] { embed },
                    username = "👻 SpookySniper",
                    avatar_url = "https://i.imgur.com/spooky-avatar.png"
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_settings.DiscordWebhookUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    LogManager.Log("👻 Discord notification sent successfully", LogLevel.Success);
                }
                else
                {
                    LogManager.Log($"Failed to send Discord notification: {response.StatusCode}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error sending Discord notification: {ex.Message}", LogLevel.Error);
            }
        }

        public static async Task<bool> TestWebhookAsync(string webhookUrl)
        {
            try
            {
                var testEmbed = new
                {
                    title = "👻 SpookySniper - Test Notification",
                    description = "If you can see this, your Discord webhook is working perfectly! 🎉",
                    color = 0x8B5CF6,
                    fields = new[]
                    {
                        new { name = "Status", value = "✅ Connected", inline = true },
                        new { name = "Version", value = "v2.0 Premium", inline = true },
                        new { name = "Test Time", value = $"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>", inline = false }
                    },
                    footer = new { text = "👻 SpookySniper v2.0 - Ready to haunt Magic Eden!" }
                };

                var payload = new 
                { 
                    embeds = new[] { testEmbed },
                    username = "👻 SpookySniper Test",
                    avatar_url = "https://i.imgur.com/spooky-test.png"
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
