﻿
using System.Net;
using System.Net.Mail;

namespace ETICARET.WebUI.EmailService
{
    public static class MailHelper
    {
        public static bool SendMail(string body,string to,string subject,bool isHtml = true)
        {
            return SendMail(body, new List<string>() { to }, subject, isHtml);
        }

        private static bool SendMail(string body, List<string> to, string subject, bool isHtml)
        {
            bool result = false;
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress("ucuncubinyilakademimailservice@gmail.com");

                to.ForEach(
                    x =>
                    {
                        message.To.Add(new MailAddress(x));
                    }
                );

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                using (var smtp = new SmtpClient("smtp.gmail.com",587))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential("ucuncubinyilakademimailservice@gmail.com", "zetb vmba ittv dabk");
                    smtp.UseDefaultCredentials = false;
                    smtp.Send(message);
                    result = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = false;

            }

            return result;
        }
    }
}
