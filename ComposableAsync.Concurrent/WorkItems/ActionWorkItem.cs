using System;
using System.Diagnostics;

namespace ComposableAsync.Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal sealed class ActionWorkItem : WorkItem<object>
    {
        internal ActionWorkItem(Action @do) : base(() => { @do.Invoke(); return null; })
        {
        }
    }
}
