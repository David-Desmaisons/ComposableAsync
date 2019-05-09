using System;
using System.Reactive.Subjects;

namespace Concurrent.Channel
{
    /// <summary>
    /// <see cref="IInChannel&lt;T&gt;"/>  implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Channel<T> : OutChannel<T>, IInChannel<T>
    {
        private readonly Subject<T> _Subject;

        /// <summary>
        /// Instantiate a new channel
        /// </summary>
        public Channel() : base(new Subject<T>())
        {
            _Subject = (Subject<T>)Observable;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Subject.OnCompleted();
            _Subject.Dispose();
        }

        /// <inheritdoc />
        public void OnNext(T value)
        {
            _Subject.OnNext(value);
        }

        /// <inheritdoc />
        public void OnError(Exception error)
        {
            _Subject.OnError(error);
        }

        /// <inheritdoc />
        public void OnCompleted()
        {
            _Subject.OnCompleted();
        }
    }
}
