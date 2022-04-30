namespace TbUtil.TbCacheManager;

/// <summary>
/// Available expiry types.
/// </summary>
public enum ExpiryType
{
    /// <summary>
    /// No automatic expiry.
    /// </summary>
    None,
    /// <summary>
    /// Expiry based on last access time.
    /// </summary>
    Sliding,
    /// <summary>
    /// Specific expiry time.
    /// </summary>
    Absolute
}
