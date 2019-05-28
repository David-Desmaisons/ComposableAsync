using System;
using System.Diagnostics;
using System.Threading;

namespace ComposableAsync.Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal sealed class ActionCancellableWorkItem : CancellableWorkItem<object>
    {
        internal ActionCancellableWorkItem(Action @do, CancellationToken cancellationToken) : base(() => { @do.Invoke(); return null; }, cancellationToken)
        {
        }
    }
}
