using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TbUtil.TbCacheManager.Interfaces;

namespace TbUtil.TbCacheManager.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the cache manager services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="cacheConfiguration"></param>
    public static void AddCustomCache(this IServiceCollection serviceCollection, Action<ICacheConfiguration> cacheConfiguration)
    {

        CacheFactory.Setup(cacheConfiguration);

        serviceCollection.AddElasticApmIfEnable();

    }

    /// <summary>
    /// Enabled Elasitc APM if enable in appsettings.json
    /// </summary>
    /// <param name="serviceCollection"></param>
    public static void AddElasticApmIfEnable(this IServiceCollection serviceCollection)
    {
        try
        {
            if (serviceCollection.BuildServiceProvider().GetService<IConfiguration>().GetValue<bool>("ElasticApm:Enabled"))
            {
                CacheFactory.EnableElasticApm();
            }
        }
        catch (Exception)
        {

        }
    }


}
