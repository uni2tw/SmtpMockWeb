using System;
using System.Net.Mail;
using System.Text;

namespace SmtpMockWeb.Code
{
    public class Utility
    {
        private static ICommonLog logger = Ioc.Get<ICommonLog>();
        public static void SendNotifyStartingMail()
        {
            Utility.SendNotifyMail(Helper.GetProductName() + " was starting.", "<h2>SearchEngine was starting.</h2>");
        }

        public static void SendNotifyStoppedMail()
        {
            Utility.SendNotifyMail(Helper.GetProductName() + " was stopped.", "<h2>SearchEngine was stopped.</h2>");
        }

        public static void SendNotifyTerminatedMail(Exception ex)
        {
            Utility.SendNotifyMail(Helper.GetProductName() + " was terminated.",
                "<h2>" + Helper.GetProductName() + " was terminated.</h2><p>" + ex.ToString() + "</p>");
        }

        public static void SendNotifyMail(string subject, string body)
        {
            var config = Ioc.Get<SystemConfig>();
            if (config.Smtp.Enabled == false)
            {
                return;
            }
            SmtpSetting smtp = config.Smtp;
            SmtpClient client = new SmtpClient {Host = smtp.Host};
            MailMessage m = new MailMessage
            {
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                Subject = subject,
                From = new MailAddress(smtp.From)
            };
            m.To.Add(new MailAddress(smtp.To));
            m.Body = body;
            try
            {
                client.Send(m);
            }
            catch (Exception ex)
            {
                logger.Log("寄信失敗: " + ex);
            }
            //client.SendAsync(m, null);
        }
    }
}
