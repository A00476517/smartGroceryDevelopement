using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SmartGroceryApp.Utility
{
    public class EmailSender 
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("smartGroceryWeb@gmail.com");
                    mail.To.Add(email);
                    mail.Subject = Convert.ToString(subject);
                  
                    mail.Body = htmlMessage;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential("smartGroceryWeb@gmail.com", "xhykjagbwyhifwfq");


                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
