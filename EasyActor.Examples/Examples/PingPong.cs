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

        private readonly ThreadPriority _Priority;
        internal PingPong(ThreadPriority priority)
        {
            _Priority = priority;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Test(bool taskPool)
        {
            IActorFactory fact = new ActorFactory(t => t.Priority = _Priority);

            var one = new PingPonger("Bjorg");

            IPingPonger Actor1 = fact.Build<IPingPonger>(one);

            var Two = new PingPonger("Lendl");
            var fact2 = taskPool ? new TaskPoolActorFactory() : fact;
            IPingPonger Actor2 = fact2.Build<IPingPonger>(Two);

            one.Ponger = Actor2;
            Two.Ponger = Actor1;

            var watch = new Stopwatch();
            watch.Start();

            await Actor1.Ping();
            Thread.Sleep(10000);

            var lifeCyle = Actor2 as IActorCompleteLifeCycle;
            Task Task2 = (lifeCyle == null) ? TaskBuilder.Completed : lifeCyle.Abort();
            await Task.WhenAll(((IActorCompleteLifeCycle)(Actor1)).Abort(), Task2);

            watch.Stop();

            Console.WriteLine($"Total Ping:{one.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine(one.Count);
            Console.WriteLine(Two.Count);
            Console.WriteLine($"Ping/ms:{one.Count/watch.ElapsedMilliseconds}");
        }
    }
}
