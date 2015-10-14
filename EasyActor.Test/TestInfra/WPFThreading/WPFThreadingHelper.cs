using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EasyActor.Test.WPFThreading
{
    public static class DispatcherHelper
    {
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
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

    public class WPFThreadingHelper
    {
        private CancellationTokenSource _CTS;
        private Window _Window;

        public WPFThreadingHelper()
        {
            _CTS = new CancellationTokenSource();
        }

        public Thread UIThread { get; private set; }

        public Dispatcher Dispatcher { get { return _Window.Dispatcher; } }

        public Task Start()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            UIThread = new Thread(()=>InitUIinSTA(tcs));
            UIThread.Name = "Simulated UI Thread";
            UIThread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            UIThread.Start();

            return tcs.Task;
        }

        public void Stop()
        {
            _CTS.Cancel();
        }

        private void InitUIinSTA(TaskCompletionSource<object> tcs)
        {
            _Window = new Window();
            while (_CTS.IsCancellationRequested == false)
            {
                DispatcherHelper.DoEvents();
                tcs.TrySetResult(null);
            }
        }


    }
}
