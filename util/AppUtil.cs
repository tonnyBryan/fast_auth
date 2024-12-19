using fast_authenticator.model;
using System.Text;
using System.Text.RegularExpressions;

namespace fast_authenticator.util
{
    public static class AppUtil
    {
        public static string GeneratePin(int length)
        {
            Random random = new Random();
            string pin = string.Empty;

            for (int i = 0; i < length; i++)
            {
                pin += random.Next(0, 10).ToString(); 
            }

            return pin;
        }

        public static string GeneratePinDiv(string pin)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<div class='container mt-5'>");
            html.AppendLine("    <div class='card p-4' style='max-width: 400px; margin: auto; background-color: #f8f9fa; border-radius: 8px;'>");
            html.AppendLine("        <h4 class='text-center mb-3' style='font-weight: bold;'>Voici votre code d'authentification</h4>");
            html.AppendLine("        <div class='d-flex justify-content-between' style='font-size: 24px; font-weight: bold; text-align: center;'>");

            foreach (char digit in pin)
            {
                html.AppendLine($"            <span class='box' style='width: 40px; height: 40px; display: inline-block; border: 2px solid #d1d8e0; border-radius: 5px; text-align: center; line-height: 36px; font-size: 24px; color: #007bff; background-color: white; margin: 0 5px;'>{digit}</span>");
            }

            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("</div>");

            return html.ToString();
        }

        public static string Crypt(string subject) 
        {
            return BCrypt.Net.BCrypt.HashPassword(subject);
        }

        public static string GenerateSecretUniqueKey(User user)
        {
            string key = AppUtil.GeneratePin(10) + user.IdUser.ToString();
            return Regex.Replace(BCrypt.Net.BCrypt.HashPassword(key), @"[^a-zA-Z0-9 ]", "");
        }
    }
}
