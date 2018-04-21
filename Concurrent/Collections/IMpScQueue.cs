using System;
using System.Collections.Generic;

namespace Concurrent.Collections 
{
    public interface IMpScQueue<T> : IDisposable
    {
        void Enqueue(T item);

        void OnElements(Action<T> onItem);

        void CompleteAdding();

        IEnumerable<T> GetUnsafeQueue();
    }
}
