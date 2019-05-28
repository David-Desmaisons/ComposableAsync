using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ComposableAsync.Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal sealed class AsyncActionWorkItem : AsyncWorkItem<object>
    {
        internal AsyncActionWorkItem(Func<Task> @do)
            : base(async () => { await @do(); return null; })
        {
        }
    }
}
