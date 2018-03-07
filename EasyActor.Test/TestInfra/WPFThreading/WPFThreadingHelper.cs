using System;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EasyActor.Test.TestInfra.WPFThreading
{
    public static class DispatcherHelper
    {
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private static object ExitFrame(object frame)
        {
            ((DispatcherFrame)frame).Continue = false;
            return null;
        }
    }

    public class WpfThreadingHelper :IDisposable
    {
        private readonly CancellationTokenSource _Cts;
        private Window _Window;

        public WpfThreadingHelper()
        {
            _Cts = new CancellationTokenSource();
        }

        public Thread UiThread { get; private set; }

        public Dispatcher Dispatcher => _Window.Dispatcher;

        public Task Start()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            UiThread = new Thread(() => InitUIinSta(tcs)) { Name = "Simulated UI Thread" };
            UiThread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            UiThread.Start();

            return tcs.Task;
        }

        public void Stop()
        {
            _Cts.Cancel();
        }

        private void InitUIinSta(TaskCompletionSource<object> tcs)
        {
            _Window = new Window();
            while (_Cts.IsCancellationRequested == false)
            {
                DispatcherHelper.DoEvents();
                tcs.TrySetResult(null);
            }
        }

        public void Dispose()
        {
            _Cts.Cancel();
            _Cts?.Dispose();
        }
    }
}
