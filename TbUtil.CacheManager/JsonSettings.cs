using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TbUtil.TbCacheManager;

internal static class JsonSettings
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        Formatting = Formatting.None,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public static JsonSerializerSettings Default => Settings;
}

