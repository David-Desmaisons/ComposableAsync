using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using EasyActor.Fiber.WorkItems;

namespace EasyActor.WPF.Fiber
{
    public sealed class DispatcherFiber: IFiber
    {
        private readonly Dispatcher _Dispatcher;
        private readonly Lazy<TaskScheduler> _TaskScheduler;

        public DispatcherFiber(DispatcherObject dispatcherObject)
        {
            _Dispatcher = dispatcherObject.Dispatcher;
            _TaskScheduler = new Lazy<TaskScheduler>(GetTaskScheduler);
        }

        private TaskScheduler GetTaskScheduler() => _Dispatcher.GetScheduler();

        public void Dispatch(Action action)
        {
            _Dispatcher.BeginInvoke(action);
        }

        public Task Enqueue(Action action)
        {
            var wi = new ActionWorkItem(action);
            _Dispatcher.BeginInvoke(new Action(() => wi.Do()));
            return wi.Task;
        }

        public Task<T> Enqueue<T>(Func<T> action)
        {
            var wi = new WorkItem<T>(action);
            _Dispatcher.BeginInvoke(new Action(() => wi.Do()));
            return wi.Task;
        }

        public Task Enqueue(Func<Task> action)
        {
            var wi = new AsyncActionWorkItem(action);
            _Dispatcher.BeginInvoke(new Action(() => wi.Do()));
            return wi.Task;
        }

        public Task<T> Enqueue<T>(Func<Task<T>> action)
        {
            var wi = new AsyncWorkItem<T>(action);
            _Dispatcher.BeginInvoke(new Action(() => wi.Do()));
            return wi.Task;
        }

        public TaskScheduler TaskScheduler => _TaskScheduler.Value;
    }
}
