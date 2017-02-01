using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;

namespace clby.Extensions.Logging.MongoDB
{
    public class ConfigurationMongoDBLoggerSettings : IMongoDBLoggerSettings
    {

        public string ConnectionString
        {
            get
            {
                var value = (_configuration["ConnectionString"] ?? "").ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
                else
                {
                    var message = $"Configuration value '{value}' for setting '{nameof(ConnectionString)}' is not supported.";
                    throw new InvalidOperationException(message);
                }
            }
        }

        public string DbName
        {
            get
            {
                var value = (_configuration["DbName"] ?? "").ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
                else
                {
                    var message = $"Configuration value '{value}' for setting '{nameof(DbName)}' is not supported.";
                    throw new InvalidOperationException(message);
                }
            }
        }

        public string CollectionName
        {
            get
            {
                var value = (_configuration["CollectionName"] ?? "").ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
                else
                {
                    var message = $"Configuration value '{value}' for setting '{nameof(CollectionName)}' is not supported.";
                    throw new InvalidOperationException(message);
                }
            }
        }


        private readonly IConfiguration _configuration;

        public ConfigurationMongoDBLoggerSettings(IConfiguration configuration)
        {
            _configuration = configuration;
            ChangeToken = configuration.GetReloadToken();
        }

        public IChangeToken ChangeToken { get; private set; }

        public bool IncludeScopes
        {
            get
            {
                bool includeScopes;
                var value = _configuration["IncludeScopes"];
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
                else if (bool.TryParse(value, out includeScopes))
                {
                    return includeScopes;
                }
                else
                {
                    var message = $"Configuration value '{value}' for setting '{nameof(IncludeScopes)}' is not supported.";
                    throw new InvalidOperationException(message);
                }
            }
        }

        public IMongoDBLoggerSettings Reload()
        {
            ChangeToken = null;
            return new ConfigurationMongoDBLoggerSettings(_configuration);
        }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            var switches = _configuration.GetSection("LogLevel");
            if (switches == null)
            {
                level = LogLevel.None;
                return false;
            }

            var value = switches[name];
            if (string.IsNullOrEmpty(value))
            {
                level = LogLevel.None;
                return false;
            }
            else if (Enum.TryParse<LogLevel>(value, out level))
            {
                return true;
            }
            else
            {
                var message = $"Configuration value '{value}' for category '{name}' is not supported.";
                throw new InvalidOperationException(message);
            }
        }
    }
}