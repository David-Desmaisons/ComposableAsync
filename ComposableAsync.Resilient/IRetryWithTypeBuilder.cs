using System;

namespace ComposableAsync.Resilient
{
    /// <summary>
    /// Retry <see cref="IDispatcher"/> builder filtering exception based on type
    /// </summary>
    public interface IRetryWithTypeBuilder : IRetryBuilderWithWait
    {
        /// <summary>
        /// Add the exception type to be caught
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IRetryWithTypeBuilder And<T>() where T : Exception;
    }
}
