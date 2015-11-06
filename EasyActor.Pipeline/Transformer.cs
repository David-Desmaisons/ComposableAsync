using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public static class Transformer
    {

        public static ITransformer<Tin, Tout> Create<Tin, Tout>(Func<Tin, Tout> transform)
        {
            return ActorBuilder.ActorFactory.Build<ITransformer<Tin, Tout>>(new Transformer<Tin, Tout>(transform));
        }

        public static ITransformer<Tin, Tout> Create<Tin, Tout>(Func<Tin, Tout> transform, int MaxParallelism)
        {
            return ActorBuilder.LoadBalancer.Build<ITransformer<Tin, Tout>>(()=>new Transformer<Tin, Tout>(transform), MaxParallelism);
        }

    }
}
