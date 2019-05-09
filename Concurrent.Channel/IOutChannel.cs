using System;
using System.Collections.Generic;

namespace Concurrent.Channel
{
    /// <summary>
    /// OutChannel abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOutChannel<out T>
    {
        /// <summary>
        /// Return messages IAsyncEnumerator
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerator<T> GetMessages();

        /// <summary>
        /// Transform into another IOutChannel
        /// </summary>
        /// <typeparam name="TNew"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        IOutChannel<TNew> Map<TNew>(Func<IObservable<T>, IObservable<TNew>> transform);

        /// <summary>
        /// Transform into another IOutChannel
        /// </summary>
        /// <typeparam name="TNew"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        IOutChannel<TNew> Map<TNew>(Func<T, TNew> transform);
    }
}
