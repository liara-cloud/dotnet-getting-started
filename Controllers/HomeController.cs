using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspnet_blog_application.Models.ViewModels;
using MimeKit;
using MailKit.Net.Smtp;
using DotNetEnv;

namespace aspnet_blog_application.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult SendEmail(string email)
    {   
        // Email Information  
        Env.Load();
        string senderName  = Env.GetString("SENDER_NAME");
        string senderEmail = Env.GetString("SENDER_ADDRESS");
        string subject     = Env.GetString("EMAIL_SUBJECT");
        string body        = Env.GetString("EMAIL_BODY");

        // Email Instance
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress("Recipient", email));
        message.Subject = subject;

        // Creating The Body 
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        try
        {
            // Sending Email 
            using (var client = new SmtpClient())
            {
                client.Connect(Env.GetString("MAIL_HOST"), Env.GetInt("MAIL_PORT"), false);
                client.Authenticate(Env.GetString("MAIL_USERNAME"), Env.GetString("MAIL_PASSWORD"));
                client.Send(message);
                client.Disconnect(true);
            }

            ViewBag.Message = "Email Sent Successfully.";
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error In Sending Email: {ex.Message}";
        }


        return RedirectToAction("Index");
    }

}
