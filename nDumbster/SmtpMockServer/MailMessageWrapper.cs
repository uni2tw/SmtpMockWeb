using System;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace nDumbster.SmtpMockServer
{
    public class MailMessageWrapper
    {
        public string HostName { get; set; }
        public MailMessage MailMessage { get; set; }
        public DateTime SendTime { get; set; }  
        public string FileName { get; set; }

        public override string ToString()
        {
            if (MailMessage == null)
            {
                return String.Empty;
            }
            return String.Format("{0} by {1} at {2}", 
                MailMessage.Subject, MailMessage.From, SendTime);
        }

      
        public static string MailFolder
        {
            get;
            private set;
        }

        public static void Init(string mailFolder)
        {
            MailFolder = mailFolder;
            if (String.IsNullOrEmpty(MailFolder))
            {
                throw new Exception("MailFolder is null");
            }
            if (Directory.Exists(MailFolder) == false)
            {
                Directory.CreateDirectory(MailFolder);
            }
        }

        public static MailMessageWrapper ConvertFrom(SmtpMessage smtpMessage)
        {
            MailMessageWrapper message = new MailMessageWrapper();
            message.MailMessage = new MailMessage();
            DateTime sendTime;            
            LoadData(message.MailMessage, smtpMessage, out sendTime);
            message.SendTime = sendTime;
            message.FileName = DateTime.Now.Ticks + Guid.NewGuid().ToString() + ".eml";
            message.HostName = smtpMessage.HostName;
            message.MailMessage.Save(Path.Combine(MailFolder, message.FileName));
            return message;
        }

        private static void LoadData(MailMessage message, SmtpMessage sm, out DateTime sendTime)
        {
            sendTime = DateTime.Now;
            foreach (string key in sm.Headers)
            {
                if (key.Equals("To", StringComparison.OrdinalIgnoreCase))
                {
                    message.To.Add(sm.Headers[key]);
                }
                if (key.Equals("From", StringComparison.OrdinalIgnoreCase))
                {
                    message.From = new MailAddress(sm.Headers[key]);
                }
                if (key.Equals("Subject", StringComparison.OrdinalIgnoreCase))
                {
                    message.Subject = sm.Headers[key];
                }
            }

            if (sm.Headers["Date"] != null)
            {
                sendTime = DateTime.Parse(sm.Headers["Date"]);
            }

            var contentType = new System.Net.Mime.ContentType(sm.Headers["Content-Type"]);
            if (sm.Headers["Content-Transfer-Encoding"] != null &&
                sm.Headers["Content-Transfer-Encoding"].Equals("base64", StringComparison.OrdinalIgnoreCase))
            {
                if (contentType.CharSet != null)
                {
                    message.Body = Encoding.GetEncoding(contentType.CharSet).GetString(Convert.FromBase64String(sm.Body));
                }
                else
                {
                    message.Body = Encoding.UTF8.GetString(Convert.FromBase64String(sm.Body));
                }
                if (contentType.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    message.IsBodyHtml = true;
                }
                else
                {
                    message.IsBodyHtml = false;
                }
            }
            else
            {
                message.Body = sm.Body;
                message.IsBodyHtml = false;
            }
        }

    }
}