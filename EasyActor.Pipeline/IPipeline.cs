using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public interface IPipeline<Tin,Tout>
    {
        IPipeline<Tin, Tnext> Next<Tnext>(ITransformer<Tout, Tnext> next);

        IClosedPipeline<Tin> Next(IConsumer<Tout> next);

        IClosedPipeline<Tin> Next(params IConsumer<Tout>[] next);
    }
}
