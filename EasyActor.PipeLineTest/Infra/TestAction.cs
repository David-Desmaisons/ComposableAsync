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
        private List<T> _Result = new List<T>();
        internal TestAction(Action<T> trans=null)
        {
            _Trans = trans;
            Threads = new HashSet<Thread>();
        }

        internal HashSet<Thread> Threads { get; private set; }

        internal Thread CallingThread { get; private set; }

        internal T LastIn { get; private set; }

        internal IEnumerable<T> Results { get { return _Result; } }

        internal Action<T> Action
        {
            get
            {
                return t =>
                    {
                        LastIn = t;
                        _Result.Add(t);
                        CallingThread = Thread.CurrentThread;
                        Threads.Add(CallingThread);
                        if (_Trans!=null) _Trans(t);
                    };
            }
        }
    }
}
