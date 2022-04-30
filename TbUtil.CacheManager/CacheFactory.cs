using Elastic.Apm.StackExchange.Redis;
using StackExchange.Redis;
using System.Collections.Concurrent;
using TbUtil.TbCacheManager.Configuration;
using TbUtil.TbCacheManager.Interfaces;

namespace TbUtil.TbCacheManager;

/// <summary>
/// Provides convenient methods to create cache providers and invalidators.
/// </summary>
public static class CacheFactory
{

    private static readonly ICacheConfiguration cacheConfiguration = new CacheConfiguration();

    private static readonly IDictionary<string, IConnectionMultiplexer> redisMultiplexers = new ConcurrentDictionary<string, IConnectionMultiplexer>();
    private static readonly IDictionary<string, object> providerCache = new ConcurrentDictionary<string, object>();
    private static readonly IDictionary<string, ICacheInvalidator> invalidatorCache = new ConcurrentDictionary<string, ICacheInvalidator>();

    private static bool isElasticApmEnabled = false;

    /// <summary>
    /// Setup the factory to be able to generate appropriate providers and invalidators when using a backplane like Redis.
    /// </summary>
    /// <remarks>Calling this will clear any previously cached provider types./</remarks>
    /// <param name="configuration">The environment to separate caches and avoid key conflicts (Dev, QA, Staging etc)</param>
    /// <remarks>Sets up a dictionary cache with no expiry</remarks>
    public static void Setup(Action<ICacheConfiguration> configuration)
    {
        configuration(cacheConfiguration);
        providerCache.Clear();
        invalidatorCache.Clear();
    }

    /// <summary>
    /// Setup the factory to be able to generate appropriate providers and invalidators when using a backplane like Redis.
    /// </summary>
    /// <remarks>Calling this will clear any previously cached provider types./</remarks>
    /// <param name="environment">The environment to separate caches and avoid key conflicts (Dev, QA, Staging etc)</param>
    /// <remarks>Sets up a dictionary cache with no expiry</remarks>
    public static void Setup(string environment)
    {
        cacheConfiguration.SetDefault(environment);
        providerCache.Clear();
        invalidatorCache.Clear();
    }

    /// <summary>
    /// Enable Elastic Apm
    /// </summary>
    public static void EnableElasticApm()
    {
        isElasticApmEnabled = true;
    }

    /// <summary>
    /// Sets up cache invalidators with the specified configs.
    /// </summary>
    /// <param name="invalidatorConfigs"></param>
    public static void SetupCacheInvalidators(IList<CacheInvalidatorConfiguration> invalidatorConfigs)
    {

        //iterate through the configs
        foreach (CacheInvalidatorConfiguration invalidatorConfig in invalidatorConfigs)
        {
            //update existing invalidator
            if (invalidatorCache.ContainsKey(invalidatorConfig.InvalidatorKey))
            {
                invalidatorCache[invalidatorConfig.InvalidatorKey] = new CacheInvalidator(
                invalidatorConfig.Environment,
                invalidatorConfig.InvalidatorKey,
                invalidatorConfig.RedisConfig.ConnectionString,
                invalidatorConfig.RedisConfig.DatabaseId);

                continue;
            }

            //insert invalidator into collection
            invalidatorCache.Add(invalidatorConfig.InvalidatorKey,
                new CacheInvalidator(
                invalidatorConfig.Environment,
                invalidatorConfig.InvalidatorKey,
                invalidatorConfig.RedisConfig.ConnectionString,
                invalidatorConfig.RedisConfig.DatabaseId));
        }
    }

    internal static IRedisConnectionMultiplexer GetRedisConnectionMultiplexer(string redisConnectionString, Action onConnectionLost, Action onConnectionRestored)
    {
        if (!redisMultiplexers.ContainsKey(redisConnectionString))
        {
            redisMultiplexers[redisConnectionString] = ConnectionMultiplexer.ConnectAsync(redisConnectionString).GetAwaiter().GetResult();
            if (isElasticApmEnabled)
            {
                redisMultiplexers[redisConnectionString].UseElasticApm();
            }
        }

        IConnectionMultiplexer multiplexer = redisMultiplexers[redisConnectionString];

        return new RedisConnectionMultiplexer(multiplexer, onConnectionLost, onConnectionRestored);
    }

