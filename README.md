[![.NET](https://github.com/TrueBurn/tbutil-tb-cache-manager/actions/workflows/dotnet.yml/badge.svg)](https://github.com/TrueBurn/tbutil-tb-cache-manager/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/TrueBurn/tbutil-tb-cache-manager/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/TrueBurn/tbutil-tb-cache-manager/actions/workflows/codeql-analysis.yml)
[![SecurityCodeScan](https://github.com/TrueBurn/tbutil-tb-cache-manager/actions/workflows/securitycodescan.yml/badge.svg)](https://github.com/TrueBurn/tbutil-tb-cache-manager/actions/workflows/securitycodescan.yml)

# TB Util Caching

This caching library provides a simplified abstraction over [CacheManager](http://cachemanager.michaco.net/) by wrapping cache configuration into generic providers and invalidators.

The library currently supports Redis as a backplane if you provide a connection to a Redis server.
A backplane allows all instances of your application to remain up to date no matter which process adds or removes cache items.
When an item is removed from a remote process, a pub/sub signal is sent to all connected clients to remove their key from memory as well so they never serve stale data.
More information on cache synchronization can be found here: [Cache Synchronization](http://cachemanager.michaco.net/documentation/CacheManagerCacheSynchronization)

The state of the connection to Redis is monitored so outages will switch to a secondary system managed, in-memory cache until the connection is restored.
Cache items stored during the outage will be lost when it switched back over to the backplane version, items removed will be removed in Redis to ensure that other consumers don't get stale data.

JSON was the choice made for serializing objects.
Having the data in cache using JSON allows it to be easily read by humans for investigation, small JSON payloads are faster to serialize than binary ([source](https://maxondev.com/serialization-performance-comparison-c-net-formats-frameworks-xmldatacontractserializer-xmlserializer-binaryformatter-json-newtonsoft-servicestack-text/)), and JSON is also far more lenient when deserialilzing if object structures change.
___

### CacheFactory

The factory provides a simple way to create providers and invalidators while internally caching their instances to avoid expensive configuration and connection operations.
You should largely avoid creating your own instances and make use of the simple factory methods instead.

At the start of your application or service, you should setup the factory with the necessary information it needs to create instances for you.

```csharp
// The environment is used as a prefix to ensure no conflicts on shared Redis servers.
// Debug | TST | PROD
var environment = "Debug";

// Optional Redis server to use as a backplane.
var redisServer = "127.0.0.1:6379,ssl=False,allowAdmin=True";

// Optional Redis database number.
var redisDatabaseId = 10

CacheFactory.Setup(environment, redisServer, redisDatabaseId);
```

If you do not call `CacheFactory.Setup`, all caching will be in-memory only. To make use of the backplane facility you should always setup the factory as early as possible.

___

### Cache Providers

Cache providers are also invalidators allowing you to remove and clear cache items.
To create a provider you use the `CacheFactory` and provide a type and any default expiry settings.

```csharp
// Optional default type of expiry, sliding will reset the time whenever an item is accessed, absolute will remove from cache as soon as the wait time is over.
var expiryType = ExpiryType.Sliding;

// Optional time for when to expire items in the cache.
var expiryTime = TimeSpan.FromMinutes(10);

// Optional indication of whether the data is volatile.
// Volatile data is very short-lived but must not be removed under memory pressure.
// Non-volatile data will be internally managed where it could be removed before the expiry under memory pressure situations.
// Be careful with volatile data because you could potentially run into out-of-memory situations with large amounts of unmanaged data.
var isVolatile = true;

// Get an instance of a cache provider that stores DeviceOrder classes.
ICacheProvider<DeviceOrder> loginUserCache = CacheFactory.GetCacheProvider<DeviceOrder>(expiryType, expiryTime, isVolatile);
```

In the example above any items will be removed from the cache if they are not access in within 10 minutes.

When adding items to the cache you can override the default expiry using the overloads available.

```csharp
// Use the default expiry settings.
loginUserCache.Set("user-id-999", deviceObject);

// Expire this item exactly 15 minutes from now.
loginUserCache.Set("user-id-123", deviceObject123, DateTime.UtcNow.AddMinutes(15));

// Expire this item if it is not accessed for 30 minutes.
loginUserCache.Set("user-id-456", deviceObject456, TimeSpan.FromMinutes(30));
```

Retrieving from cache is also as simple as calling the `Get` method with the key. If the item is not found you will get `default(T)` as a return value.

```csharp
// Get user 999, if not found then it will be null
var deviceObject = loginUserCache.Get("user-id-999");
if (deviceObject is null)
{
  // Not found in the cache :(
}
else
{
  // Found the device object in the cache :)
}
```

Factory `Get` methods are also provided to automatically populate the cache if the item does not exist.

```csharp
// Get cached number, if not found then it set the cache item to 42 and return the value.
var number = numberCache.Get("my-number", () => 42);
```
___


### Tools
[Redis on Windows](https://github.com/MicrosoftArchive/redis/releases) - Run a local Redis server and connect to it using `127.0.0.1:6379,ssl=False,allowAdmin=True` as your connection string.

[Redis Desktop Manager](https://redisdesktop.com/) - A simple and free GUI client to connect to Redis and view cache keys and values.

