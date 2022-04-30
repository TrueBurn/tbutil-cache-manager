namespace TbUtil.TbCacheManager.Configuration;

/// <summary>
/// Redis Configuration
/// </summary>
public class RedisConfiguration : BaseConfiguration
{
    /// <summary>
    /// Redis Configuration
    /// </summary>
    /// <param name="expiryType"></param>
    /// <param name="timeOut"></param>
    public RedisConfiguration(ExpiryType expiryType, TimeSpan timeOut) : base(expiryType, timeOut) { }
    /// <summary>
    /// Redis Configuration
    /// </summary>
    /// <param name="expiryType"></param>
    public RedisConfiguration(ExpiryType expiryType) : base(expiryType, default) { }

    /// <summary>
    /// Connection String
    /// </summary>
    public string ConnectionString { get; set; }
    /// <summary>
    /// Database Id
    /// </summary>
    public int DatabaseId { get; set; }

    /// <summary>
    /// Convert the configuration to a string
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{base.ToString()}-{ConnectionString}-{DatabaseId}";
}