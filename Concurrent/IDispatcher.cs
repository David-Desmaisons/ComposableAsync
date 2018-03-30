﻿using System;
using System.Threading.Tasks;

namespace Concurrent
{
    public interface IDispatcher
    {
        /// <summary>
        /// Execute action on dispatcher context in a
        /// none-blocking way
        /// </summary>
        /// <param name="action"></param>
        void Dispatch(Action action);

        /// <summary>
        /// Enqueue the action and return a task corresponding to
        /// the completion of the action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task Enqueue(Action action);

        /// <summary>
        /// Enqueue the function and return a task corresponding to
        /// the result of the function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<T> Enqueue<T>(Func<T> action);

        /// <summary>
        /// Enqueue the task and return a task corresponding to
        /// the comletion of the task
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task Enqueue(Func<Task> action);

        /// <summary>
        /// Enqueue the task and return a task corresponding
        /// to the execution of the original task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<T> Enqueue<T>(Func<Task<T>> action);
    }
}
