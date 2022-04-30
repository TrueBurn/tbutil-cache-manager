namespace TbUtil.TbCacheManager.Interfaces;

/// <summary>
/// Implementation requirements for a cache invalidator.
/// </summary>
public interface ICacheInvalidator : IDisposable
{
    /// <summary>
    /// Clear all cache items of the provided type.
    /// </summary>
    void Clear();

    /// <summary>
    /// Remove a specific item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to remove.</param>
    /// <returns><c>true</c> if the cache item was removed, <c>false</c> if the item was not found.</returns>
    bool Remove(string key);

    /// <summary>
    /// Remove a specific versioned item from the cache.
    /// </summary>
    /// <param name="key">The unique key associated with the cache item to remove.</param>
    /// <returns><c>true</c> if the cache item was removed, <c>false</c> if the item was not found.</returns>
    bool RemoveVersioned(string key);
}
