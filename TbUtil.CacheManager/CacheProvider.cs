using CacheManager.Core;
using TbUtil.TbCacheManager.Configuration;
using TbUtil.TbCacheManager.Interfaces;

namespace TbUtil.TbCacheManager;

/// <summary>
/// Default implementation of a cache provider.
/// </summary>
/// <typeparam name="T">The type of data the cache will store.</typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0034:Simplify 'default' expression", Justification = "<Pending>")]
public sealed class CacheProvider<T> : ICacheProvider<T>
{
    private const string RedisConfigKey = "RedisConnection";
    private const string VersionSuffix = "-Version";

    private readonly string cacheRegion;
    private readonly IRedisConnectionMultiplexer redisConnection;

    private bool isDisposed = false;
    private ICacheInvalidator invalidator = null;

    /// <summary>
    /// Create an instance of <seealso cref="CacheProvider{T}"/>.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="cacheRegion"></param>
    public CacheProvider(CacheModule configuration, string cacheRegion = null)
    {
        // Get the type name and version for the key.
        this.cacheRegion = string.IsNullOrWhiteSpace(cacheRegion)
            ? $"{configuration.Environment}-{typeof(T).FullName}"
            : $"{configuration.Environment}-{cacheRegion}";

        if (configuration.Redis is not null)
        {
            redisConnection = CacheFactory.GetRedisConnectionMultiplexer(configuration.Redis.ConnectionString, null, null);
            invalidator = new CacheInvalidator(configuration.Environment, typeof(T).FullName, configuration.Redis.ConnectionString, configuration.Redis.DatabaseId);
        }

        ICacheManagerConfiguration config = ConfigurationBuilder.BuildConfiguration(settings =>
        {
            ConfigurationBuilderCacheHandlePart cache;

            if (configuration.Dictionary is not null && configuration.Redis is not null)
            {
                cache = settings.WithDictionaryHandle();

                if (configuration.Dictionary.ExpiryType != ExpiryType.None)
                {
                    cache.WithExpiration(ExpiryTypeToExpirationMode(configuration.Dictionary.ExpiryType), configuration.Dictionary.TimeOut);
                }

                cache = cache.And
                   .WithJsonSerializer(JsonSettings.Default, JsonSettings.Default)
                   .WithRedisConfiguration(RedisConfigKey, redisConnection.Multiplexer, configuration.Redis.DatabaseId)
                   .WithRedisBackplane(RedisConfigKey)
                   .WithRedisCacheHandle(RedisConfigKey, true);

                if (configuration.Redis.ExpiryType != ExpiryType.None)
                {
                    cache.WithExpiration(ExpiryTypeToExpirationMode(configuration.Redis.ExpiryType), configuration.Redis.TimeOut);
                }
            }
            else if (configuration.Dictionary is null && configuration.Redis is not null)
            {
                cache = settings
                        .WithJsonSerializer(JsonSettings.Default, JsonSettings.Default)
                        .WithRedisConfiguration(RedisConfigKey, redisConnection.Multiplexer, configuration.Redis.DatabaseId)
                        .WithRedisBackplane(RedisConfigKey)
                        .WithRedisCacheHandle(RedisConfigKey, true);

                if (configuration.Redis.ExpiryType != ExpiryType.None)
                {
                    cache.WithExpiration(ExpiryTypeToExpirationMode(configuration.Redis.ExpiryType), configuration.Redis.TimeOut);
                }
            }
            else
            {
                cache = settings.WithDictionaryHandle();

                if (configuration.Dictionary.ExpiryType != ExpiryType.None)
                {
                    cache.WithExpiration(ExpiryTypeToExpirationMode(configuration.Dictionary.ExpiryType), configuration.Dictionary.TimeOut);
                }
            }
        });

        CurrentCache = CacheManager.Core.CacheFactory.FromConfiguration<T>(config);
        VersionCache = CacheManager.Core.CacheFactory.FromConfiguration<long>(config);

    }

