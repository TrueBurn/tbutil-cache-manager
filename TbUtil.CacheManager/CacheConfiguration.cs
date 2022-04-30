using TbUtil.TbCacheManager.Configuration;
using TbUtil.TbCacheManager.Interfaces;

namespace TbUtil.TbCacheManager;

/// <inheritdoc />
public class CacheConfiguration : ICacheConfiguration
{
    private static readonly Dictionary<Type, CacheModule> typeCacheConfigurations = new();

    /// <inheritdoc />
    public CacheModule Default { get; private set; }

    /// <inheritdoc />
    public void AddModule<T>(CacheModule cacheModule)
    {
        typeCacheConfigurations[typeof(T)] = cacheModule;
    }

    /// <inheritdoc />
    public CacheModule GetModule<T>()
    {
        return typeCacheConfigurations.TryGetValue(typeof(T), out CacheModule config) ? config : Default;
    }

    /// <inheritdoc />
    public void SetDefault(CacheModule cacheModule)
    {
        Default = cacheModule;
    }

    /// <inheritdoc />
    public void SetDefault(string environment)
    {
        Default = new CacheModule(environment) { Dictionary = new DictionaryConfiguration(ExpiryType.None) };
    }

}
