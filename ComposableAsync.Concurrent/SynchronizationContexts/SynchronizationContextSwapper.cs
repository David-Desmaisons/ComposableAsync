using System;
using System.Threading;

namespace ComposableAsync.Concurrent.SynchronizationContexts 
{
    internal struct SynchronizationContextSwapper: IDisposable
    {
        private readonly SynchronizationContext _SynchronizationContext;

        public SynchronizationContextSwapper(SynchronizationContext synchronizationContext)
        {
            _SynchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        }

        public void Dispose() 
        {
            SynchronizationContext.SetSynchronizationContext(_SynchronizationContext);
        }
    }
}
