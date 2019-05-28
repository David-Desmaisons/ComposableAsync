using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal sealed class AsyncActionCancellableWorkItem : AsyncCancellableWorkItem<object>
    {
        internal AsyncActionCancellableWorkItem(Func<Task> @do, CancellationToken cancellationToken)
            : base(async () => { await @do(); return null;}, cancellationToken)
        {
        }
    }
}
