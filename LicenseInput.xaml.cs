using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Calculator_WPF
{
    public struct LicenseKey
    {
        public string Public { get; set; }
        public string Secret { get; set; }

        public LicenseKey(string licensePublic, string licenseSecret)
        {
            Public = licensePublic;
            Secret = licenseSecret;
        }
    }
    public partial class LicenseInput : Window
    {
        public string licenseFilePath = "license.lic";


        public LicenseInput()
        {
            InitializeComponent();
            
        }

        public bool isValid()
        {
            LicenseKey license = new LicenseKey();
            license = generateLicenseKey();

            bool isValidLicense = IsLicenseValid(license.Public, license.Secret);
            if (isValidLicense)
            {
                Debug.WriteLine("VALID");
                this.Close();
                return true;
            }
            else
            {
                tb_public_key.Text = license.Public;
                Debug.WriteLine($"INVALID: {license.Public}");
                Debug.WriteLine($"USERKEY: {license.Secret}");
                return false;
            }
        }

        private void btn_activate_license_Click(object sender, RoutedEventArgs e)
        {
            string public_key = tb_public_key.Text;
            string secret_key = tb_secret_key.Text.ToLower();
            bool isValidLicense = IsLicenseValid(public_key, secret_key);
            if (isValidLicense)
            {
                btn_activate_license.Content = "SUCCESS";
                Debug.WriteLine("VALID");
                string macAddress = GetMacAddress();
                string licenseContent = $"###FAST_QUEUE_PUBLIC####MAC#{macAddress}#MAC#####FAST_QUEUE_PUBLIC###";
                string licenseSecretKey = $"###FAST_QUEUE_SYS###{secret_key}###FAST_QUEUE_SYS###";
                string saveContent = licenseContent + licenseSecretKey;
                LicenseFile.SaveLicenseFile(licenseFilePath, saveContent);
                DialogResult = true;
                this.Close();
            }
            else
            {
                btn_activate_license.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF5, 0x77, 0x3C));
                btn_activate_license.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF5, 0x77, 0x3C));
                btn_activate_license.Content = "INVALID";
                Debug.WriteLine($"INVALID: {public_key}");
            }
        }

        public bool IsLicenseValid(string publicKey, string secretKey)
        {
            bool isValid = false;
            DateTime currentDate = DateTime.Now;

            try
            {
                string generateSecretKey = HashLicenseKey(publicKey).Substring(1, 8);

                if (secretKey == generateSecretKey)
                {
                    isValid = true;
                }
                else
                {
                    Debug.WriteLine("You need to activate software license with below key:");
                    Debug.WriteLine($"{publicKey}");
                    Debug.WriteLine(generateSecretKey);
                    isValid = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the license file. " + ex.Message);
                isValid = false;
            }
            return isValid;
        }

        public LicenseKey generateLicenseKey()
        {
            try
            {
                string userSecretKey = "";
                if (File.Exists(licenseFilePath))
                {
                    string readLicense = LicenseFile.LoadLicenseFile(licenseFilePath);
                    userSecretKey = ExtractContent(readLicense, "###FAST_QUEUE_SYS###", "###FAST_QUEUE_SYS###");
                }

                string macAddress = GetMacAddress();
                string licenseContent = $"###FAST_QUEUE_PUBLIC####MAC#{macAddress}#MAC#####FAST_QUEUE_PUBLIC###";
                string licenseSecretKey = $"###FAST_QUEUE_SYS###{userSecretKey}###FAST_QUEUE_SYS###";

                string saveContent = licenseContent + licenseSecretKey;
                LicenseFile.SaveLicenseFile(licenseFilePath, saveContent); // NEED TO SAVE THIS TO LICENSE FILE
                LicenseKey license = new LicenseKey("ABC", "XYZ");
                license.Public = HashLicenseKey(macAddress).Substring(1, 8).ToUpper();
                license.Secret = userSecretKey;
                return license;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the license file. " + ex.Message);
                return new LicenseKey("XXX", "YYY");
            }
        }
        static string GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    return nic.GetPhysicalAddress().ToString();
                }
            }
            return string.Empty;
        }

        static string ExtractContent(string input, string startTag, string endTag)
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
            string APP_SECRET_KEY = "FQK-v1.0";
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

                return stringBuilder.ToString();
            }
        }
    }
}
