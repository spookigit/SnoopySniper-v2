using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;
using System.Linq;
using SpookySniper.App;

namespace SpookySniper.Core
{
    public static class ChromeAutomation
    {
        private static ChromeDriver _driver;
        public static bool IsConnected => _driver != null;

        public static async Task<bool> InitializeAsync(string profilePath = null, int port = 9515)
        {
            try
            {
                var options = new ChromeOptions();
                
                if (!string.IsNullOrEmpty(profilePath))
                {
                    options.AddArgument($"--user-data-dir={profilePath}");
                    LogManager.Log($"Using Chrome profile: {profilePath}", LogLevel.Info);
                }
                
                // Stealth options
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-gpu");
                options.AddExperimentalOption("excludeSwitches", new[] { "enable-automation" });
                options.AddExperimentalOption("useAutomationExtension", false);
                
                var service = ChromeDriverService.CreateDefaultService();
                service.Port = port;
                service.HideCommandPromptWindow = true;
                
                _driver = new ChromeDriver(service, options);
                
                // Execute script to hide automation
                _driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
                
                LogManager.Log("👻 Chrome automation initialized - SpookySniper is ready to haunt!", LogLevel.Success);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log($"Failed to initialize Chrome: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public static async Task<bool> NavigateToMagicEden()
        {
            try
            {
                if (_driver == null) return false;
                
                _driver.Navigate().GoToUrl("https://magiceden.io");
                await Task.Delay(3000);
                
                LogManager.Log("👻 Navigated to Magic Eden - Ready to hunt NFTs!", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error navigating to Magic Eden: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public static async Task StartBiddingTask(BidTask task)
        {
            if (_driver == null)
            {
                LogManager.Log("Chrome not initialized - attempting to reconnect", LogLevel.Warning);
                var settings = ConfigurationManager.LoadSettings();
                if (!await InitializeAsync(settings.ChromeProfilePath, settings.WebDriverPort))
                {
                    LogManager.Log("Failed to reconnect to Chrome", LogLevel.Error);
                    return;
                }
            }

            try
            {
                LogManager.Log($"👻 Starting bidding task: {task.TaskName}", LogLevel.Info);
                
                // Navigate to collection
                _driver.Navigate().GoToUrl(task.CollectionUrl);
                await Task.Delay(3000);

                // Start bidding loop
                while (task.Status == "Running")
                {
                    await PlaceBid(task);
                    
                    // Wait for next bid interval
                    int remainingSeconds = task.IntervalMs / 1000;
                    while (remainingSeconds > 0 && task.Status == "Running")
                    {
                        task.UpdateProgress(remainingSeconds, task.IntervalMs / 1000);
                        await Task.Delay(1000);
                        remainingSeconds--;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error in bidding task {task.TaskName}: {ex.Message}", LogLevel.Error);
                task.Status = "Error";
            }
        }

        private static async Task PlaceBid(BidTask task)
        {
            try
            {
                LogManager.Log($"👻 Attempting to place bid on {task.TaskName}", LogLevel.Info);
                
                // Look for bid button
                var bidButtons = _driver.FindElements(By.XPath("//button[contains(text(), 'Place Bid') or contains(text(), 'Bid') or contains(@class, 'bid')]"));
                
                if (bidButtons.Count == 0)
                {
                    LogManager.Log($"No bid button found for {task.TaskName}", LogLevel.Warning);
                    return;
                }

                bidButtons[0].Click();
                await Task.Delay(2000);

                // Look for bid amount input
                var bidInputs = _driver.FindElements(By.XPath("//input[@type='number' or @placeholder*='bid' or @placeholder*='amount']"));
                
                if (bidInputs.Count > 0)
                {
                    bidInputs[0].Clear();
                    bidInputs[0].SendKeys(task.MaxBidAmount.ToString());
                    await Task.Delay(1000);

                    // Look for confirm button
                    var confirmButtons = _driver.FindElements(By.XPath("//button[contains(text(), 'Confirm') or contains(text(), 'Submit') or contains(text(), 'Place')]"));
                    
                    if (confirmButtons.Count > 0)
                    {
                        confirmButtons[0].Click();
                        LogManager.Log($"👻 Bid placed for {task.TaskName}: {task.MaxBidAmount}", LogLevel.Success);
                        
                        // Check result after delay
                        await Task.Delay(5000);
                        await CheckBidResult(task);
                    }
                }
            }
            catch (NoSuchElementException)
            {
                LogManager.Log($"Bid elements not found for {task.TaskName} - page may have changed", LogLevel.Warning);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error placing bid for {task.TaskName}: {ex.Message}", LogLevel.Error);
                task.RecordFailedBid();
            }
        }

        private static async Task CheckBidResult(BidTask task)
        {
            try
            {
                // Look for success indicators
                var successElements = _driver.FindElements(By.XPath("//*[contains(text(), 'success') or contains(text(), 'Success') or contains(text(), 'successful')]"));
                var errorElements = _driver.FindElements(By.XPath("//*[contains(text(), 'error') or contains(text(), 'Error') or contains(text(), 'failed')]"));

                if (successElements.Count > 0)
                {
                    LogManager.Log($"👻 SUCCESSFUL BID on {task.TaskName}! 🎉", LogLevel.Success);
                    task.RecordSuccessfulBid(task.MaxBidAmount);
                    
                    // Send Discord notification
                    await WebhookManager.SendSuccessNotification(task);
                    
                    if (task.StopAfterSuccess)
                    {
                        task.Status = "Completed";
                        LogManager.Log($"Task {task.TaskName} completed successfully", LogLevel.Success);
                    }
                }
                else if (errorElements.Count > 0)
                {
                    LogManager.Log($"Bid failed for {task.TaskName}", LogLevel.Warning);
                    task.RecordFailedBid();
                }
                else
                {
                    LogManager.Log($"Bid result unclear for {task.TaskName} - continuing", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error checking bid result: {ex.Message}", LogLevel.Error);
            }
        }

        public static void Dispose()
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
                _driver = null;
                LogManager.Log("👻 Chrome automation disposed - Ghost has left the browser", LogLevel.Info);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error disposing Chrome: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
