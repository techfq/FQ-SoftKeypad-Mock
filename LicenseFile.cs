using System;
using System.IO;
using System.Security.Cryptography;

class LicenseFile
{
    private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        byte[] encrypted;
        // Create an Aes object with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                }
                encrypted = msEncrypt.ToArray();
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold the decrypted text.
        string plaintext = null;

        // Create an Aes object with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

    public static void SaveLicenseFile(string path, string content)
    {
        // Generate a random key and IV
        byte[] key = new byte[32]; // AES 256 bits key
        byte[] iv = new byte[16]; // AES block size is 128 bits

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        // Encrypt the content
        byte[] encryptedContent = EncryptStringToBytes_Aes(content, key, iv);

        // Save the key, IV, and encrypted content to the file
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            fs.Write(key, 0, key.Length);
            fs.Write(iv, 0, iv.Length);
            fs.Write(encryptedContent, 0, encryptedContent.Length);
        }
    }

    public static string LoadLicenseFile(string path)
    {
        // Read the key, IV, and encrypted content from the file
        byte[] key = new byte[32];
        byte[] iv = new byte[16];
        byte[] encryptedContent;

        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            fs.Read(key, 0, key.Length);
            fs.Read(iv, 0, iv.Length);

            encryptedContent = new byte[fs.Length - key.Length - iv.Length];
            fs.Read(encryptedContent, 0, encryptedContent.Length);
        }

        // Decrypt the content
        string decryptedContent = DecryptStringFromBytes_Aes(encryptedContent, key, iv);

        return decryptedContent;
    }
}

