using EasyActor.TaskHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Examples
{
    public class StufferAwait: IDoStuff
    {
        private int _Count = 0;

        //Thread unsafe code
        public async Task DoStuff()
        {
            await Task.Delay(5);
            var c = _Count;
            Thread.Sleep(5);
            _Count = c + 1;
        }

        public Task<int> GetCount()
        {
            return Task.FromResult<int>(_Count);
        }
    }
}
