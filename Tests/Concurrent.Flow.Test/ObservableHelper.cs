using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Concurrent.Flow.Test
{
    public class ObservableHelper
    {
        private readonly HashSet<IObserver<string>> _Observers = new HashSet<IObserver<string>>();

        public IObservable<string> GetObservable()
        {
            return Observable.Create<string>(
                (observer) =>
                {
                    lock (this)
                    {
                        _Observers.Add(observer);
                    }
                    return Disposable.Create(() =>
                    {
                        lock (this)
                        {
                            _Observers.Remove(observer);
                        }
                    });
                });
        }

        public void Observe(string observed)
        {
            OnObserved(obs => obs.OnNext(observed), () => _Observers.Clear());
        }

        public void OnEnd()
        {
            OnObserved(obs => obs.OnCompleted());
        }

        private void OnObserved(Action<IObserver<string>> action, Action final=null)
        {
            lock (this)
            {
                foreach (var obs in _Observers)
                {
                    action(obs);
                }

                if (final != null)
                    final();
            }
        }
    }
}
