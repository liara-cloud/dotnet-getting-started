using System.Net;
using System.Net.Mail;
using dotenv.net;

DotEnv.Load(); 

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/send-test-email", async context =>
{
    var smtpHost = Environment.GetEnvironmentVariable("MAIL_HOST");
    int smtpPort = int.Parse(Environment.GetEnvironmentVariable("MAIL_PORT") ?? "587");
    var smtpUser = Environment.GetEnvironmentVariable("MAIL_USER");
    var smtpPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD");
    var fromAddress = Environment.GetEnvironmentVariable("MAIL_FROM_ADDRESS") ?? "info@example.com";
    var toAddress = "some@example.com"; 


    using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
    {
        smtpClient.EnableSsl = true; 
        smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);

        var mailMessage = new MailMessage(fromAddress, toAddress)
        {
            Subject = "Test Email",
            Body = "<h2>This is a test email sent from a .NET Core application using SMTP<h2>",
            IsBodyHtml = true
        };

        mailMessage.CC.Add("test.one@example.com");
        mailMessage.CC.Add("test.two@example.com");       

        // mailMessage.Bcc.Add("test.one@example.com");
        // mailMessage.Bcc.Add("test.two@example.com");

        mailMessage.Headers.Add("x-liara-tag", "test-tag");

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
