using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace EasyActor.FlowTest
{
    public class ObservableHelper
    {
        private HashSet<IObserver<string>> _observers = new HashSet<IObserver<string>>();

        public IObservable<string> GetObservable()
        {
            return Observable.Create<string>(
                (observer) =>
                {
                    lock (this)
                    {
                        _observers.Add(observer);
                    }
                    return Disposable.Create(() =>
                    {
                        lock (this)
                        {
                            _observers.Remove(observer);
                        }
                    });
                });
        }

        public void Observe(string observed)
        {
            OnObserved(obs => obs.OnNext(observed), () => _observers.Clear());
        }

        public void OnEnd()
        {
            OnObserved(obs => obs.OnCompleted());
        }

        private void OnObserved(Action<IObserver<string>> action, Action final=null)
        {
            lock (this)
            {
                foreach (var obs in _observers)
                {
                    action(obs);
                }

                if (final != null)
                    final();
            }
        }
    }
}
