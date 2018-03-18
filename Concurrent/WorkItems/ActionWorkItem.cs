using System;

namespace Concurrent.WorkItems
{
    public sealed class ActionWorkItem : WorkItem<object>
    {
        public ActionWorkItem(Action @do = null) : base(() => { @do?.Invoke(); return null; })
        {
        }
    }
}
