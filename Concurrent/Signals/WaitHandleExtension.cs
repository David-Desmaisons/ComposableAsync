using System.Threading;

namespace Concurrent.Signals 
{
    internal static class WaitHandleExtension 
    {
        public static bool WaitOne(this WaitHandle handle, CancellationToken cancellationToken)
        {
            var index = WaitHandle.WaitAny(new[] { handle, cancellationToken.WaitHandle });
            return (index == 0);
        }
    }
}
