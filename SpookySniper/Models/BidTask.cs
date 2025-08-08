using System;
using System.ComponentModel;

namespace SpookySniper.App
{
    public class BidTask : INotifyPropertyChanged
    {
        private string _status = "Stopped";
        private int _successfulBids = 0;
        private int _failedBids = 0;
        private decimal _totalSpent = 0;
        private DateTime _lastBidTime = DateTime.MinValue;
        private int _currentProgress = 0;
        private int _maxProgress = 100;

        // Basic Properties
        public string TaskName { get; set; } = "";
        public string CollectionUrl { get; set; } = "";
        public string Blockchain { get; set; } = "Solana";
        public string BlockchainIcon => Blockchain == "Bitcoin" ? "₿" : "◎";
        public decimal MaxBidAmount { get; set; } = 0.1m;
        public int IntervalMs { get; set; } = 30000;
        public bool StopAfterSuccess { get; set; } = false;
        public bool IsHighValue => MaxBidAmount >= 1.0m;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Status Properties
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public string StatusColor => Status switch
        {
            "Running" => "#10B981",
            "Stopped" => "#6B7280",
            "Error" => "#EF4444",
            "Completed" => "#8B5CF6",
            _ => "#6B7280"
        };

        public bool IsRunning => Status == "Running";

        // Statistics
        public int SuccessfulBids
        {
            get => _successfulBids;
            private set
            {
                _successfulBids = value;
                OnPropertyChanged(nameof(SuccessfulBids));
                OnPropertyChanged(nameof(TotalBids));
                OnPropertyChanged(nameof(SuccessRate));
            }
        }

        public int FailedBids
        {
            get => _failedBids;
            private set
            {
                _failedBids = value;
                OnPropertyChanged(nameof(FailedBids));
                OnPropertyChanged(nameof(TotalBids));
                OnPropertyChanged(nameof(SuccessRate));
            }
        }

        public int TotalBids => SuccessfulBids + FailedBids;

        public string SuccessRate
        {
            get
            {
                if (TotalBids == 0) return "0%";
                return $"{(SuccessfulBids * 100.0 / TotalBids):F1}%";
            }
        }

        public decimal TotalSpent
        {
            get => _totalSpent;
            private set
            {
                _totalSpent = value;
                OnPropertyChanged(nameof(TotalSpent));
                OnPropertyChanged(nameof(TotalSpentFormatted));
            }
        }

        public string TotalSpentFormatted => $"{TotalSpent:F4} {(Blockchain == "Bitcoin" ? "BTC" : "SOL")}";

        public DateTime LastBidTime
        {
            get => _lastBidTime;
            private set
            {
                _lastBidTime = value;
                OnPropertyChanged(nameof(LastBidTime));
                OnPropertyChanged(nameof(LastBidTimeFormatted));
            }
        }

        public string LastBidTimeFormatted => LastBidTime == DateTime.MinValue ? "Never" : LastBidTime.ToString("HH:mm:ss");

        // Progress Properties
        public int CurrentProgress
        {
            get => _currentProgress;
            private set
            {
                _currentProgress = value;
                OnPropertyChanged(nameof(CurrentProgress));
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        public int MaxProgress
        {
            get => _maxProgress;
            private set
            {
                _maxProgress = value;
                OnPropertyChanged(nameof(MaxProgress));
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        public double ProgressPercentage => MaxProgress > 0 ? (CurrentProgress * 100.0 / MaxProgress) : 0;

        // Methods
        public void Start()
        {
            Status = "Running";
            Core.LogManager.Log($"👻 Task started: {TaskName}", Core.LogLevel.Success);
        }

        public void Stop()
        {
            Status = "Stopped";
            Core.LogManager.Log($"👻 Task stopped: {TaskName}", Core.LogLevel.Info);
        }

        public void RecordSuccessfulBid(decimal amount)
        {
            SuccessfulBids++;
            TotalSpent += amount;
            LastBidTime = DateTime.Now;
            Core.LogManager.Log($"👻 SUCCESSFUL BID: {TaskName} - {amount} {(Blockchain == "Bitcoin" ? "BTC" : "SOL")}", Core.LogLevel.Success);
        }

        public void RecordFailedBid()
        {
            FailedBids++;
            LastBidTime = DateTime.Now;
            Core.LogManager.Log($"👻 Failed bid: {TaskName}", Core.LogLevel.Warning);
        }

        public void UpdateProgress(int current, int max)
        {
            CurrentProgress = current;
            MaxProgress = max;
        }

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper Methods
        public string GetDisplayInfo()
        {
            return $"{TaskName} | {Blockchain} | {MaxBidAmount:F4} | {Status}";
        }

        public bool IsValidTask()
        {
            return !string.IsNullOrEmpty(TaskName) &&
                   !string.IsNullOrEmpty(CollectionUrl) &&
                   MaxBidAmount > 0 &&
                   IntervalMs >= 5000; // Minimum 5 second interval
        }
    }
}
