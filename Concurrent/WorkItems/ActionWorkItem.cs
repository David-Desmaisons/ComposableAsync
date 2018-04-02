using System;
using System.Diagnostics;

namespace Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    public sealed class ActionWorkItem : WorkItem<object>
    {
        public ActionWorkItem(Action @do) : base(() => { @do.Invoke(); return null; })
        {
        }
    }
}
