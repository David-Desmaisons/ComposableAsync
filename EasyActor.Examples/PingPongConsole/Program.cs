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
            foreach (var fact in PingPong.GetFactories().Select(o => o[0] as IActorFactory))
            {
                Console.WriteLine("Task");
                await new PingPong().Test(fact);
                Console.WriteLine("=================");

                Console.WriteLine("No task");
                await new PingPong().TestNoTask(fact);
                Console.WriteLine("=================");
            }
            Console.ReadLine();
        }
    }
}
