using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ScheduledJobService>();

var app = builder.Build();

app.MapGet("/", () => "Hello, World!");

app.MapPost("/execute", () =>
{
    Console.WriteLine("The script is executed");
    return Results.Ok("Script executed successfully");
});

app.Run();
