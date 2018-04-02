using System;
using System.Diagnostics;

namespace Concurrent.WorkItems
{
    [DebuggerNonUserCode]
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
