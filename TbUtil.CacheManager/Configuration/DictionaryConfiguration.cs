using TbUtil.TbCacheManager.Configuration;

namespace TbUtil.TbCacheManager;

/// <summary>
/// Dictionary Configuration
/// </summary>
public class DictionaryConfiguration : BaseConfiguration
{
    /// <summary>
    /// Dictionary Configuration
    /// </summary>
    /// <param name="expiryType"></param>
    /// <param name="timeOut"></param>
    public DictionaryConfiguration(ExpiryType expiryType, TimeSpan timeOut) : base(expiryType, timeOut) { }
    /// <summary>
    /// Dictionary Configuration
    /// </summary>
    /// <param name="expiryType"></param>
    public DictionaryConfiguration(ExpiryType expiryType) : base(expiryType, default) { }
}
