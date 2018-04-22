using System;
using System.Threading;

namespace Concurrent.Signals 
{
    internal static class WaitHandleExtension 
    {
        public static void WaitOne(this WaitHandle handle, CancellationToken cancellationToken)
        {
            var index = WaitHandle.WaitAny(new[] { handle, cancellationToken.WaitHandle });
            if (index == 0)
                return;

            throw new OperationCanceledException(cancellationToken);
        }
    }
}
