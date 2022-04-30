using StackExchange.Redis;
using TbUtil.TbCacheManager.Interfaces;

namespace TbUtil.TbCacheManager;

internal class RedisConnectionMultiplexer : IRedisConnectionMultiplexer
{

    public RedisConnectionMultiplexer(IConnectionMultiplexer multiplexer, Action onConnectionLost, Action onConnectionRestored)
    {
        Multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
        IsAvailable = true;

        Multiplexer.ConnectionFailed += (sender, args) =>
        {
            IsAvailable = false;
            onConnectionLost?.Invoke();
        };

        Multiplexer.ConnectionRestored += (sender, args) =>
        {
            IsAvailable = true;
            onConnectionRestored?.Invoke();
        };
    }

    public bool IsAvailable { get; private set; }

    public IConnectionMultiplexer Multiplexer { get; }

}
