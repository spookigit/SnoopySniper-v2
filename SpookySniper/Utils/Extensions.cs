using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpookySniper.Utils
{
    public static class Extensions
    {
        public static T FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;

                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static string ToFormattedString(this decimal value, string currency)
        {
            return $"{value:F4} {currency}";
        }

        public static string ToTimeString(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            else if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            else
                return $"{timeSpan.Seconds}s";
        }

        public static void ShowSuccess(this Window window, string message)
        {
            MessageBox.Show(window, message, "👻 SpookySniper - Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(this Window window, string message)
        {
            MessageBox.Show(window, message, "👻 SpookySniper - Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool ShowConfirmation(this Window window, string message)
        {
            var result = MessageBox.Show(window, message, "👻 SpookySniper - Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
