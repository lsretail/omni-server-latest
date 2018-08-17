using System;
using System.Net.Mail;

namespace LSOmni.Common.Util
{
    public static class Email
    {
        // http://msdn.microsoft.com/en-us/library/system.net.mail.smtpclient(v=vs.100).aspx
        public static void SendEmailAsHtml(string from, string to, string subject, string body, string attachmentFile = "", bool inLineContent = false)
        {
            // SMTP  Mail  
            using (MailMessage mail = new MailMessage())
            {
                //mail.from = new MailAddress(from); //this is ignored when reading from config file
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                if (inLineContent && (string.IsNullOrWhiteSpace(attachmentFile) == false))
                {
                    Attachment inlineLogo = new Attachment(attachmentFile); //@"C:\Desktop\Image.jpg");
                    mail.Attachments.Add(inlineLogo);
                    string contentId = "Image";
                    inlineLogo.ContentId = contentId;

                    //To make the image display as inline and not as attachment

                    inlineLogo.ContentDisposition.Inline = true;
                    inlineLogo.ContentDisposition.DispositionType = "inline"; //DispositionTypeNames.Inline;
                    mail.Body = mail.Body.Replace("cid:", "cid:" + contentId);
                }
                else if (string.IsNullOrWhiteSpace(attachmentFile) == false)
                {
                    Attachment attachment = new Attachment(attachmentFile);
                    mail.Attachments.Add(attachment);//attachment passed as parameter
                }

                //SmtpClient client = new SmtpClient(Host);//mailServer = "smtp.mail.yahoo.com" for YAHOO or "smtp.gmail.com" for GMAIL
                SmtpClient client = new SmtpClient(); //read from config file (mailSettings.config)

                client.Send(mail);
                mail.Dispose();
                client.Dispose();
            }
        }

        public static void SendEmail(string from, string to, string subject, string body, string attachmentFile = "")
        {
            // SMTP  Mail  
            using (MailMessage mail = new MailMessage())
            {
                //mail.from = new MailAddress(from); //this is ignored when reading from config file
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;
                if (string.IsNullOrWhiteSpace(attachmentFile) == false)
                {
                    Attachment attachment = new Attachment(attachmentFile);
                    mail.Attachments.Add(attachment);//attachment passed as parameter
                }

                //SmtpClient client = new SmtpClient(Host);//mailServer = "smtp.mail.yahoo.com" for YAHOO or "smtp.gmail.com" for GMAIL
                SmtpClient client = new SmtpClient(); //read from config file (mailSettings.config)

                client.Send(mail);
                mail.Dispose();
                client.Dispose();
            }
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            //safest to simple let .Net decide what it can send..
            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }

            //^[^@\s]+@[^@\s]+(\.[^@\s]+)+$    very simple check
            // \\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)* 

            //Regex rgxEmail = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
            //                           @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
            //                           @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            //return rgxEmail.IsMatch(email);
        }
    }
}
 
