using System;

namespace ComposableAsync.Resilient.ExceptionFilter
{
    internal class NoThrow : IExceptionFilter
    {
        private NoThrow()
        {
        }

        public bool IsFiltered(Exception exception) => false;

        internal static IExceptionFilter Instance { get; } = new NoThrow();
    }
}
