namespace TbUtil.TbCacheManager.Configuration;

/// <summary>
/// Cache Invalidator Configuration
/// </summary>
public class CacheInvalidatorConfiguration
{
    /// <summary>
    /// Redis Config
    /// </summary>
    public RedisConfiguration RedisConfig { get; set; }
    /// <summary>
    /// Invalidator Key
    /// </summary>
    public string InvalidatorKey { get; set; }
    /// <summary>
    /// Environment
    /// </summary>
    public string Environment { get; set; }
}
