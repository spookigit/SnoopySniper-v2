using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpookySniper.Core
{
    public static class LicenseManager
    {
        private static readonly string LicenseFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SpookySniper", "license.dat");

        private static readonly string WhopApiUrl = "https://api.whop.com/v1/licenses/validate";

        public static async Task<bool> ValidateLicenseAsync()
        {
            try
            {
                // Check if license file exists
                if (!File.Exists(LicenseFilePath))
                    return false;

                string encryptedLicense = File.ReadAllText(LicenseFilePath);
                string licenseKey = DecryptLicense(encryptedLicense);

                // Validate with Whop API (mock for now)
                return await ValidateWithWhopAsync(licenseKey);
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> ActivateLicenseAsync(string licenseKey)
        {
            try
            {
                // Validate with Whop API
                if (!await ValidateWithWhopAsync(licenseKey))
                    return false;

                // Save encrypted license
                Directory.CreateDirectory(Path.GetDirectoryName(LicenseFilePath));
                string encryptedLicense = EncryptLicense(licenseKey);
                File.WriteAllText(LicenseFilePath, encryptedLicense);

                LogManager.Log($"License activated successfully", LogLevel.Success);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log($"License activation failed: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        private static async Task<bool> ValidateWithWhopAsync(string licenseKey)
        {
            try
            {
                // Mock validation for now - replace with actual Whop API
                // For demo purposes, accept any key that starts with "SPOOKY-"
                if (licenseKey.StartsWith("SPOOKY-") && licenseKey.Length >= 20)
                {
                    await Task.Delay(1000); // Simulate API call
                    return true;
                }

                // Uncomment when ready to integrate with Whop
                /*
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_WHOP_API_KEY");
                
                var payload = new { license_key = licenseKey };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync(WhopApiUrl, content);
                return response.IsSuccessStatusCode;
                */

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static string EncryptLicense(string license)
        {
            byte[] data = Encoding.UTF8.GetBytes(license);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptLicense(string encryptedLicense)
        {
            byte[] encrypted = Convert.FromBase64String(encryptedLicense);
            byte[] data = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }

        public static string GetLicenseInfo()
        {
            try
            {
                if (!File.Exists(LicenseFilePath))
                    return "No License";

                string encryptedLicense = File.ReadAllText(LicenseFilePath);
                string licenseKey = DecryptLicense(encryptedLicense);
                
                // Mask the license key for display
                if (licenseKey.Length > 8)
                {
                    return $"{licenseKey.Substring(0, 8)}...{licenseKey.Substring(licenseKey.Length - 4)}";
                }
                
                return "Licensed";
            }
            catch
            {
                return "Invalid License";
            }
        }
    }
}
