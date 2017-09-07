using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Serilog.Sinks.Azure.TableStorage.Compact
{
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory)
            : base(() => Task.Factory.StartNew(valueFactory))
        {
        }

        public AsyncLazy(Func<Task<T>> valueFactory)
            : base(() => Task.Factory.StartNew(valueFactory).Unwrap())
        {
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return Value.GetAwaiter();
        }
    }
}
