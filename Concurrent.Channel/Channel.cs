using System;
using System.Reactive.Subjects;

namespace Concurrent.Channel
{
    public sealed class Channel<T> : OutChannel<T>, IInChannel<T>
    {
        private readonly Subject<T> _Subject;
        public Channel() : base(new Subject<T>())
        {
            _Subject = (Subject<T>)Observable;
        }

        public void Dispose()
        {
            _Subject.OnCompleted();
            _Subject.Dispose();
        }

        public void OnNext(T value)
        {
            _Subject.OnNext(value);
        }

        public void OnError(Exception error)
        {
            _Subject.OnError(error);
        }

        public void OnCompleted()
        {
            _Subject.OnCompleted();
        }
    }
}
