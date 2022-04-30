using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TbUtil.TbCacheManager.Configuration;
using TbUtil.TbCacheManager.Test.CacheClasses;

namespace TbUtil.TbCacheManager.Test;

[TestClass]
public class CacheFactoryTests
{
    [TestMethod]
    public void Same_Type_Returns_Same_Instance()
    {
        CacheFactory.Setup("Test");

        // This ensures this internal provider "singleton" caching of the factory is working.
        Interfaces.ICacheProvider<SimpleClass> cache1 = CacheFactory.GetCacheProvider<SimpleClass>();
        Interfaces.ICacheProvider<SimpleClass> cache2 = CacheFactory.GetCacheProvider<SimpleClass>();

        Assert.AreEqual(cache1.GetHashCode(), cache2.GetHashCode());
    }

    [TestMethod]
    public void Return_Object_Using_Provided_Cache_Region()
    {
        CacheFactory.Setup("Test");

        string key = "1";
        SimpleClass value = new();
        // This ensures this internal provider "singleton" caching of the factory is working.
        Interfaces.ICacheProvider<SimpleClass> cache1 = CacheFactory.GetCacheProvider<SimpleClass>(region: "Test");
        cache1.Set(key, value);

        Interfaces.ICacheProvider<SimpleClass> cache2 = CacheFactory.GetCacheProvider<SimpleClass>(region: "Test");
        SimpleClass value2 = cache2.Get(key);

        Assert.IsTrue(object.ReferenceEquals(value, value2));
    }

    [TestMethod]
    public void Return_Object_Not_Providing_Cache_Region()
    {
        CacheFactory.Setup("Test");
        
        string key = "1";
        SimpleClass value = new();
        // This ensures this internal provider "singleton" caching of the factory is working.
        Interfaces.ICacheProvider<SimpleClass> cache1 = CacheFactory.GetCacheProvider<SimpleClass>(region: "Test");
        cache1.Set(key, value);

        Interfaces.ICacheProvider<SimpleClass> cache2 = CacheFactory.GetCacheProvider<SimpleClass>();
        SimpleClass value2 = cache2.Get(key);

        Assert.IsFalse(object.ReferenceEquals(value, value2));
    }

    [TestMethod]
    public void Setup_Clears_Provider_Cache()
    {
        // Even though using the same type, we'll get a new cache provider if calling setup and resetting things.
        Interfaces.ICacheProvider<SimpleClass> cache1 = CacheFactory.GetCacheProvider<SimpleClass>();

        CacheFactory.Setup((string)null);

        Interfaces.ICacheProvider<SimpleClass> cache2 = CacheFactory.GetCacheProvider<SimpleClass>();

        Assert.IsFalse(ReferenceEquals(cache1, cache2));
    }

    [TestMethod]
    public void Different_Namespace_Returns_Different_Instance()
    {
        CacheFactory.Setup("Test");

        // This ensures that the full class and namespace is used to cache not just the class name.
        Interfaces.ICacheProvider<SimpleClass> cache1 = CacheFactory.GetCacheProvider<SimpleClass>();
        Interfaces.ICacheProvider<CacheClasses.NS2.SimpleClass> cache2 = CacheFactory.GetCacheProvider<CacheClasses.NS2.SimpleClass>();

        Assert.IsFalse(ReferenceEquals(cache1, cache2));
    }


    [TestMethod]
    public void Different_Settings_Returns_Different_Instance()
    {
        CacheFactory.Setup("Test");

        CacheModule configuration = new(string.Empty)
        {
            Dictionary = new DictionaryConfiguration(ExpiryType.None, TimeSpan.FromDays(1))
        };

        // This ensures that different settings for the same type results in different instances to ensure not just the type is cached.
        Interfaces.ICacheProvider<SimpleClass> cache1 = CacheFactory.GetCacheProvider<SimpleClass>();
        Interfaces.ICacheProvider<SimpleClass> cache2 = CacheFactory.GetCacheProvider<SimpleClass>();
        Interfaces.ICacheProvider<SimpleClass> cache3 = CacheFactory.GetCacheProvider<SimpleClass>(configuration);
        Interfaces.ICacheProvider<SimpleClass> cache4 = CacheFactory.GetCacheProvider<SimpleClass>(configuration);

        Assert.IsTrue(ReferenceEquals(cache1, cache2));
        Assert.IsFalse(ReferenceEquals(cache1, cache3));
        Assert.IsFalse(ReferenceEquals(cache1, cache4));

        Assert.IsTrue(ReferenceEquals(cache2, cache1));
        Assert.IsFalse(ReferenceEquals(cache2, cache3));
        Assert.IsFalse(ReferenceEquals(cache2, cache4));

        Assert.IsFalse(ReferenceEquals(cache3, cache1));
        Assert.IsFalse(ReferenceEquals(cache3, cache2));
        Assert.IsTrue(ReferenceEquals(cache3, cache4));
    }


}