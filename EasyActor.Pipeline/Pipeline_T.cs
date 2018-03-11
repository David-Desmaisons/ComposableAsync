using System;
using System.Linq;
using System.Threading.Tasks;
using EasyActor.Disposable;
using EasyActor.TaskHelper;

namespace EasyActor.Pipeline
{
    public class Pipeline<TIn, TOut> : IPipeline<TIn, TOut>
    {
        private readonly Func<TIn, Task<TOut>> _Process;
        private readonly IAsyncDisposable _Disposable;

        internal Pipeline(ITransformer<TIn, TOut> init)
        {
            if (init == null) throw new ArgumentNullException(nameof(init));
            _Process = init.Transform;
            _Disposable = ActorDisposable.StopableToAsyncDisposable(init as IActorLifeCycle);
        }

        internal Pipeline(Func<TIn, Task<TOut>> init, IAsyncDisposable asyncDisposable)
        {
            if (init == null) throw new ArgumentNullException(nameof(init));
            _Process = init;
            _Disposable = asyncDisposable;
        }

        public IPipeline<TIn, TNext> Next<TNext>(ITransformer<TOut, TNext> next)
        {
            return new Pipeline<TIn, TNext>(async (tin) =>
            {
                var trans = await _Process(tin);
                return await next.Transform(trans);
            },
            ActorDisposable.StopableToAsyncDisposable(next as IActorLifeCycle));
        }

        public IClosedPipeline<TIn> Next(IConsumer<TOut> next)
        {
            return ClosedPipeline<TIn>.CreateClosedPipeline(_Process, next);
        }

        public IClosedPipeline<TIn> Next(params IConsumer<TOut>[] next)
        {
            return ClosedPipeline<TIn>.CreateClosedPipeline(_Process, next);
        }

        public void Dispose()
        {
            _Disposable.Dispose();
        }

        public Task DisposeAsync()
        {
            return _Disposable.DisposeAsync();
        }
    }
}
