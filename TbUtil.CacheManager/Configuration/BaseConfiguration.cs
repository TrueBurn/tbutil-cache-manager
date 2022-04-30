namespace TbUtil.TbCacheManager.Configuration;

/// <summary>
/// Base configuration class
/// </summary>
public abstract class BaseConfiguration
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="expiryType"></param>
    /// <param name="timeOut"></param>
    public BaseConfiguration(ExpiryType expiryType, TimeSpan timeOut)
    {
        ExpiryType = expiryType;
        TimeOut = timeOut;
    }

    /// <summary>
    /// Returns configured value
    /// </summary>
    public TimeSpan TimeOut { get; internal set; }

    /// <summary>
    /// Returns configured value
    /// </summary>
    public ExpiryType ExpiryType { get; internal set; }

    /// <summary>
    /// Convertes instance to a string representation
    /// </summary>
    public override string ToString() => $"{ExpiryType}-{TimeOut}";
}
