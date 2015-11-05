using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public class Transformer<Tin,Tout> : ITransformer<Tin,Tout>
    {
        private Func<Tin,Tout> _Trans;

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
