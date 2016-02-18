using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public class Pipeline<Tin, Tout> : IPipeline<Tin,Tout>
    {
        private Func<Tin, Task<Tout>> _Process;

        internal Pipeline(ITransformer<Tin, Tout> Init)
        {
            if (Init == null) throw new ArgumentNullException("Init");
            _Process = Init.Transform;
        }


        internal Pipeline(Func<Tin, Task<Tout>> Init)
        {
            if (Init == null) throw new ArgumentNullException("Init");
            _Process = Init;
        }
       

        public IPipeline<Tin, Tnext> Next<Tnext>(ITransformer<Tout, Tnext> next)
        {
            return new Pipeline<Tin, Tnext>(async (tin) =>
                {
                    var trans = await _Process(tin);

                    return await next.Transform(trans);
                });
        }

        public IClosedPipeline<Tin> Next(IConsumer<Tout> next)
        {
            return new ClosedPipeline<Tin>(async (tin) =>
            {
                var trans = await _Process(tin);

                await next.Consume(trans);
            });
        }

        public IClosedPipeline<Tin> Next(params IConsumer<Tout>[] next)
        {
            return new ClosedPipeline<Tin>(async (tin) =>
            {
                var trans = await _Process(tin);

                await Task.WhenAll(next.Select( n => n.Consume(trans)).ToArray());
            });
        }
    }
}
