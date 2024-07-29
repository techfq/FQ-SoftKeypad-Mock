using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace FQS_KEYPAD
{
    public struct LicenseKey
    {
        public string Public { get; set; }
        public string Secret { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string LsType { get; set; }
        public bool Status { get; set; }

        public LicenseKey(
            string licensePublic,
            string licenseSecret,
            string licenseStart,
            string licenseEnd,
            string lsType,
            bool status = false
        )
        {
            Public = licensePublic;
            Secret = licenseSecret;
            StartDate = licenseStart;
            EndDate = licenseEnd;
            LsType = lsType;
            Status = status;
        }
    }

    public partial class LicenseInput : Window
    {
        public string licenseFilePath = "license.lic";

        public LicenseInput()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Your click event handling logic goes here
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void btn_activate_license_Click(object sender, RoutedEventArgs e)
        {
            string public_key = tb_public_key.Text;
            string secret_key = tb_secret_key.Text.ToUpper();
            string licenseType = "0";
            bool isValidLicense = false;
            if (IsLicenseValid(public_key + "45", secret_key))
            {
                isValidLicense = true;
                licenseType = "45";
            }
            else if (IsLicenseValid(public_key + "65", secret_key))
            {
                isValidLicense = true;
                licenseType = "65";
            }
            else if (IsLicenseValid(public_key + "888", secret_key))
            {
                isValidLicense = true;
                licenseType = "888";
            }
            else
            {
                btn_activate_license.Background = new SolidColorBrush(
                    Color.FromArgb(0xFF, 0xF5, 0x77, 0x3C)
                );
                btn_activate_license.BorderBrush = new SolidColorBrush(
                    Color.FromArgb(0xFF, 0xF5, 0x77, 0x3C)
                );
                btn_activate_license.Content = "INVALID";
                Debug.WriteLine($"INVALID: {public_key}");
            }

            if (isValidLicense)
            {
                btn_activate_license.Content = "SUCCESS";
                string deviceUuid = GetDeviceId();
                string deviceKey =
                    $"###FAST_QUEUE_PUBLIC####MAC#{deviceUuid}#MAC#####FAST_QUEUE_PUBLIC###";

                string licenseSecretKey = $"###FAST_QUEUE_SYS###{secret_key}###FAST_QUEUE_SYS###";

                string startDate = $"{DateTime.Now.Day}/{DateTime.Now.Month}/{DateTime.Now.Year}";
                DateTime endDate = DateTime.Now.AddDays(45);
                string date = $"###STARTDATE###{startDate}###STARTDATE###" + $"###ENDDATE###{endDate}###ENDDATE###";

                string saveLicenseType = $"###LSTYPE###{licenseType}###LSTYPE###";
                string saveContent = deviceKey + licenseSecretKey + date + saveLicenseType;
                LicenseFile.SaveLicenseFile(licenseFilePath, saveContent);

                DialogResult = true;
                this.Close();
            }
        }

        public LicenseKey IsValid()
        {
            LicenseKey license = generateLicenseKey();
            Debug.WriteLine("IsValid: " + license.Public + license.LsType);
            if (IsLicenseValid(license.Public + license.LsType, license.Secret))
            {
                Debug.WriteLine("VALID");
                license.Status = true;
                return license;
            }
            else
            {
                license.Status = false;
                tb_public_key.Text = license.Public;
                Debug.WriteLine($"INVALID: {license.Public}");
            }
            return license;
        }

        private static bool IsLicenseValid(string publicKey, string secretKey)
        {
            try
            {
                string generateSecretKey = HashLicenseKey(publicKey).Substring(1, 8);

                if (secretKey == generateSecretKey)
                {
                    return true;
                }
                else
                {
                    Debug.WriteLine("You need to activate software license with below key:");
                    Debug.WriteLine($"Pub: {publicKey}");
                    Debug.WriteLine($"Pri: {generateSecretKey}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the license file. " + ex.Message);
            }
            return false;
        }

        private LicenseKey generateLicenseKey()
        {
            Debug.WriteLine("Run generateLicenseKey!");
            try
            {
                string userSecretKey = "";
                string lsStartDate = $"{DateTime.Now.Day}/{DateTime.Now.Month}/{DateTime.Now.Year}";
                string lsEndDate = "";
                string lsType = "";
                if (File.Exists(licenseFilePath))
                {
                    string readLicense = LicenseFile.LoadLicenseFile(licenseFilePath);
                    userSecretKey = ExtractContent(
                        readLicense,
                        "###FAST_QUEUE_SYS###",
                        "###FAST_QUEUE_SYS###"
                    );
                    lsStartDate = ExtractContent(readLicense, "###STARTDATE###", "###STARTDATE###");
                    lsEndDate = ExtractContent(readLicense, "###ENDDATE###", "###ENDDATE###");
                    lsType = ExtractContent(readLicense, "###LSTYPE###", "###LSTYPE###");
                }

                string deviceUuid = GetDeviceId();
                string deviceKey =
                    $"###FAST_QUEUE_PUBLIC####MAC#{deviceUuid}#MAC#####FAST_QUEUE_PUBLIC###";

                string licenseSecretKey =
                    $"###FAST_QUEUE_SYS###{userSecretKey}###FAST_QUEUE_SYS###";

                string saveStartDate = $"###STARTDATE###{lsStartDate}###STARTDATE###";
                string saveEndDate = $"###ENDDATE###{lsEndDate}###ENDDATE###";
                string saveLsType = $"###LSTYPE###{lsType}###LSTYPE###";
                string saveContent = deviceKey + licenseSecretKey + saveStartDate + saveEndDate + saveLsType;
                LicenseFile.SaveLicenseFile(licenseFilePath, saveContent); // NEED TO SAVE THIS TO LICENSE FILE

                string pub_lic = HashLicenseKey(deviceUuid + lsStartDate).Substring(1, 8).ToUpper();
                string pri_lic = userSecretKey;
                LicenseKey license = new LicenseKey(pub_lic, pri_lic, lsStartDate, lsEndDate, lsType);
                return license;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the license file. " + ex.Message);
                return new LicenseKey("XXX", "YYY", "000", "000", "000");
            }
        }

        private static string GetDeviceId()
        {
            try
            {
                using (
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                        "SELECT * FROM Win32_DiskDrive"
                    )
                )
                {
                    // Iterate through the query results
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        // Get the serial number from the "SerialNumber" property
                        string serialNumber = disk["SerialNumber"].ToString();

                        // Return the serial number if found
                        if (!string.IsNullOrEmpty(serialNumber))
                        {
                            return serialNumber;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving device identifier: " + ex.Message);
            }
            return string.Empty;
        }

        private static string ExtractContent(string input, string startTag, string endTag)
        {
            string pattern = Regex.Escape(startTag) + "(.*?)" + Regex.Escape(endTag);
            Match match = Regex.Match(input, pattern);
            if (!match.Success)
            {
                return string.Empty;
            }
            return match.Groups[1].Value;
        }

        private static string HashLicenseKey(string licenseKey)
        {
            string APP_SECRET_KEY = "FQKeypad-v1.0";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(licenseKey + APP_SECRET_KEY);
                byte[] hash = sha256.ComputeHash(bytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in hash)
                {
                    stringBuilder.AppendFormat("{0:x2}", b);
                }

                return stringBuilder.ToString().ToUpper();
            }
        }
    }
}
