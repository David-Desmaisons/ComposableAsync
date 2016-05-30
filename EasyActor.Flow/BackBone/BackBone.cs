using EasyActor.Flow.Processor;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Flow.BackBone
{
    internal class BackBone<TRes, TProgress> : IBackbone<TRes, TProgress>
    {
        private readonly IDictionary<Type, object> _Processors;
        private Lazy<CompositeDisposable> _Disposable = new Lazy<CompositeDisposable>();
        private CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();

        internal BackBone(IDictionary<Type, object> processors)
        {
            _Processors = processors;
        }

        public IDisposable Connect<TMessage>(IObservable<TMessage> source)
        {
            if (_CancellationTokenSource.IsCancellationRequested)
                return Disposable.Empty;

            var disp = source.Subscribe(async message => await Process(message, null, _CancellationTokenSource.Token));
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
            _CancellationTokenSource.Token.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();

            var messageType = typeof(TMessage);
            progress = progress ?? new NullProgess<TProgress>();
            try
            {
                var processor = _Processors[messageType] as IProcessor<TRes, TMessage, TProgress>;
                return await processor.Process(message, this, progress, cancellationToken);
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(string.Format("No processor found for message {0}. Use register on BackBone builder to register processor.", messageType));
            }  
        }
    }
}
