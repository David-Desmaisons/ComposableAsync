using System;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Concurrent.Test.Helper
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

    public class WpfThreadingHelper : IDisposable
    {
        public Thread UiThread { get; private set; }
        public Dispatcher Dispatcher => _Window.Dispatcher;

        private readonly CancellationTokenSource _Cts;
        private Window _Window;

        public WpfThreadingHelper()
        {
            _Cts = new CancellationTokenSource();
        }
    
        public Task Start()
        {
            var tcs = new TaskCompletionSource<object>();
            UiThread = new Thread(InitUIinSta) { Name = "Simulated UI Thread" };
            UiThread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            UiThread.Start(tcs);
            return tcs.Task;
        }

        public void Stop()
        {
            _Cts.Cancel();
        }

        private void InitUIinSta(object context)
        {
            var tcs = context as TaskCompletionSource<object>;
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
