using CacheManager.Core;
using System.Collections.Concurrent;
using TbUtil.TbCacheManager.Interfaces;

namespace TbUtil.TbCacheManager;

/// <summary>
/// Default implementation of a cache invalidator.
/// </summary>
public sealed class CacheInvalidator : ICacheInvalidator
{
    private readonly string cacheRegion;
    private readonly IRedisConnectionMultiplexer redisConnection;

    private ConcurrentBag<string> offlineRemovals = new();
    private bool offlineClear = false;
    private bool isDisposed = false;
    private ICacheManager<object> redisManager = null;

    private const string VersionSuffix = "-Version";

    /// <summary>
    /// Create an instance of <seealso cref="CacheInvalidator"/>.
    /// </summary>
    /// <param name="environment">The environment to separate caches and avoid key conflicts (Dev, QA, Staging etc)</param>
    /// <param name="typeName">The full type name of object that you want to invalidate.</param>
    /// <param name="redisConnectionString">The Redis connection string to use as a backplane.</param>
    /// <param name="redisDatabaseId">The optional database number/ID to further separate caches and avoid conflicts.</param>
    public CacheInvalidator(string environment, string typeName, string redisConnectionString, int redisDatabaseId)
    {
        cacheRegion = $"{environment ?? string.Empty}-{typeName}";

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            redisConnection = CacheFactory.GetRedisConnectionMultiplexer(redisConnectionString, null, OnRedisConnectionRestored);
            SetupRedisManager(redisDatabaseId);
        }
    }

    /// <summary>
    /// Destructor in case object is not disposed.
    /// </summary>
    ~CacheInvalidator()
    {
        Dispose(false);
    }

    /// <summary>
    /// Dispose internal resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Clear all cache items of the provided type.
    /// </summary>
    public void Clear()
    {
        if (redisManager is not null)
        {
            if (redisConnection?.IsAvailable ?? false)
            {
                redisManager.ClearRegion(cacheRegion);
            }
            else
            {
                // Need to remove these when Redis comes back to life.
                offlineClear = true;
            }
        }
    }

    /// <summary>
    /// Remove a specific item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to remove.</param>
    /// <returns><c>true</c> if the cache item was removed, <c>false</c> if the item was not found.</returns>
    public bool Remove(string key)
    {
        if (redisManager is not null)
        {
            if (redisConnection?.IsAvailable ?? false)
            {
                return redisManager.Remove(key, cacheRegion);
            }
            else
            {
                // Need to remove these when Redis comes back to life.
                offlineRemovals.Add(key);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Remove a specific versioned item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to remove.</param>
    /// <returns><c>true</c> if the cache item was removed, <c>false</c> if the item was not found.</returns>
    public bool RemoveVersioned(string key)
    {
        if (redisManager is not null)
        {
            if (redisConnection?.IsAvailable ?? false)
            {
                return redisManager.Remove($"{key}{VersionSuffix}", cacheRegion);
            }
            else
            {
                // Need to remove these when Redis comes back to life.
                offlineRemovals.Add(key);

                return true;
            }
        }

        return false;
    }

    private void SetupRedisManager(int redisDatabaseId)
    {
        const string RedisConfigKey = "RedisConnection";

        ICacheManagerConfiguration config = ConfigurationBuilder.BuildConfiguration(settings =>
        {
            settings.WithJsonSerializer(JsonSettings.Default, JsonSettings.Default)
                    .WithRedisConfiguration(RedisConfigKey, redisConnection.Multiplexer, redisDatabaseId)
                    .WithRedisBackplane(RedisConfigKey)
                    .WithRedisCacheHandle(RedisConfigKey, true);
        });

        redisManager = CacheManager.Core.CacheFactory.FromConfiguration<object>(config);
    }

    private void OnRedisConnectionRestored()
    {
        if (offlineClear)
        {
            offlineClear = false;
            redisManager.ClearRegion(cacheRegion);
        }
        else
        {
            foreach (var cacheKey in offlineRemovals)
            {
                redisManager.Remove(cacheKey, cacheRegion);
            }
        }

        offlineRemovals = new ConcurrentBag<string>();
    }

    private void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing && redisManager is not null)
            {
                redisManager.Dispose();
                redisManager = null;
            }

            isDisposed = true;
        }
    }


}