using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.PipeLineTest.Infra
{
    internal class TestFunction<Tin,Tout>
    {
        private Func<Tin, Tout> _Trans;
        internal TestFunction(Func<Tin, Tout> trans)
        {
            _Trans = trans;
        }

        internal Thread CallingThread { get; private set; }

        internal Tin LastIn { get; private set; }

        internal Tout LastOut { get; private set; }

        internal Func<Tin, Tout> Function
        {
            get
            {
                return t =>
                    {
                        LastIn = t;
                        CallingThread = Thread.CurrentThread;
                        return LastOut = _Trans(t);
                    };
            }

        }
    }
}
