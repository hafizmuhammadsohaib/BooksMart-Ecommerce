using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BooksMart.Utilities
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecret { get; set; }
        public EmailSender(IConfiguration configuration)
        {
            SendGridSecret = configuration.GetValue<string>("SendGrid:SecretKey");
            if (string.IsNullOrEmpty(SendGridSecret))
            {
                // Log but don't throw - allow app to start
                Console.WriteLine("WARNING: SendGrid SecretKey is not configured");
            }
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(SendGridSecret))
            {
                Console.WriteLine($"Cannot send email - SendGrid not configured. Email: {email}, Subject: {subject}");
                return; // Or throw an exception if email is critical
            }
            var client = new SendGridClient(SendGridSecret);
            var from = new EmailAddress("hafizmuhammadsohaib24@gmail.com", "BooksMart");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
            //return client.SendEmailAsync(msg);
            await client.SendEmailAsync(msg);
        }
    }
}
