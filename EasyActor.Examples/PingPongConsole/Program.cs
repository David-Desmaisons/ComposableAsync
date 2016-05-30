using EasyActor.Examples;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            await new PingPong(ThreadPriority.Normal).Test(); 
         
            Console.ReadLine();
        }
    }
}
