using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

public class LicenseValidation
{
    public string licenseFilePath = "license.lic";
    public bool IsLicenseValid(string license)
    {
        bool isValid = false;
        DateTime currentDate = DateTime.Now;

        try
        {
            generateLicenseKey();
            string licenseContent = LicenseFile.LoadLicenseFile(licenseFilePath);
            DateTime expiryDate = currentDate.AddDays(60);

            string userPublicKey = ExtractContent(licenseContent, "###FAST_QUEUE_PUBLIC###", "###FAST_QUEUE_PUBLIC###");
            string userHashKey = HashLicenseKey(userPublicKey).Substring(1, 8);

            string userSecretKey = ExtractContent(licenseContent, "###FAST_QUEUE_SYS###", "###FAST_QUEUE_SYS###");
            string generateSecretKey = HashLicenseKey(userHashKey).Substring(1, 8);

            if (userSecretKey == generateSecretKey)
            {
                Debug.WriteLine($"Valid keygen: {userSecretKey}");
                Debug.WriteLine($"User keygen: {userHashKey}");
            }
            else
            {
                Debug.WriteLine("You need to activate software license with below key:");
                Debug.WriteLine($"{userHashKey}");
                Debug.WriteLine(generateSecretKey);
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load the license file. " + ex.Message);
            return false;
        }

        isValid = true;

        return isValid;
    }

    void generateLicenseKey()
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
        string APP_SECRET_KEY = "YourSecretKey";
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
