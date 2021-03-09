//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace EventRouterFunc
//{
//    // Add to function class to use
//    //private readonly IConfigCacheClient configCacheClient;
//    //public EventRouterFunc(IConfigCacheClient configCacheClient, TelemetryConfiguration telemetryConfiguration)
//    //{
//    //    this.configCacheClient = configCacheClient;
//    //    this.telemetryClient = new TelemetryClient(telemetryConfiguration);
//    //}

//    /// <summary>
//    /// Cache Class Implementation that encapsulates Redis Cache
//    /// </summary>
//    public class ConfigCacheClient : IConfigCacheClient
//    {
//        private ConnectionMultiplexer connectionMultiplexer;

//        public ConfigCacheClient()
//        {
//            string cacheConnection = Environment.GetEnvironmentVariable("redisConnectionString");
//            this.connectionMultiplexer = ConnectionMultiplexer.Connect(cacheConnection);
//        }

//        public string GetItem(string key)
//        {
//            //            IDatabase cache = connectionMultiplexer.GetDatabase();
//            //            return cache.StringGet(key);
//            return "";
//        }
//    }

//    /// <summary>
//    /// Cache Interface
//    /// </summary>
//    public interface IConfigCacheClient
//    {
//        string GetItem(string key);
//    }
//}
