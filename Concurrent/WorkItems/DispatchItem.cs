using System;

namespace Concurrent.WorkItems
{
    public sealed class DispatchItem : IWorkItem
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
