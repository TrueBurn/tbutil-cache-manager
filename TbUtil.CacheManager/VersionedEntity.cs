namespace TbUtil.TbCacheManager;

/// <summary>
/// Class to house a verioned cache item.
/// </summary>
/// <typeparam name="T"></typeparam>
public class VersionedEntity<T>
{
    /// <summary>
    /// The cached item
    /// </summary>
    public T Entity { get; set; }
    /// <summary>
    /// The version of the cached item
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Class to house a verioned cache item.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="version"></param>
    public VersionedEntity(T entity, long version)
    {
        Entity = entity;
        Version = version;
    }

    /// <summary>
    /// Class to house a verioned cache item.
    /// </summary>
    public VersionedEntity()
    {
    }
}
