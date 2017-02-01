using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace clby.Extensions.Logging.MongoDB
{
    public class MongoDBLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, MongoDBLogger> _loggers = new ConcurrentDictionary<string, MongoDBLogger>();

        private readonly Func<string, LogLevel, bool> _filter;
        private IMongoDBLoggerSettings _settings;

        public MongoDBLoggerProvider(Func<string, LogLevel, bool> filter, bool includeScopes)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filter = filter;
            _settings = new MongoDBLoggerSettings()
            {
                IncludeScopes = includeScopes,
            };
        }

        public MongoDBLoggerProvider(IMongoDBLoggerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _settings = settings;

            if (_settings.ChangeToken != null)
            {
                _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        private void OnConfigurationReload(object state)
        {
            _settings = _settings.Reload();

            foreach (var logger in _loggers.Values)
            {
                logger.Filter = GetFilter(logger.Name, _settings);
                logger.IncludeScopes = _settings.IncludeScopes;
            }

            if (_settings?.ChangeToken != null)
            {
                _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }

        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, CreateLoggerImplementation);
        }

        private MongoDBLogger CreateLoggerImplementation(string name)
        {
            return new MongoDBLogger(name, GetFilter(name, _settings), _settings);
        }

        private Func<string, LogLevel, bool> GetFilter(string name, IMongoDBLoggerSettings settings)
        {
            if (_filter != null)
            {
                return _filter;
            }

            if (settings != null)
            {
                foreach (var prefix in GetKeyPrefixes(name))
                {
                    LogLevel level;
                    if (settings.TryGetSwitch(prefix, out level))
                    {
                        return (n, l) => l >= level;
                    }
                }
            }

            return (n, l) => false;
        }

        private IEnumerable<string> GetKeyPrefixes(string name)
        {
            while (!string.IsNullOrEmpty(name))
            {
                yield return name;
                var lastIndexOfDot = name.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    yield return "Default";
                    break;
                }
                name = name.Substring(0, lastIndexOfDot);
            }
        }

        public void Dispose()
        { }
    }
}
