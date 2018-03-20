﻿using System.Threading.Tasks;

//For reference see discussion:
//https://github.com/dotnet/roslyn/issues/114

namespace System
{
    /// <summary>
    ///  Asynchroneous version of IDisposable
    /// </summary>
    public interface IAsyncDisposable
    {
        /// <summary>
        ///  Performs asyncroneously application-defined tasks associated with freeing,
        ///  releasing, or resetting unmanaged resources.
        /// </summary>
        Task DisposeAsync();
    }
}
