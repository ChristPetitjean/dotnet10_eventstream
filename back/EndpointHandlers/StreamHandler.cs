using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Channels;
using EventStreamBrowser.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamBrowser.EndpointHandlers;

public class StreamHandler
{
    public static async Task<IResult> HandleStream(
        HttpContext context,
        CancellationToken token,
        [FromServices] ChannelFactory channelFactory,
        [FromServices] ILogger<StreamHandler> logger)
    {
        // Retrieve the ClientId from the request cookie if exists, or from the response headers
        string? clientId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(clientId))
        {
            logger.LogWarning("Client ID is missing in the request. Cannot establish SSE connection.");
            return Results.BadRequest("Client ID is missing.");
        }

        // Get the channel for the specific client
        var channel = channelFactory.GetChannel(clientId);

        // if client close connexion, remove the channel
        token.Register(() =>
        {
            logger.LogInformation("Cancellation requested for client {ClientId}.", clientId);
            channelFactory.RemoveChannel(clientId);
        });

        // Return the SSE stream
        return TypedResults.ServerSentEvents(StreamSseItems(token, logger, clientId, channel));
    }

    private static async IAsyncEnumerable<SseItem<WeatherForecast>> StreamSseItems(
        [EnumeratorCancellation] CancellationToken cancellationToken,
        ILogger logger,
        string? clientId,
        Channel<WeatherForecast>? channel)
    {
        logger.LogInformation("Starting SSE stream for client {ClientId}.", clientId);

        while (!cancellationToken.IsCancellationRequested
                && channel is not null
                && await channel.Reader.WaitToReadAsync(cancellationToken))
        {
            // Read the next WeatherForecast from the channel
            var forecast = await channel.Reader.ReadAsync(cancellationToken);
            logger.LogInformation("Sending weather forecast to client {ClientId}: {Forecast}", clientId, forecast);

            // Yield the forecast as an SSE item
            yield return new SseItem<WeatherForecast>(forecast, "weatherforecast")
            {
                ReconnectionInterval = TimeSpan.FromSeconds(30),
            };
        }

        logger.LogInformation("SSE stream for client {ClientId} has ended.", clientId);
    }
}