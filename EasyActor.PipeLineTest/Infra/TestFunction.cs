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
            Threads = new HashSet<Thread>();
        }

        internal HashSet<Thread> Threads { get; private set; }

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
                        Threads.Add(CallingThread);
                        return LastOut = _Trans(t);
                    };
            }

        }
    }
}
