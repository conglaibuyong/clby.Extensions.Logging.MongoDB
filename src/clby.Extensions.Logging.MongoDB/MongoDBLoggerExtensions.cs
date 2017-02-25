using clby.Extensions.Logging.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace clby.Extensions.Logging
{
    public static class MongoDBLoggerExtensions
    {
        /*
        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory)
        {
            return factory.AddMongoDB(includeScopes: false);
        }
        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory, bool includeScopes)
        {
            factory.AddMongoDB((n, l) => l >= LogLevel.Information, includeScopes);
            return factory;
        }
        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory, LogLevel minLevel)
        {
            factory.AddMongoDB(minLevel, includeScopes: false);
            return factory;
        }
        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory, LogLevel minLevel, bool includeScopes)
        {
            factory.AddMongoDB((category, logLevel) => logLevel >= minLevel, includeScopes);
            return factory;
        }
        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory, Func<string, LogLevel, bool> filter)
        {
            factory.AddMongoDB(filter, includeScopes: false);
            return factory;
        }
        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory, Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            factory.AddProvider(new MongoDBLoggerProvider(filter, includeScopes));
            return factory;
        }
        */
        
        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory, IMongoDBLoggerSettings settings)
        {
            factory.AddProvider(new MongoDBLoggerProvider(settings));
            return factory;
        }

        public static ILoggerFactory AddMongoDB(this ILoggerFactory factory, IConfiguration configuration)
        {
            var settings = new ConfigurationMongoDBLoggerSettings(configuration);
            return factory.AddMongoDB(settings);
        }
    }
}
