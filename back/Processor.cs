using System.Threading.Channels;
using EventStreamBrowser.Services;

public class MyProcessor : BackgroundService
{
    private readonly ChannelFactory _channelFactory;
    private readonly ILogger<MyProcessor> _logger;

    public MyProcessor(ChannelFactory channelFactory, ILogger<MyProcessor> logger)
    {
        _channelFactory = channelFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Simulate some work
            await Task.Delay(2000, stoppingToken);

            // Get all connected clients
            var connectedClients = _channelFactory.GetAllClientIds();

            if (connectedClients.Count == 0)
            {
                _logger.LogInformation("No connected clients to send weather forecasts.");
                continue;
            }

            // Select a random client
            var randomClientId = connectedClients[Random.Shared.Next(connectedClients.Count)];
            var clientChannel = _channelFactory.GetChannel(randomClientId);

            // Generate a random number of forecasts
            int number = Random.Shared.Next(1, 5);

            for (int i = 0; i < number; i++)
            {
                var forecast = new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now),
                    Random.Shared.Next(-20, 55),
                    WeatherForecast.summaries[Random.Shared.Next(WeatherForecast.summaries.Length)]
                );

                // Publish the forecast to the selected client's channel
                await clientChannel.Writer.WriteAsync(forecast, stoppingToken);
                _logger.LogInformation("Weather forecast sent to client {ClientId}: {Forecast}", randomClientId, forecast);
            }
        }
    }
}