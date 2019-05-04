using System;
using System.Diagnostics;

namespace Concurrent.WorkItems
{
    [DebuggerNonUserCode]
    internal sealed class DispatchItem : IWorkItem
    {
        private readonly Action _Do;
        internal DispatchItem(Action @do)
        {
            _Do = @do;
        }

        public void Cancel() { }

        public void Do() => _Do();
    }
}
