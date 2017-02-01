using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace clby.Extensions.Logging.MongoDB
{
    public class MongoDBLoggerSettings : IMongoDBLoggerSettings
    {

        public string ConnectionString { get; set; }

        public string DbName { get; set; }

        public string CollectionName { get; set; }


        public IChangeToken ChangeToken { get; set; }

        public bool IncludeScopes { get; set; }

        public IDictionary<string, LogLevel> Switches { get; set; } = new Dictionary<string, LogLevel>();

        public IMongoDBLoggerSettings Reload()
        {
            return this;
        }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            return Switches.TryGetValue(name, out level);
        }
    }
}
