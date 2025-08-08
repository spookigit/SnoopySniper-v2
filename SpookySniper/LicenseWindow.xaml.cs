using System;
using System.Diagnostics;
using System.Windows;
using SpookySniper.Core;

namespace SpookySniper.App
{
    public partial class LicenseWindow : Window
    {
        public LicenseWindow()
        {
            InitializeComponent();
            LicenseKeyTextBox.Text = "";
        }

        private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            string licenseKey = LicenseKeyTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(licenseKey))
            {
                ShowError("Please enter a license key.");
                return;
            }

            ActivateButton.IsEnabled = false;
            ActivateButton.Content = "🔄 Validating...";
            ErrorMessage.Visibility = Visibility.Collapsed;

            try
            {
                bool isValid = await LicenseManager.ActivateLicenseAsync(licenseKey);
                
                if (isValid)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("Invalid license key. Please check your key and try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Activation failed: {ex.Message}");
            }
            finally
            {
                ActivateButton.IsEnabled = true;
                ActivateButton.Content = "🚀 Activate License";
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://whop.com/spookysniper",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowError($"Could not open purchase page: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
