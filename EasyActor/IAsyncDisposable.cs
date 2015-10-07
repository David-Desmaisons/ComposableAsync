using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//For reference see:
//https://github.com/dotnet/roslyn/issues/114

namespace EasyActor
{
    public interface IAsyncDisposable : IDisposable
    {
        Task DisposeAsync();
    }
}
