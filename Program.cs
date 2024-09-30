using System.Net;
using System.Net.Mail;
using dotenv.net;

DotEnv.Load(); // Load the environment variables from .env file

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/send-test-email", async context =>
{
    // Read SMTP settings from environment variables
    var smtpHost = Environment.GetEnvironmentVariable("MAIL_HOST");
    int smtpPort = int.Parse(Environment.GetEnvironmentVariable("MAIL_PORT") ?? "587");
    var smtpUser = Environment.GetEnvironmentVariable("MAIL_USER");
    var smtpPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD");
    var fromAddress = Environment.GetEnvironmentVariable("MAIL_FROM_ADDRESS") ?? "info@example.com";
    var toAddress = "recipient@example.com"; // Replace with recipient's email address

    // Create a new SmtpClient
    using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
    {
        smtpClient.EnableSsl = true; // Use TLS encryption
        smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);

        // Create the email message
        var mailMessage = new MailMessage(fromAddress, toAddress)
        {
            Subject = "Test Email",
            Body = "<h2>This is a test email sent from a .NET Core application using SMTP<h2>",
            IsBodyHtml = true
        };

        // Add custom headers
        mailMessage.Headers.Add("x-liara-tag", "test-tag");

        // Send the email
        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            await context.Response.WriteAsync("Test email sent successfully!");
        }
        catch (Exception ex)
        {
            await context.Response.WriteAsync($"Failed to send email: {ex.Message}");
        }
    }
});

app.Run();
