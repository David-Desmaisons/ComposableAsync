using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;

namespace EasyActor.Examples
{
    public interface IPinger
    {
        Task Ping();
    }

    public interface IPonger
    {
        Task Pong();
    }

    internal class Pinger : IPinger
    {
        public int s_Count = 0;
        private readonly IPonger m_Ponger;
        private Thread _Thread = null;
        public bool Check { get; set; }

        public Pinger(IPonger ponger)
        {
            m_Ponger = ponger;
        }



        public Task Ping()
        {
            if (Check)
            {
                if (_Thread != null)
                {
                    _Thread.Should().Be(Thread.CurrentThread);
                }
                else
                    _Thread = Thread.CurrentThread;
            }
            s_Count++;
            m_Ponger.Pong();
            return Task.FromResult<object>(null);
        }
    }

    internal class Ponger : IPonger
    {
        public int s_Count = 0;
        private Thread _Thread = null;
        public bool Check { get; set; }

        internal IPinger pinger { get; set; }
        public Task Pong()
        {
            if (Check)
            {
                if (_Thread != null)
                {
                    _Thread.Should().Be(Thread.CurrentThread);
                }
                else
                    _Thread = Thread.CurrentThread;
            }
            s_Count++;

            if (pinger != null)
                pinger.Ping();

            return Task.FromResult<object>(null);
        }
    }

    [TestFixture]
    public class pingpong
    {
        [Test]
        public async Task Test()
        {
            var fact = new ActorFactory(priority: Priority.Highest);

            var basicponger = new Ponger();
            IPonger ponger = fact.Build<IPonger>(basicponger);
            var basicpinger = new Pinger(ponger);


            IPinger pinger = fact.Build<IPinger>(basicpinger);
            basicponger.pinger = pinger;

            await pinger.Ping();
            Thread.Sleep(10000);

            Console.WriteLine(basicpinger.s_Count);
            Console.WriteLine(basicponger.s_Count);
        }
    }
}
