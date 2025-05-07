using System.Net.ServerSentEvents;
using EventStreamBrowser.Services;
using EventStreamBrowser.Endpoints; // Add this for StreamHandler

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ChannelFactory>();
builder.Services.AddHostedService<MyProcessor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Use the cookie middleware
app.UseCookieMiddleware();

// Configure the HTTP request pipeline.
app.MapGet("/stream", StreamHandler.HandleStream);

app.UseCors("AllowAllOrigins");

app.Run();