    /// <summary>
    /// Destructor in case object is not disposed.
    /// </summary>
    ~CacheProvider()
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
        invalidator.Clear();
    }

    /// <summary>
    /// Retrieve a specific item version from the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public long GetVersion(string key)
    {
        return VersionCache.Get($"{key}{VersionSuffix}", cacheRegion);
    }

    /// <summary>
    /// Retrieve a specific item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <returns>The cache item, or <c>default(T)</c> if the item was not found.</returns>
    public T Get(string key)
    {
        return CurrentCache.Get(key, cacheRegion);
    }

    /// <summary>
    /// Retrieve a specific versioned item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <returns>The cache item, or <c>default(T)</c> if the item was not found.</returns>
    public VersionedEntity<T> GetVersioned(string key)
    {
        VersionedEntity<T> versionedEntity = new();
        versionedEntity.Entity = CurrentCache.Get(key, cacheRegion);
        versionedEntity.Version = GetVersion(key);

        return versionedEntity;
    }

    /// <summary>
    /// Retrieve a specific item from the cache asynchronously or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObjectAsync">Asynchronous factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>
    public async Task<T> GetAsync(string key, Func<Task<T>> fetchObjectAsync)
    {
        T obj = Get(key);

        // If we found something then return.
        if (!EqualityComparer<T>.Default.Equals(obj, default))
        {
            return obj;
        }

        // If we cannot fetch the value then return.
        if (fetchObjectAsync is null)
        {
            return obj;
        }

        obj = await fetchObjectAsync();

        if (obj is not null)
        {
            Set(key, obj);
        }

        return obj;
    }

    /// <summary>
    /// Retrieve a specific versioned item from the cache asynchronously or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObjectAsync">Asynchronous factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>
    public async Task<VersionedEntity<T>> GetVersionedAsync(string key, Func<Task<VersionedEntity<T>>> fetchObjectAsync)
    {
        VersionedEntity<T> versionedEntity = new();
        versionedEntity.Entity = Get(key);

        // If we found something then return.
        if (versionedEntity.Entity is not null)
        {
            versionedEntity.Version = GetVersion(key);
            return versionedEntity;
        }

        // If we cannot fetch the value then return.
        if (fetchObjectAsync is null)
        {
            return versionedEntity;
        }

        versionedEntity = await fetchObjectAsync();

        if (versionedEntity is not null)
        {
            SetVersioned(key, versionedEntity.Entity, versionedEntity.Version);
        }

        return versionedEntity;
    }

    /// <summary>
    /// Retrieve a specific item from the cache or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObject">Factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>
    public T Get(string key, Func<T> fetchObject)
    {
        T obj = Get(key);

        // If we found something then return.
        if (!EqualityComparer<T>.Default.Equals(obj, default))
        {
            return obj;
        }

        // If we cannot fetch the value then return.
        if (fetchObject is null)
        {
            return obj;
        }

        obj = fetchObject();

        if (obj is not null)
        {
            Set(key, obj);
        }

        return obj;
    }

    /// <summary>
    /// Retrieve a specific versioned item from the cache or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObject">Factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>
    public VersionedEntity<T> GetVersioned(string key, Func<VersionedEntity<T>> fetchObject)
    {

        VersionedEntity<T> versionedEntity = new();
        versionedEntity.Entity = Get(key);

        // If we found something then return.
        if (!EqualityComparer<T>.Default.Equals(versionedEntity.Entity, default))
        {
            versionedEntity.Version = GetVersion(key);
            return versionedEntity;
        }

        // If we cannot fetch the value then return.
        if (fetchObject is null)
        {
            return versionedEntity;
        }

        versionedEntity = fetchObject();

        if (versionedEntity.Entity is not null)
        {
            SetVersioned(key, versionedEntity.Entity, versionedEntity.Version);
        }

        return versionedEntity;
    }

    /// <summary>
    /// Remove a specific item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to remove.</param>
    /// <returns><c>true</c> if the cache item was removed, <c>false</c> if the item was not found.</returns>
    public bool Remove(string key)
    {
        bool result = true;

        if (CurrentCache is not null)
        {
            CurrentCache.RemoveExpiration(key, cacheRegion);
            result = CurrentCache.Remove(key, cacheRegion);
        }
        if (invalidator is not null)
        {
            result = invalidator.Remove(key);
        }

        return result;
    }

    /// <summary>
    /// Remove a specific versioned item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to remove.</param>
    /// <returns><c>true</c> if the cache item was removed, <c>false</c> if the item was not found.</returns>
    public bool RemoveVersioned(string key)
    {
        bool result = true;

        if (CurrentCache is not null)
        {
            CurrentCache.RemoveExpiration(key, cacheRegion);
            result = CurrentCache.Remove(key, cacheRegion);
        }
        if (VersionCache is not null)
        {
            VersionCache.RemoveExpiration($"{key}{VersionSuffix}", cacheRegion);
            result = result && VersionCache.Remove($"{key}{VersionSuffix}", cacheRegion);
        }
        if (invalidator is not null)
        {
            result = invalidator.Remove(key) && invalidator.Remove($"{key}{VersionSuffix}");
        }

        return result;
    }

    /// <summary>
    /// Add or update an item in cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    public void Set(string key, T value)
    {
        CurrentCache.RemoveExpiration(key, cacheRegion);
        CurrentCache.Put(key, value, cacheRegion);
    }

    /// <summary>
    /// Add or update a versioned item in cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="verison">The cache item version to store in cache.</param>
    public void SetVersioned(string key, T value, long verison)
    {
        CurrentCache.RemoveExpiration(key, cacheRegion);
        CurrentCache.Put(key, value, cacheRegion);

        VersionCache.RemoveExpiration($"{key}{VersionSuffix}", cacheRegion);
        VersionCache.Put($"{key}{VersionSuffix}", verison, cacheRegion);
    }

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="slidingExpiration">The amount of time the cache item must not be accessed for before it gets removed.</param>
    public void Set(string key, T value, TimeSpan slidingExpiration)
    {
        CurrentCache.Put(key, value, cacheRegion);
        CurrentCache.Expire(key, cacheRegion, slidingExpiration);
    }

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="slidingExpiration">The amount of time the cache item must not be accessed for before it gets removed.</param>
    /// <param name="verison">The cache item version to store in cache.</param>
    public void SetVersioned(string key, T value, TimeSpan slidingExpiration, long verison)
    {
        CurrentCache.Put(key, value, cacheRegion);
        CurrentCache.Expire(key, cacheRegion, slidingExpiration);

        VersionCache.Put($"{key}{VersionSuffix}", verison, cacheRegion);
        VersionCache.Expire($"{key}{VersionSuffix}", cacheRegion, slidingExpiration);
    }

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="absoluteExpiration">The exact time the cache item must will get removed.</param>
    public void Set(string key, T value, DateTimeOffset absoluteExpiration)
    {
        CurrentCache.Put(key, value, cacheRegion);
        CurrentCache.Expire(key, cacheRegion, absoluteExpiration);
    }

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="absoluteExpiration">The exact time the cache item must will get removed.</param>
    /// <param name="verison">The cache item version to store in cache.</param>
    public void SetVersioned(string key, T value, DateTimeOffset absoluteExpiration, long verison)
    {
        CurrentCache.Put(key, value, cacheRegion);
        CurrentCache.Expire(key, cacheRegion, absoluteExpiration);

        VersionCache.Put($"{key}{VersionSuffix}", verison, cacheRegion);
        VersionCache.Expire($"{key}{VersionSuffix}", cacheRegion, absoluteExpiration);
    }

    /// <summary>
    /// If Redis is not connected then we can fallback to memory.
    /// </summary>
    private ICacheManager<T> CurrentCache { get; set; } = null;

    /// <summary>
    /// If Redis is not connected then we can fallback to memory.
    /// </summary>
    private ICacheManager<long> VersionCache { get; set; } = null;

    private static ExpirationMode ExpiryTypeToExpirationMode(ExpiryType type) => type switch
    {
        ExpiryType.Sliding => ExpirationMode.Sliding,
        ExpiryType.Absolute => ExpirationMode.Absolute,
        ExpiryType.None => ExpirationMode.None,
        _ => ExpirationMode.None,
    };

    private void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {

                if (CurrentCache is not null)
                {
                    CurrentCache.Dispose();
                    CurrentCache = null;
                }

                if (VersionCache is not null)
                {
                    VersionCache.Dispose();
                    VersionCache = null;
                }

                if (invalidator is not null)
                {
                    invalidator.Dispose();
                    invalidator = null;
                }

            }

            isDisposed = true;
        }
    }

}
