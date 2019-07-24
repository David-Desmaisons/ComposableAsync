using System;

namespace ComposableAsync.Resilient.ExceptionFilter
{
    internal interface IExceptionFilter
    {
        bool IsFiltered(Exception exception);
    }
}
