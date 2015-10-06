using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using EasyActor.TaskHelper;

namespace EasyActor.Examples
{
    public interface IPingPonger
    {
        Task Ping();
    }

    internal class PingPonger : IPingPonger
    {
        public int Count {get;set;}
        public string Name { get; private set; }

        internal IPingPonger Ponger { get; set; }

        public PingPonger(string iName)
        {
            Name = iName;
        }

        public Task Ping()
        {
            Console.WriteLine("{0} Ping from thread {1}",Name, Thread.CurrentThread.ManagedThreadId);
            Count++;
            if (Ponger != null)
                Ponger.Ping();
            return TaskBuilder.GetCompleted();
        }
    }

    [TestFixture]
    public class PingPong
    {
        [Test]
        public async Task Test()
        {
            var fact = new ActorFactory(priority: Priority.AboveNormal);

            var One = new PingPonger("Bjorg");
            IPingPonger Actor1 = fact.Build<IPingPonger>(One);

            var Two = new PingPonger("Lendl");
            IPingPonger Actor2 = fact.Build<IPingPonger>(Two);

            One.Ponger = Actor2;
            Two.Ponger = Actor1;

            await Actor1.Ping();
            Thread.Sleep(10000);

            Console.WriteLine(One.Count);
            Console.WriteLine(Two.Count);
        }
    }
}
