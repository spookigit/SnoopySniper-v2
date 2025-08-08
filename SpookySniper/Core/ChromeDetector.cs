using System;
using System.IO;
using Microsoft.Win32;

namespace SpookySniper.Core
{
    public static class ChromeDetector
    {
        public static string DetectChromePath()
        {
            try
            {
                // Try common Chrome profile locations
                string[] possiblePaths = {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        "Google", "Chrome", "User Data", "Default"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        "Google", "Chrome", "User Data", "Profile 1"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                        "Google", "Chrome", "User Data", "Default")
                };

                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        LogManager.Log($"Chrome profile found: {path}", LogLevel.Success);
                        return path;
                    }
                }

                // Try registry detection
                string registryPath = GetChromePathFromRegistry();
                if (!string.IsNullOrEmpty(registryPath))
                {
                    return registryPath;
                }

                LogManager.Log("Chrome profile not found - user will need to set manually", LogLevel.Warning);
                return string.Empty;
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error detecting Chrome: {ex.Message}", LogLevel.Error);
                return string.Empty;
            }
        }

        private static string GetChromePathFromRegistry()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe"))
                {
                    if (key != null)
                    {
                        string chromePath = key.GetValue(string.Empty)?.ToString();
                        if (!string.IsNullOrEmpty(chromePath) && File.Exists(chromePath))
                        {
                            string profilePath = Path.Combine(Path.GetDirectoryName(chromePath), "..", "..", "User Data", "Default");
                            return Path.GetFullPath(profilePath);
                        }
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        public static bool IsChromeInstalled()
        {
            try
            {
                string[] chromePaths = {
                    @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Google", "Chrome", "Application", "chrome.exe")
                };

                foreach (string path in chromePaths)
                {
                    if (File.Exists(path))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
