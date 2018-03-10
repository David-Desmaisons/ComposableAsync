using System;

namespace EasyActor.Fiber.WorkItems
{
    internal class ActionWorkItem : WorkItem<object>, IWorkItem
    {
        public ActionWorkItem(Action @do) : base(() => { @do(); return null; })
        {
        }
    }
}
