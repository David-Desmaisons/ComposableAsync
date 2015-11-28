using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Examples
{
    public interface IDoStuff
    {
        Task DoStuff();

        Task<int> GetCount();
    }
}
