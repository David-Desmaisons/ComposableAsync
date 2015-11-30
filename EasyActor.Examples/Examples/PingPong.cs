using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using NUnit.Framework;
using FluentAssertions;

using EasyActor.TaskHelper;


namespace EasyActor.Examples
{
    [TestFixture]
    public class PingPong
    {
        public PingPong()
            : this(ThreadPriority.Normal)
        {
        }

        private ThreadPriority _Priority;
        public PingPong(ThreadPriority priority)
        {
            _Priority = priority;
        }

        [TestCase(false, TestName="ActorFactory")]
        [TestCase(true, TestName = "ActorFactory and TaskPoolActorFactory")]
        public async Task Test(bool TaskPool = false)
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

            var lifeCyle = Actor2 as IActorLifeCycle;
            Task Task2 = (lifeCyle == null) ? TaskBuilder.Completed : lifeCyle.Abort();
            await Task.WhenAll(((IActorLifeCycle)(Actor1)).Abort(), Task2);

            watch.Stop();

            Console.WriteLine("Total Ping:{0} Total Time: {1}", One.Count, watch.Elapsed);
            Console.WriteLine(One.Count);
            Console.WriteLine(Two.Count);
        }
    }
}
