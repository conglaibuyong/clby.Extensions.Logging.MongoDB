using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace clby.Extensions.Logging.MongoDB
{
    public interface IMongoDBLoggerSettings
    {
        string ConnectionString { get; }

        string DbName { get; }

        string CollectionName { get; }


        bool IncludeScopes { get; }

        IChangeToken ChangeToken { get; }

        bool TryGetSwitch(string name, out LogLevel level);

        IMongoDBLoggerSettings Reload();
    }
}
