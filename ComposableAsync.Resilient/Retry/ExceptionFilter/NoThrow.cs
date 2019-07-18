using System;

namespace ComposableAsync.Retry.ExceptionFilter
{
    internal class NoThrow : IExceptionFilter
    {
        public bool ShouldBeThrown(Exception exception) => false;
    }
}
