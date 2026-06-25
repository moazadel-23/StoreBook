using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using StoreBook.Models;
using System.Net;
using System.Net.Mail;

namespace StoreBook.Utilities
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.FromEmail, _emailSettings.Password)
            };

            return client.SendMailAsync(
            new MailMessage(from: _emailSettings.FromEmail,
                            to: email,
                            subject,
                            htmlMessage
                            )
            {
                IsBodyHtml = true
            });
        }
    }
}
