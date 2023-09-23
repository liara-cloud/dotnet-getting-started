using System;
using System.Net;
using System.Net.Mail;
using System.IO;
using dotenv.net;

class Program
{
    static void Main()
    {
        DotEnv.Load(); // بارگذاری متغیرهای محیطی از فایل .env

        string mailHost = Environment.GetEnvironmentVariable("MAIL_HOST");
        int mailPort = int.Parse(Environment.GetEnvironmentVariable("MAIL_PORT"));
        string mailUser = Environment.GetEnvironmentVariable("MAIL_USERNAME");
        string mailPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD");

        // SMTP Conf
        SmtpClient client = new SmtpClient(mailHost)
        {
            Port = mailPort,
            Credentials = new NetworkCredential(mailUser, mailPassword),
            EnableSsl = true
        };

        // Creating and Sending Email  
        MailMessage message = new MailMessage("info@alinajmabadi.ir", "alinajmabadizadeh2002@gmail.com",
         "hello", "hello from dotnet!");
        try
        {
            client.Send(message);
            Console.WriteLine("email sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error in sending email: {ex.Message}");
        }
    }
}