using System.Net.Mail;
using System.Net;

namespace fast_authenticator.util
{
    public class MailSender
    {
        public static void SendEmail(string toEmail, string subject, string htmlBody) 
        {
            string smtpHost = Constant.smtpHost;
            int smtpPort = Constant.smtpPort;
            string smtpUser = Constant.smtpUser;
            string smtpPassword = Constant.smtpPassword;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(smtpUser);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = htmlBody;
            mail.IsBodyHtml = true; 

            // Configuration du client SMTP
            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true // Activer SSL si nécessaire
            };

            // Envoi de l'email
            smtpClient.Send(mail);

            Console.WriteLine("L'email a été envoyé avec succès !");
   
        }
    }
}
