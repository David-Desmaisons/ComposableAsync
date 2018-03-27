using EasyActor.Examples;
using System;
using System.Linq;
using System.Threading.Tasks;
using EasyActor;

namespace PingPongConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var pingPong = new PingPong();
            await OnEveryFactory("Task Bool", pingPong.TestTask_T);

            await OnEveryFactory("Task", pingPong.TestTask);

            await OnEveryFactory("No Task", pingPong.TestNoTask);

            Console.ReadLine();
        }

        private static async Task OnEveryFactory(string title, Func<IActorFactory, Task> doOnActor)
        {
            foreach (var fact in PingPong.GetFactories().Select(o => o[0] as IActorFactory))
            {
                Console.WriteLine(title);
                await doOnActor(fact);
                Console.WriteLine("=================");
            }
        }
    }
}
