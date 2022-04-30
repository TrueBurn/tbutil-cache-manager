namespace TbUtil.TbCacheManager.Configuration;

/// <summary>
/// Cache Module
/// </summary>
public class CacheModule
{
    /// <summary>
    /// Cache Module
    /// </summary>
    /// <param name="environment"></param>
    public CacheModule(string environment) => Environment = environment;

    /// <summary>
    /// Environment
    /// </summary>
    public string Environment { get; private set; }
    /// <summary>
    /// Dictionary configuration
    /// </summary>
    public DictionaryConfiguration Dictionary { get; set; }
    /// <summary>
    /// Redis configuration
    /// </summary>
    public RedisConfiguration Redis { get; set; }
}