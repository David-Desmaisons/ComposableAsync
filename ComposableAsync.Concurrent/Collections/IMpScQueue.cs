using System;
using System.Collections.Generic;

namespace ComposableAsync.Concurrent.Collections 
{
    /// <summary>
    /// Multiple producer single consumer queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMpScQueue<T> : IDisposable
    {
        /// <summary>
        /// Enqueue an item
        /// </summary>
        /// <param name="item"></param>
        void Enqueue(T item);

        /// <summary>
        /// Perform an action on all element
        /// </summary>
        /// <param name="onItem"></param>
        void OnElements(Action<T> onItem);

        /// <summary>
        /// Stop addition of element
        /// </summary>
        void CompleteAdding();

        /// <summary>
        /// Obtain the elements in an unsafe way
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetUnsafeQueue();
    }
}
