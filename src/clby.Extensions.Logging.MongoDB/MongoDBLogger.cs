using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Text;

namespace clby.Extensions.Logging.MongoDB
{
    public class MongoDBLogger : ILogger
    {
        private static IMongoDatabase mdb = null;
        private static string CollectionName = string.Empty;

        private static readonly string _loglevelPadding = ": ";
        private static readonly string _messagePadding;
        private static readonly string _newLineWithMessagePadding;

        private Func<string, LogLevel, bool> _filter;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        public Func<string, LogLevel, bool> Filter
        {
            get { return _filter; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _filter = value;
            }
        }

        public bool IncludeScopes { get; set; }

        public string Name { get; }

        static MongoDBLogger()
        {
            var logLevelString = GetLogLevelString(LogLevel.Information);
            _messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
            _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
        }

        public MongoDBLogger(string name, Func<string, LogLevel, bool> filter, IMongoDBLoggerSettings settings)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Filter = filter ?? ((category, logLevel) => true);
            IncludeScopes = settings.IncludeScopes;

            var Client = new MongoClient(settings.ConnectionString);
            mdb = Client.GetDatabase(settings.DbName);
            CollectionName = settings.CollectionName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, Name, eventId.Id, message, exception);
            }
        }

        public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            var logBuilder = _logBuilder;
            _logBuilder = null;

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            var logLevelString = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                logLevelString = GetLogLevelString(logLevel);

                logBuilder.Append(logLevelString);
                logBuilder.Append(_loglevelPadding);
                logBuilder.Append(logName);
                logBuilder.Append("[");
                logBuilder.Append(eventId);
                logBuilder.AppendLine("]");

                if (IncludeScopes)
                {
                    GetScopeInformation(logBuilder);
                }

                logBuilder.Append(_messagePadding);
                var len = logBuilder.Length;
                logBuilder.AppendLine(message);
                logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
            }

            var exceptionString = string.Empty;
            if (exception != null)
            {
                exceptionString = exception.ToString();
                logBuilder.AppendLine(exceptionString);
            }

            if (logBuilder.Length > 0)
            {
                var logMessage = logBuilder.ToString();

                var log = new BsonDocument();
                log.Add("LogLevelString", logLevelString);
                log.Add("LogName", logName);
                log.Add("EventId", eventId);
                log.Add("Data", logMessage);
                log.Add("Exception", exceptionString);
                log.Add("CreateTime", DateTime.Now);

                //效率不行啊，考虑先放内存，定时批量写入数据库
                mdb
                    .GetCollection<BsonDocument>(string.Format("{0}_{1}", CollectionName, DateTime.Now.ToString("yyyyMM")))
                    .InsertOneAsync(log);
            }

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Filter(Name, logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return MongoDBLogScope.Push(Name, state);
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private void GetScopeInformation(StringBuilder builder)
        {
            var current = MongoDBLogScope.Current;
            string scopeLog = string.Empty;
            var length = builder.Length;

            while (current != null)
            {
                if (length == builder.Length)
                {
                    scopeLog = $"=> {current}";
                }
                else
                {
                    scopeLog = $"=> {current} ";
                }

                builder.Insert(length, scopeLog);
                current = current.Parent;
            }
            if (builder.Length > length)
            {
                builder.Insert(length, _messagePadding);
                builder.AppendLine();
            }
        }

    }
}
