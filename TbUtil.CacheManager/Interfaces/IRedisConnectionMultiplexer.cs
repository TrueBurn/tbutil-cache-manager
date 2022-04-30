using StackExchange.Redis;

namespace TbUtil.TbCacheManager.Interfaces;

internal interface IRedisConnectionMultiplexer
{
    bool IsAvailable { get; }
    IConnectionMultiplexer Multiplexer { get; }
}