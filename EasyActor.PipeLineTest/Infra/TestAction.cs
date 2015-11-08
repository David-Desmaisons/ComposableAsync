using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.PipeLineTest.Infra
{
    internal class TestAction<T>
    {
        private Action<T> _Trans;
        internal TestAction(Action<T> trans=null)
        {
            _Trans = trans;
            Threads = new HashSet<Thread>();
        }

        internal HashSet<Thread> Threads { get; private set; }

        internal Thread CallingThread { get; private set; }

        internal T LastIn { get; private set; }

        internal Action<T> Action
        {
            get
            {
                return t =>
                    {
                        LastIn = t;
                        CallingThread = Thread.CurrentThread;
                        Threads.Add(CallingThread);
                        if (_Trans!=null) _Trans(t);
                    };
            }

        }
    }
}
