using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Factory.Ninject 
{
    public class Actor: IActor
    {
        public async Task DoSomething()
        {
            Console.WriteLine($"Running on thread {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(50);
            Console.WriteLine($"Continuing on thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
