namespace TbUtil.TbCacheManager.Interfaces;

/// <summary>
/// Implementation requirements for a cache provider.
/// </summary>
/// <typeparam name="T">The type of data the cache will store.</typeparam>
public interface ICacheProvider<T> : ICacheInvalidator, IDisposable
{

    /// <summary>
    /// Add or update an item in cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    void Set(string key, T value);

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="absoluteExpiration">The exact time the cache item must will get removed.</param>
    void Set(string key, T value, DateTimeOffset absoluteExpiration);

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="slidingExpiration">The amount of time the cache item must not be accessed for before it gets removed.</param>
    void Set(string key, T value, TimeSpan slidingExpiration);

    /// <summary>
    /// Retrieve a specific item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <returns>The cache item, or <c>default(T)</c> if the item was not found.</returns>
    T Get(string key);

    /// <summary>
    /// Retrieve a specific item from the cache or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObject">Factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>
    T Get(string key, Func<T> fetchObject);

    /// <summary>
    /// Retrieve a specific item from the cache asynchronously or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObjectAsync">Asynchronous factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>
    Task<T> GetAsync(string key, Func<Task<T>> fetchObjectAsync);

    /// <summary>
    /// Retrieve a specific item version from the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    long GetVersion(string key);

    /// <summary>
    /// Retrieve a specific versioned item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <returns>The cache item, or <c>default(T)</c> if the item was not found.</returns>
    VersionedEntity<T> GetVersioned(string key);

    /// <summary>
    /// Retrieve a specific versioned item from the cache or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObject">Factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>    
    VersionedEntity<T> GetVersioned(string key, Func<VersionedEntity<T>> fetchObject);

    /// <summary>
    /// Retrieve a specific versioned item from the cache asynchronously or add it automatically if it doesn't exist.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to retrieve.</param>
    /// <param name="fetchObjectAsync">Asynchronous factory method to get the cache value if it is not found.</param>
    /// <returns>The cached item value or the result of the factory method.</returns>
    Task<VersionedEntity<T>> GetVersionedAsync(string key, Func<Task<VersionedEntity<T>>> fetchObjectAsync);

    /// <summary>
    /// Add or update a versioned item in cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="verison">The cache item version to store in cache.</param>    
    void SetVersioned(string key, T value, long verison);

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="slidingExpiration">The amount of time the cache item must not be accessed for before it gets removed.</param>
    /// <param name="verison">The cache item version to store in cache.</param>
    void SetVersioned(string key, T value, TimeSpan slidingExpiration, long verison);

    /// <summary>
    /// Add or update an item in cache with sliding expiration.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to store.</param>
    /// <param name="value">The cache item value to store in cache.</param>
    /// <param name="absoluteExpiration">The exact time the cache item must will get removed.</param>
    /// <param name="verison">The cache item version to store in cache.</param>
    void SetVersioned(string key, T value, DateTimeOffset absoluteExpiration, long verison);
}