using System;

namespace ComposableAsync.Retry.ExceptionFilter
{
    internal interface IExceptionFilter
    {
        bool ShouldBeThrown(Exception exception);
    }
}
