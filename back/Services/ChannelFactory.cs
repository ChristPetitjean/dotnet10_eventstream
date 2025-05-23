using System.Collections.Concurrent;
using System.Threading.Channels;

namespace EventStreamBrowser.Services;

public class ChannelFactory(ILogger<ChannelFactory> logger)
{
    private readonly ConcurrentDictionary<string, Channel<WeatherForecast>> _channels = new();

    public Channel<WeatherForecast> GetChannel(string clientId)
    {
        return _channels.GetOrAdd(clientId, _ =>
            Channel.CreateBounded<WeatherForecast>(new BoundedChannelOptions(20)
            {
                SingleReader = false,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.DropOldest
            }));
    }

    public List<string> GetAllClientIds()
    {
        return _channels.Keys.ToList();
    }

    internal void RemoveChannel(string clientId)
    {
        if (_channels.TryRemove(clientId, out var channel))
        {
            channel.Writer.TryComplete();
            GC.SuppressFinalize(channel); // Suppress finalization for the channel
            logger.LogInformation("Channel for client {clientId} removed.", clientId);
        }
        else
        {
            logger.LogWarning("Channel for client {clientId} not found.", clientId);
        }
    }
}