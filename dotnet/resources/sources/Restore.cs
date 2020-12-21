using System;
using System.Net;
using iTeffa.Settings;
using System.Net.Mail;

namespace iTeffa
{
    public static class PasswordRestore
    {

        private static readonly Nlogs Log = new Nlogs("PassRestore");
        private static readonly Config config = new Config("PassRestore");
        private static readonly string mailFrom = config.TryGet<string>("From", "noreply@iteffa.com");
        private static readonly string mailTitle1 = config.TryGet<string>("Title1", "Password Restore");
        private static readonly string mailTitle2 = config.TryGet<string>("Title2", "New Password");
        private static readonly string mailBody1 = config.TryGet<string>("Body1", "<p>Код для восстановления пароля: {0}</p>");
        private static readonly string mailBody2 = config.TryGet<string>("Body2", "<p>Вы успешно восстановили пароль, Ваш новый пароль: {0}</p>");
        private static readonly string Server = config.TryGet<string>("SMTP", "smtp.iteffa.com");
        private static readonly string Password = config.TryGet<string>("Pass", "Password");
        private static readonly int Port = config.TryGet<int>("Port", 587);

        public static void SendEmail(byte type, string email, int textcode)
        {
            try
            {
                MailMessage msg;
                if (type == 0) msg = new MailMessage(mailFrom, email, mailTitle1, string.Format(mailBody1, textcode));
                else msg = new MailMessage(mailFrom, email, mailTitle2, string.Format(mailBody2, textcode));
                msg.IsBodyHtml = true;
                SmtpClient smtpClient = new SmtpClient(Server, Port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(mailFrom, Password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                smtpClient.Send(msg);
                if (type == 0) Log.Debug($"Сообщение с кодом для восстановления пароля успешно отправлено на {email}!", Nlogs.Type.Success);
                else Log.Debug($"Сообщение с новым паролем успешно отправлено на {email}!", Nlogs.Type.Success);
            }
            catch (Exception ex)
            {
                Log.Write("EXCEPTION AT \"SendEmail\":\n" + ex.ToString(), Nlogs.Type.Error);
            }
        }


    }
}
