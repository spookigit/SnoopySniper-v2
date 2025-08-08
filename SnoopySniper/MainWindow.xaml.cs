using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpookySniper.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigateToPage("Dashboard");
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NavigateToPage(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string pageName = button.Tag.ToString();
                NavigateToPage(pageName);

                ResetAllButtonStyles();
                button.Style = FindResource("SidebarButtonActive") as Style;
            }
        }

        private void NavigateToPage(string pageName)
        {
            switch (pageName)
            {
                case "Dashboard":
                    ContentFrame.Navigate(new Pages.DashboardPage());
                    break;
                case "Tasks":
                    ContentFrame.Navigate(new Pages.TasksPage());
                    break;
                case "Webhooks":
                    ContentFrame.Navigate(new Pages.WebhooksPage());
                    break;
                case "Listing":
                    ContentFrame.Navigate(new Pages.ListingPage());
                    break;
                case "Console":
                    ContentFrame.Navigate(new Pages.ConsolePage());
                    break;
                case "Settings":
                    ContentFrame.Navigate(new Pages.SettingsPage());
                    break;
                case "Help":
                    ContentFrame.Navigate(new Pages.HelpPage());
                    break;
                default:
                    ContentFrame.Navigate(new Pages.DashboardPage());
                    break;
            }
        }

        private void ResetAllButtonStyles()
        {
            Style defaultStyle = FindResource("SidebarButton") as Style;
            DashboardButton.Style = defaultStyle;
            TasksButton.Style = defaultStyle;
            WebhooksButton.Style = defaultStyle;
            ListingButton.Style = defaultStyle;
            ConsoleButton.Style = defaultStyle;
            SettingsButton.Style = defaultStyle;
            HelpButton.Style = defaultStyle;
        }
    }
}
