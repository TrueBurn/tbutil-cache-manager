using System.Collections.Generic;

namespace TbUtil.TbCacheManager.Test.CacheClasses
{
    public class SimpleClass
    {
        public string TestString { get; set; } = "Monkey breath";
        public int TestInt { get; set; } = 1986;
        public IEnumerable<string> TestArray { get; set; } = new List<string>() { "AWS", "Azure", "Google" };
    }
}

namespace TbUtil.TbCacheManager.Test.CacheClasses.NS2
{
    public class SimpleClass
    {
        public string TestString { get; set; } = "Apple Pie";
        public int TestInt { get; set; } = 2020;
        public IEnumerable<string> TestArray { get; set; } = new List<string>() { "MySql", "MsSql", "MariaDB" };
    }
}
