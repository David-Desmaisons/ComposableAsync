using System;
using System.Linq;
using System.Threading.Tasks;
using EasyActor.Disposable;

namespace EasyActor.Pipeline
{
    public class ClosedPipeline<T> : IClosedPipeline<T>
    {
        private readonly Func<T, Task> _Process;
        private readonly IAsyncDisposable _Disposable;

        public ClosedPipeline(IConsumer<T> init)
        {
            _Disposable = ActorDisposable.StopableToAsyncDisposable(init as IActorLifeCycle);
            _Process = init.Consume;
        }

        private ClosedPipeline(Func<T, Task> init, IAsyncDisposable disposable)
        {
            _Process = init;
            _Disposable = disposable;
        }

        public static ClosedPipeline<T> CreateClosedPipeline<TTransform>(Func<T, Task<TTransform>> init, IConsumer<TTransform> trans)
        {
            return new ClosedPipeline<T>(async (tin) =>
            {
                var value = await init(tin);
                await trans.Consume(value);
            },
            ActorDisposable.StopableToAsyncDisposable(trans as IActorLifeCycle));
        }

        public static ClosedPipeline<T> CreateClosedPipeline<TTransform>(Func<T, Task<TTransform>> init, params IConsumer<TTransform>[] next)
        {
            return new ClosedPipeline<T>(async (tin) =>
            {
                var value = await init(tin);
                await Task.WhenAll(next.Select(n => n.Consume(value)).ToArray());
            },
            ActorDisposable.StopableToAsyncDisposable(next.OfType<IActorLifeCycle>()));
        }

        public Task Consume(T entry)
        {
            return _Process(entry);
        }

        public IDisposable Connect(IObservable<T> source)
        {
            return source.Subscribe(t => _Process(t));
        }

        public void Dispose() => _Disposable.Dispose();

        public Task DisposeAsync() => _Disposable.DisposeAsync();
    }
}
