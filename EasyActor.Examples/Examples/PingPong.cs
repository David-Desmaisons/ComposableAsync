using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using EasyActor.Factories;
using EasyActor.TaskHelper;
using Xunit;
using Xunit.Abstractions;

namespace EasyActor.Examples
{

    public class PingPong
    {
        private readonly ITestOutputHelper _Output;
        public PingPong(ITestOutputHelper output = null) : this(ThreadPriority.Normal)
        {
            _Output = output;
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

            var actor1 = fact.Build<IPingPonger>(one);

            var two = new PingPonger("Lendl");
            var fact2 = taskPool ? new TaskPoolActorFactory() : fact;
            var actor2 = fact2.Build<IPingPonger>(two);

            one.Ponger = actor2;
            two.Ponger = actor1;

            var watch = new Stopwatch();
            watch.Start();

            await actor1.Ping();
            Thread.Sleep(10000);

            var lifeCyle = actor2 as IActorCompleteLifeCycle;
            Task Task2 = (lifeCyle == null) ? TaskBuilder.Completed : lifeCyle.Abort();
            await Task.WhenAll(((IActorCompleteLifeCycle)(actor1)).Abort(), Task2);

            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestRelang(bool taskPool)
        {
            IActorFactory fact = new ActorRetlangFactor(t => t.Priority = _Priority);

            var one = new PingPonger("Bjorg");

            var actor1 = fact.Build<IPingPonger>(one);

            var two = new PingPonger("Lendl");
            var fact2 = taskPool ? new TaskPoolActorFactory() : fact;
            var actor2 = fact2.Build<IPingPonger>(two);

            one.Ponger = actor2;
            two.Ponger = actor1;

            var watch = new Stopwatch();
            watch.Start();

            await actor1.Ping();
            Thread.Sleep(10000);

            var lifeCyle = actor2 as IActorCompleteLifeCycle;
            Task Task2 = (lifeCyle == null) ? TaskBuilder.Completed : lifeCyle.Abort();
            await Task.WhenAll(((IActorCompleteLifeCycle)(actor1)).Abort(), Task2);

            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");
        }

        private void Output(string message)
        {
            Console.WriteLine(message);
            _Output?.WriteLine(message);
        }
    }
}
