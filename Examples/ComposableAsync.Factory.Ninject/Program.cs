using System;
using System.Threading;
using System.Threading.Tasks;
using Ninject;

namespace ComposableAsync.Factory.Ninject
{
    public class Program
    {
        static void Main(string[] args)
        {
            var kernel =  Configuration.GetKernel();
            var actor = kernel.Get<IActor>();

            MainAsync(actor).Wait();

            MainAsync(kernel.Get<IActor>()).Wait();

            Console.ReadLine();
        }

        private static async Task MainAsync(IActor actor)
        {
            Console.WriteLine($"Main program running on thread {Thread.CurrentThread.ManagedThreadId}");
            await actor.DoSomething();
            Console.WriteLine($"Main program continuing on thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
