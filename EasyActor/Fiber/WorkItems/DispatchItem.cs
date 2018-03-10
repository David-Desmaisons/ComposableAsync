using System;

namespace EasyActor.Fiber.WorkItems
{
    internal class DispatchItem : IWorkItem
    {
        private readonly Action _Do;
        public DispatchItem(Action @do)
        {
            _Do = @do;
        }

        public void Cancel() { }

        public void Do() => _Do();
    }
}
