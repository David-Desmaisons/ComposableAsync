using EasyActor.Examples;
using System;
using System.Linq;
using System.Threading.Tasks;
using Concurrent.Collections;
using Concurrent.WorkItems;
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

            Console.WriteLine("Testing queues");
            await OnEveryQueueTestPerform(pingPong);

            await OnEveryFactory("Task Bool Cancellation", pingPong.TestTask_T_CancellationToken);

            await OnEveryFactory("Task Bool", pingPong.TestTask_T);

            await OnEveryFactory("Task", pingPong.TestTask);

            await OnEveryFactory("No Task", pingPong.TestNoTask);

            Console.ReadLine();
        }

        private static async Task OnEveryQueueTestPerform(PingPong pingPong)
        {
            foreach (var queue in PingPong.GetQueues().Select(o => o[0] as IMpScQueue<IWorkItem>))
            {
                await pingPong.Test_Queue_Performance(queue);
                Console.WriteLine("============================================");
            }
        }   

        private static async Task OnEveryFactory(string title, Func<string, IActorFactory, Task> doOnActor)
        {
            foreach (var fact in PingPong.GetFactories().Select(o => new
            {
                Name = o[0] as string, Factory = o[1] as IActorFactory
            }))
            {
                Console.WriteLine(title);
                await doOnActor(fact.Name, fact.Factory);
                Console.WriteLine("=================");
            }
        }
    }
}
