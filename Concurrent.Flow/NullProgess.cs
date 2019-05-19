using System;

namespace Concurrent.Flow
{
    /// <summary>
    /// Progress implementation that does not do nothing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NullProgess<T> : IProgress<T>
    {
        /// <inheritdoc />
        public void Report(T value)
        {
        }
    }
}
