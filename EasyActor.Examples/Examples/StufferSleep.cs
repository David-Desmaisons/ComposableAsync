using EasyActor.TaskHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Examples
{
    public class StufferSleep : IDoStuff
    {
        private int _Count = 0;

        //Thread unsafe code
        public Task DoStuff()
        {
            var c = _Count;
            Thread.Sleep(5);
            _Count = c + 1;
            return TaskBuilder.Completed;
        }

        public Task<int> GetCount()
        {
            return Task.FromResult<int>(_Count);
        }
    }
}
