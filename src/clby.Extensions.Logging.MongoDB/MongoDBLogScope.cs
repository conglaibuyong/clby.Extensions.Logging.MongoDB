using System;
using System.Threading;

namespace clby.Extensions.Logging.MongoDB
{
    public class MongoDBLogScope
    {
        private readonly string _name;
        private readonly object _state;

        internal MongoDBLogScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public MongoDBLogScope Parent { get; private set; }

        private static AsyncLocal<MongoDBLogScope> _value = new AsyncLocal<MongoDBLogScope>();

        public static MongoDBLogScope Current
        {
            set
            {
                _value.Value = value;
            }
            get
            {
                return _value.Value;
            }
        }

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new MongoDBLogScope(name, state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        public override string ToString()
        {
            return _state?.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}
