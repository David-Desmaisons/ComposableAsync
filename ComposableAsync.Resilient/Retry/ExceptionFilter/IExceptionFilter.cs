using System;

namespace ComposableAsync.Resilient.Retry.ExceptionFilter
{
    internal interface IExceptionFilter
    {
        bool ShouldBeThrown(Exception exception);
    }
}
