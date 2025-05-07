using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using EventStreamBrowser.Services;

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
app.MapGet("/stream", (HttpContext context, CancellationToken token, ChannelFactory channelFactory, ILogger<Program> logger) =>
{
    // Retrieve the ClientId from the request cookie if exists
    // or from the response headers if it was set in a previous middelware
    string? clientId = context.Request.Cookies["ClientId"]
    ?? context.Response.GetTypedHeaders().SetCookie.FirstOrDefault(c => c.Name == "ClientId")?.Value.Value;
    if (string.IsNullOrEmpty(clientId))
    {
        logger.LogWarning("Client ID is missing in the request.");
        return Results.BadRequest("Client ID is missing.");
    }

    // Get the channel for the specific client
    var channel = channelFactory.GetChannel(clientId);

    token.Register(() =>
    {
        logger.LogInformation("Cancellation requested for client {ClientId}.", clientId);
        channelFactory.RemoveChannel(clientId);
    });

    // Define the Server-Sent Events stream
    async IAsyncEnumerable<SseItem<WeatherForecast>> StreamSseItems([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting SSE stream for client {ClientId}.", clientId);

        while (!cancellationToken.IsCancellationRequested && await channel.Reader.WaitToReadAsync())
        {
            // Read the next WeatherForecast from the channel
            var forecast = await channel.Reader.ReadAsync();
            logger.LogInformation("Sending weather forecast to client {ClientId}: {Forecast}", clientId, forecast);

            // Yield the forecast as an SSE item
            yield return new SseItem<WeatherForecast>(forecast, "weatherforecast")
            {
                ReconnectionInterval = TimeSpan.FromSeconds(30),
            };
        }

        logger.LogInformation("SSE stream for client {ClientId} has ended.", clientId);
    }

    // Return the SSE stream
    return TypedResults.ServerSentEvents(StreamSseItems(token));
});

app.UseCors("AllowAllOrigins");

app.Run();
