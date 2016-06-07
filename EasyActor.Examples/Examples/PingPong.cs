using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

 
using EasyActor.TaskHelper;
using Xunit;

namespace EasyActor.Examples
{
     
    public class PingPong
    {
        public PingPong() : this(ThreadPriority.Normal)
        {
        }

        private ThreadPriority _Priority;
        internal PingPong(ThreadPriority priority)
        {
            _Priority = priority;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Test(bool TaskPool)
        {
            IActorFactory fact = new ActorFactory(t=>t.Priority=_Priority);

            var One = new PingPonger("Bjorg");
           
            IPingPonger Actor1 = fact.Build<IPingPonger>(One);

            var Two = new PingPonger("Lendl"); 
            var fact2 = TaskPool ?  new TaskPoolActorFactory() : fact;
            IPingPonger Actor2 = fact2.Build<IPingPonger>(Two);

            One.Ponger = Actor2;
            Two.Ponger = Actor1;

            var watch = new Stopwatch();
            watch.Start();

            await Actor1.Ping();
            Thread.Sleep(10000);

            var lifeCyle = Actor2 as IActorCompleteLifeCycle;
            Task Task2 = (lifeCyle == null) ? TaskBuilder.Completed : lifeCyle.Abort();
            await Task.WhenAll(((IActorCompleteLifeCycle)(Actor1)).Abort(), Task2);

            watch.Stop();

            Console.WriteLine("Total Ping:{0} Total Time: {1}", One.Count, watch.Elapsed);
            Console.WriteLine(One.Count);
            Console.WriteLine(Two.Count);
        }
    }
}
