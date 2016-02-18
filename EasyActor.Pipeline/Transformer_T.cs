using System;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public class Transformer<Tin,Tout> : ITransformer<Tin,Tout>
    {
        private readonly Func<Tin,Tout> _Trans;

        public Transformer(Func<Tin,Tout> trans)
        {
            _Trans = trans;
        }

        public Task<Tout> Transform(Tin entry)
        {
            return Task.FromResult(_Trans(entry));
        }
    }
}
