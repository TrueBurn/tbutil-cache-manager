using TbUtil.TbCacheManager.Configuration;

namespace TbUtil.TbCacheManager.Interfaces;

/// <summary>
/// Interface for the cache manager
/// </summary>
public interface ICacheConfiguration
{
    /// <summary>
    /// The default cache module
    /// </summary>
    CacheModule Default { get; }
    
    /// <summary>
    /// Set the default for the environment
    /// </summary>
    /// <param name="environment"></param>
    void SetDefault(string environment);
    
    /// <summary>
    /// Set the provided cache module as the default
    /// </summary>
    /// <param name="cacheConfiguration"></param>
    void SetDefault(CacheModule cacheConfiguration);
    
    /// <summary>
    /// Add an extra cache module
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cacheConfiguration"></param>
    void AddModule<T>(CacheModule cacheConfiguration);
    
    /// <summary>
    /// Gets a configured cache model for the defined type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    CacheModule GetModule<T>();
}
