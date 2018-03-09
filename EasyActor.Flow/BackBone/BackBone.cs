using EasyActor.Flow.Processor;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Flow.BackBone
{
    internal class BackBone<TRes, TProgress> : IBackbone<TRes, TProgress>
    {
        private readonly IDictionary<Type, object> _Processors;
        private readonly Lazy<CompositeDisposable> _Disposable = new Lazy<CompositeDisposable>();
        private readonly CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();
        private event EventHandler<object> _OnElement;

        internal BackBone(IDictionary<Type, object> processors)
        {
            _Processors = processors;
        }

        public IDisposable Connect<TMessage>(IObservable<TMessage> source)
        {
            if (_CancellationTokenSource.IsCancellationRequested)
                return Disposable.Empty;

            var disp = source.Subscribe(async message => await Process(message, null, CancellationToken.None));
            _Disposable.Value.Add(disp);
            return disp;
        }

        public void Dispose()
        {
            if (_Disposable.IsValueCreated)
            {
                _Disposable.Value.Dispose();
            }
            _CancellationTokenSource.Cancel();          
        }

        public async Task<TRes> Process<TMessage>(TMessage message, IProgress<TProgress> progress, CancellationToken cancellationToken)
        {
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _CancellationTokenSource.Token))
            {
                var token = linkedCts.Token;
                token.ThrowIfCancellationRequested();

                var messageType = typeof(TMessage);
                progress = progress ?? new NullProgess<TProgress>();

                try
                {
                    FireEvent(message);
                    var processor = _Processors[messageType] as IProcessor<TRes, TMessage, TProgress>;
                    return await processor.Process(message, this, progress, token);
                }
                catch (KeyNotFoundException)
                {
                    throw new ArgumentException($"No processor found for message {messageType}. Use register on BackBone builder to register processor.");
                }
            }
        }

        private void FireEvent(object element)
        {
            var onElement = _OnElement;
            onElement?.Invoke(this, element);
        }

        public IObservable<T> GetObservableMessage<T>() 
        {
            return Observable.FromEventPattern<object>(ea => _OnElement += ea, ea => _OnElement -= ea)
                             .Select(epa => epa.EventArgs).OfType<T>();
        }
    }
}
