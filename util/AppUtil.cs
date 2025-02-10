using fast_authenticator.model;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;


namespace fast_authenticator.util
{
    public static class AppUtil
    {
        private static Random random = new Random();

        public static string GeneratePin(int length)
        {
            Random random = new();
            string pin = string.Empty;

            for (int i = 0; i < length; i++)
            {
                pin += random.Next(0, 10).ToString(); 
            }

            return pin;
        }


        public static string GeneratePinDiv(string pinCode)
        {
            StringBuilder pinHtml = new StringBuilder();

            foreach (char digit in pinCode)
            {
                pinHtml.Append($@"
                <div style=""display: inline-block; margin-left: 3px; width: 40px; height: 40px; border: 1px solid #ddd; font-size: 1.5rem; font-weight: bold; background: #f9f9f9; border-radius: 5px; text-align: center;"">
                    <p style=""margin-top: 5px;"">{digit}</p>
                </div>");
            }

            return $@"
            <html lang=""fr"">
            <head>
                <link rel=""stylesheet"" href=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css"">
            </head>
            <body>
                <div style=""margin: 0; padding: 0; background: linear-gradient(135deg, #f5f7fa, #c3cfe2); font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh;"">
                    <div class=""card"" style=""background: #fff; border-radius: 15px; box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.2); max-width: 400px; width: 90%; text-align: center; padding: 20px; margin:auto; height: fit-content;"">
                        <h3 style=""font-weight: bold; color: #5972a7; margin-bottom: 15px; font-size: xx-large;"">Fast_auth</h3>
                        <div style=""width: 80px; height: 80px; margin: 0 auto 15px;"">
                            <img src=""https://res.cloudinary.com/dunnmcqsz/image/upload/v1738573477/mfnfhoacpnbbkjnauoeb.png"" alt=""Logo Crypt-G"" style=""width: 80px; height: 80px; margin: 0 auto 15px;"" />
                        </div>
                        <h6 class=""card-title"" style=""font-weight: bold; font-size: 1.0rem; margin-bottom: 15px;"">PIN Code :</h6>
                        <div style=""text-align: center;padding-bottom: 2.5rem"">
                            {pinHtml}
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string GenerateEmailHtmlConfirmation(string confirmationCode, string url)
        {
            return $@"
            <html lang=""fr"">
            <head>
                <link rel=""stylesheet"" href=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css"">
            </head>
            <body>
                <div style=""margin: 0; padding: 0; background: linear-gradient(135deg, #f5f7fa, #c3cfe2); font-family: Arial, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh;"">
                    <div class=""card"" style=""background: #fff; border-radius: 15px; box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.2); max-width: 400px; width: 90%; text-align: center; padding: 20px; margin:auto; height: fit-content;"">
                        <h3 style=""font-weight: bold; color: #5972a7; margin-bottom: 15px; font-size: xx-large;"">Fast_auth</h3>
                        <div class=""alert alert-info"" role=""alert"">
                            <p class=""alert-link"">POSTMAN (PUT) : {url}</p>
                        </div>
                        <div style=""width: 80px; height: 80px; margin: 0 auto 15px;"">
                            <img src=""https://res.cloudinary.com/dunnmcqsz/image/upload/v1738573477/mfnfhoacpnbbkjnauoeb.png"" alt=""Logo Crypt-G"" style=""width: 80px; height: 80px; margin: 0 auto 15px;"" />
                        </div>
                        <h6 class=""card-title"" style=""font-weight: bold; font-size: 1.5rem; margin-bottom: 15px;"">Key :</h6>
                        <div style=""background: #f9f9f9; padding-top: 10px; padding-bottom: 10px; width: 100%; border-radius: 5px; border: 1px solid #ddd; margin-bottom: 15px; display: inline-block; font-weight: bold; font-size: 1.5rem;"" id=""confirmation-code"">
                            {confirmationCode}
                        </div>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string Crypt(string subject) 
        {
            return BCrypt.Net.BCrypt.HashPassword(subject);
        }

        // public static string GenerateSecretUniqueKey(User user)
        // {
        //     string key = GeneratePin(10) + user.IdUser.ToString();
        //     string hashedKey = Regex.Replace(BCrypt.Net.BCrypt.HashPassword(key), @"[^a-zA-Z0-9]", "");
        //     return hashedKey.Substring(0, 6);
        // }

        public static string GenerateSecretUniqueKey(User user)
        {
            const string chars = "ABCDEF0123GHIJKLMNOPWXYZabc456defghijklmnopqQRSTUVrstuvwxyz789";
            return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        public static string Encrypt(string plainText, string secretKey)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(secretKey); // La clé secrète (16, 24, ou 32 bytes)
            aesAlg.IV = new byte[16]; // Initialisation vector (IV) à 0 pour simplification (ne jamais faire cela en production)

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter swEncrypt = new(csEncrypt);
                swEncrypt.Write(plainText);
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static string Decrypt(string cipherText, string secretKey)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(secretKey); // La clé secrète
            aesAlg.IV = new byte[16]; // Utilisation de l'IV initialisé à 0

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new(Convert.FromBase64String(cipherText));
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
    }
}
