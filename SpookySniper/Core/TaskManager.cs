using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace SpookySniper.Core
{
    public static class TaskManager
    {
        private static readonly ObservableCollection<App.BidTask> _tasks = new ObservableCollection<App.BidTask>();
        private static readonly Dictionary<string, CancellationTokenSource> _taskCancellationTokens = new Dictionary<string, CancellationTokenSource>();

        public static ObservableCollection<App.BidTask> Tasks => _tasks;
        public static int ActiveTaskCount => _tasks.Count(t => t.IsRunning);

        public static event Action<int> ActiveTaskCountChanged;

        public static void AddTask(App.BidTask task)
        {
            if (task.IsValidTask())
            {
                _tasks.Add(task);
                LogManager.Log($"👻 Task added: {task.TaskName}", LogLevel.Success);
            }
            else
            {
                LogManager.Log($"Invalid task configuration: {task.TaskName}", LogLevel.Error);
                throw new ArgumentException("Invalid task configuration");
            }
        }

        public static void RemoveTask(App.BidTask task)
        {
            if (task.IsRunning)
            {
                StopTask(task);
            }

            _tasks.Remove(task);
            LogManager.Log($"👻 Task removed: {task.TaskName}", LogLevel.Info);
            NotifyActiveTaskCountChanged();
        }

        public static async Task StartTask(App.BidTask task)
        {
            if (task.IsRunning)
            {
                LogManager.Log($"Task {task.TaskName} is already running", LogLevel.Warning);
                return;
            }

            try
            {
                var cancellationToken = new CancellationTokenSource();
                _taskCancellationTokens[task.TaskName] = cancellationToken;

                task.Start();
                NotifyActiveTaskCountChanged();

                // Send Discord notification
                await WebhookManager.SendTaskNotification(task, "Started");

                // Start the bidding task in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ChromeAutomation.StartBiddingTask(task);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Log($"Error in task {task.TaskName}: {ex.Message}", LogLevel.Error);
                        task.Status = "Error";
                        await WebhookManager.SendFailureNotification(task, ex.Message);
                    }
                    finally
                    {
                        if (_taskCancellationTokens.ContainsKey(task.TaskName))
                        {
                            _taskCancellationTokens.Remove(task.TaskName);
                        }
                        NotifyActiveTaskCountChanged();
                    }
                }, cancellationToken.Token);

                LogManager.Log($"👻 Task started successfully: {task.TaskName}", LogLevel.Success);
            }
            catch (Exception ex)
            {
                LogManager.Log($"Failed to start task {task.TaskName}: {ex.Message}", LogLevel.Error);
                task.Status = "Error";
                throw;
            }
        }

        public static void StopTask(App.BidTask task)
        {
            try
            {
                if (_taskCancellationTokens.ContainsKey(task.TaskName))
                {
                    _taskCancellationTokens[task.TaskName].Cancel();
                    _taskCancellationTokens.Remove(task.TaskName);
                }

                task.Stop();
                NotifyActiveTaskCountChanged();

                LogManager.Log($"👻 Task stopped: {task.TaskName}", LogLevel.Info);

                // Send Discord notification
                _ = Task.Run(async () =>
                {
                    await WebhookManager.SendTaskNotification(task, "Stopped");
                });
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error stopping task {task.TaskName}: {ex.Message}", LogLevel.Error);
            }
        }

        public static void StopAllTasks()
        {
            var runningTasks = _tasks.Where(t => t.IsRunning).ToList();
            
            foreach (var task in runningTasks)
            {
                StopTask(task);
            }

            LogManager.Log($"👻 All tasks stopped ({runningTasks.Count} tasks)", LogLevel.Info);
        }

        public static void PauseTask(App.BidTask task)
        {
            if (task.IsRunning)
            {
                StopTask(task);
                LogManager.Log($"👻 Task paused: {task.TaskName}", LogLevel.Info);
            }
        }

        public static async Task RestartTask(App.BidTask task)
        {
            if (task.IsRunning)
            {
                StopTask(task);
                await Task.Delay(2000); // Wait 2 seconds before restarting
            }

            await StartTask(task);
            LogManager.Log($"👻 Task restarted: {task.TaskName}", LogLevel.Success);
        }

        public static App.BidTask GetTaskByName(string taskName)
        {
            return _tasks.FirstOrDefault(t => t.TaskName.Equals(taskName, StringComparison.OrdinalIgnoreCase));
        }

        public static List<App.BidTask> GetTasksByBlockchain(string blockchain)
        {
            return _tasks.Where(t => t.Blockchain.Equals(blockchain, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static List<App.BidTask> GetRunningTasks()
        {
            return _tasks.Where(t => t.IsRunning).ToList();
        }

        public static TaskStatistics GetStatistics()
        {
            return new TaskStatistics
            {
                TotalTasks = _tasks.Count,
                RunningTasks = _tasks.Count(t => t.IsRunning),
                CompletedTasks = _tasks.Count(t => t.Status == "Completed"),
                ErrorTasks = _tasks.Count(t => t.Status == "Error"),
                TotalSuccessfulBids = _tasks.Sum(t => t.SuccessfulBids),
                TotalFailedBids = _tasks.Sum(t => t.FailedBids),
                TotalSpentBTC = _tasks.Where(t => t.Blockchain == "Bitcoin").Sum(t => t.TotalSpent),
                TotalSpentSOL = _tasks.Where(t => t.Blockchain == "Solana").Sum(t => t.TotalSpent)
            };
        }

        private static void NotifyActiveTaskCountChanged()
        {
            ActiveTaskCountChanged?.Invoke(ActiveTaskCount);
        }

        public static void Initialize()
        {
            LogManager.Log("👻 TaskManager initialized - Ready to manage spooky tasks!", LogLevel.Info);
        }

        public static void Shutdown()
        {
            StopAllTasks();
            _tasks.Clear();
            _taskCancellationTokens.Clear();
            LogManager.Log("👻 TaskManager shutdown complete", LogLevel.Info);
        }
    }

    public class TaskStatistics
    {
        public int TotalTasks { get; set; }
        public int RunningTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int ErrorTasks { get; set; }
        public int TotalSuccessfulBids { get; set; }
        public int TotalFailedBids { get; set; }
        public decimal TotalSpentBTC { get; set; }
        public decimal TotalSpentSOL { get; set; }

        public string SuccessRate
        {
            get
            {
                int totalBids = TotalSuccessfulBids + TotalFailedBids;
                if (totalBids == 0) return "0%";
                return $"{(TotalSuccessfulBids * 100.0 / totalBids):F1}%";
            }
        }
    }
}
