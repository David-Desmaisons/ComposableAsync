using System;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.WorkItems
{
    public sealed class AsyncActionWorkItem : AsyncWorkItem<object>
    {
        public AsyncActionWorkItem(Func<Task> @do)
            : base(async () => { await @do(); return null; })
        {
        }

        public AsyncActionWorkItem(Func<Task> @do, CancellationToken cancellationToken)
            : base(async () => { await @do(); return null;}, cancellationToken)
        {
        }
    }
}