    /// <summary>
    /// Get an instance of a <seealso cref="ICacheInvalidator"/> by providing the type name.
    /// </summary>
    /// <remarks>The type name should be the <c>FullName</c> of a given class or value type.</remarks>
    /// <param name="typeName">The full type name of object that you want to invalidate.</param>
    /// <param name="cacheConfig">Cache configuration <see cref="CacheModule"/></param>
    /// <returns><seealso cref="ICacheInvalidator"/></returns>
    public static ICacheInvalidator GetCacheInvalidator(string typeName, CacheModule cacheConfig = null)
    {
        RedisConfiguration redisConfig = cacheConfig?.Redis ?? cacheConfiguration.Default.Redis;

        if (redisConfig is null)
        {
            return null;
        }

        if (invalidatorCache.ContainsKey($"{typeName}-{redisConfig.ConnectionString}-{redisConfig.DatabaseId}"))
        {
            return invalidatorCache[typeName];
        }

        invalidatorCache[typeName] = new CacheInvalidator(cacheConfig?.Environment ?? cacheConfiguration.Default.Environment, typeName, redisConfig.ConnectionString, redisConfig.DatabaseId);

        return invalidatorCache[typeName];
    }

    /// <summary>
    /// Get an instance of a <seealso cref="ICacheProvider{T}"/> for a specific type.
    /// </summary>
    /// <remarks>
    /// Providing the same type more than once will re-use the same underlying provider to simply cache sharing.
    /// The expiry settings will default to sliding for seven days.
    /// </remarks>
    /// <typeparam name="T">The data type the cache needs to store.</typeparam>
    /// <returns><seealso cref="ICacheProvider{T}"/></returns>
    public static ICacheProvider<T> GetCacheProvider<T>()
    {
        return GetCacheProvider<T>(cacheConfiguration.GetModule<T>(), null);
    }

    /// <summary>
    /// Get an instance of a <seealso cref="ICacheProvider{T}"/> for a specific type.
    /// </summary>
    /// <remarks>
    /// Providing the same type more than once will re-use the same underlying provider to simply cache sharing.
    /// The expiry settings will default to sliding for seven days.
    /// </remarks>
    /// <typeparam name="T">The data type the cache needs to store.</typeparam>
    /// <param name="region">Optionally specifies the cache region key. If none provided the types full name is used</param>
    /// <returns><seealso cref="ICacheProvider{T}"/></returns>
    public static ICacheProvider<T> GetCacheProvider<T>(string region)
    {
        return GetCacheProvider<T>(cacheConfiguration.Default, region);
    }

    /// <summary>
    /// Get an instance of a <seealso cref="ICacheProvider{T}"/> for a specific type.
    /// </summary>
    /// <typeparam name="T">The data type the cache needs to store.</typeparam>
    /// <param name="cacheConfiguration">Cache configuration for the proivder</param>
    /// <returns><seealso cref="ICacheProvider{T}"/></returns>
    public static ICacheProvider<T> GetCacheProvider<T>(CacheModule cacheConfiguration)
    {
        return GetCacheProvider<T>(cacheConfiguration, null);
    }

    /// <summary>
    /// Get an instance of a <seealso cref="ICacheProvider{T}"/> for a specific type.
    /// </summary>
    /// <typeparam name="T">The data type the cache needs to store.</typeparam>
    /// <param name="cacheConfiguration">Cache configuration for the proivder</param>
    /// <param name="region">Optionally specify that the cache region.</param>
    /// <returns><seealso cref="ICacheProvider{T}"/></returns>
    public static ICacheProvider<T> GetCacheProvider<T>(CacheModule cacheConfiguration, string region)
    {
        string regionKey = string.IsNullOrWhiteSpace(region) ? typeof(T).FullName : region;

        string key = $"{regionKey}-{GetProviderKey(cacheConfiguration)}";

        if (providerCache.ContainsKey(key))
        {
            return providerCache[key] as ICacheProvider<T>;
        }

        ICacheProvider<T> provider = new CacheProvider<T>(cacheConfiguration, region);

        providerCache[key] = provider;

        return provider;
    }

    private static string GetProviderKey(CacheModule cacheConfiguration)
    {
        List<string> keyBuilder = new();

        if (cacheConfiguration?.Dictionary is not null)
        {
            keyBuilder.Add($"D-{cacheConfiguration.Dictionary}");
        }

        if (cacheConfiguration?.Redis is not null)
        {
            keyBuilder.Add($"R-{cacheConfiguration.Redis}");
        }

        return string.Join("-", keyBuilder);
    }

}
