using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TodoBackend.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender()
        {

        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string fromMail = "testproject3120@gmail.com";
            string fromPassword = "informatika2019";

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = subject;
            message.To.Add(new MailAddress(email));
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };
            smtpClient.Send(message);
            return Task.CompletedTask;
        }
    }
}
