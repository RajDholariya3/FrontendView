using System;
using System.Net.Mail;
using System.Net;

namespace FrontendView.Helper
{
    public class MailService
    {
        public void SendEmailNotification(string toEmail, string subject, string message)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("techverse311@gmail.com"),
                    Subject = subject,
                    Body = $"Dear {toEmail},\n\n{message}"
                };
                mail.To.Add(toEmail);

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("techverse311@gmail.com", "eigh jahj bqbm zdhn"); // Gmail App Password
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;

                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework instead of Console.WriteLine in production)
                Console.WriteLine("Email Error: " + ex.Message);
                throw; // Re-throw the exception so it can be caught in the controller
            }
        }
    }
}