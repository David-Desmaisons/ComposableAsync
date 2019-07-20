using System;

namespace ComposableAsync.Resilient.Retry.ExceptionFilter
{
    internal class NoThrow : IExceptionFilter
    {
        private NoThrow()
        {
        }

        public bool ShouldBeThrown(Exception exception) => false;

        internal static IExceptionFilter Instance { get; } = new NoThrow();
    }
}
