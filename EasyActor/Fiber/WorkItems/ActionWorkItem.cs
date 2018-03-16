using System;

namespace EasyActor.Fiber.WorkItems
{
    public sealed class ActionWorkItem : WorkItem<object>, IWorkItem
    {
        public ActionWorkItem(Action @do) : base(() => { @do(); return null; })
        {
        }
    }
}
