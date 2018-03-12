using System;
using System.Reactive.Subjects;

namespace EasyActor.Channel
{
    public sealed class InChannel<T> : Channel<T>, IInChannel<T>
    {
        private readonly Subject<T> _Subject;
        public InChannel() : base(new Subject<T>())
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
