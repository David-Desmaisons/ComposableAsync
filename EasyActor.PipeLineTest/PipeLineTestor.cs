using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EasyActor.Pipeline;
using System.Threading;
using EasyActor.PipeLineTest.Infra;
using FluentAssertions;

namespace EasyActor.PipelineTest
{
    [TestFixture]
    public class PipeLineTestor
    {
        private TestFunction<int, int>  _Func;
        private TestAction<int> _Act;

        [SetUp]
        public void SetUp()
        {
            _Func = new TestFunction<int, int>(a => a * 5);
            _Act = new TestAction<int>();
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(2,10)]
        [TestCase(1, 5)]
        public async Task Should_Compute_Result_OK(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var pip = PipeLine.Create<int, int>(_Func.Function).Next(_Act.Action);
            await pip.Consume(iin);

            _Func.LastOut.Should().Be(iout);
            _Func.CallingThread.Should().NotBe(current);
        }



        [Test]
        public async Task Composition()
        {
            var trans = Transformer.Create<int, int>(a => a * 2);
            var print = Consumer.Create<int>(Console.WriteLine);
            var pip = PipeLine.Create(trans);
            var final = pip.Next(print);

            await final.Consume(25);
        }

        [Test]
        public async Task Composition2()
        {
            var pip = PipeLine.Create<int, int>(a => a * 2);
            var final = pip.Next(Console.WriteLine);
            await final.Consume(25);
        }

        [Test]
        public async Task Composition3()
        {
            var pip =  PipeLine.Create<int, int>(a => a * 2).Next(Console.WriteLine);
            await pip.Consume(25);
        }

       

        [Test]
        public async Task Composition4()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser = PipeLine.Create<int>(i => Console.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            await PipeLine.Create<int, int>(a => a * 2).Next(finaliser).Consume(25);

            await finaliser.Consume(40);
        }

        [Test]
        public async Task Composition5()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser = PipeLine.Create<int>(i => Console.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            await PipeLine.Create<int, int>(a => a * 2).Next(a=>a-2).Next(finaliser).Consume(25);

            await finaliser.Consume(40);
        }

        //                     ___ i => i * 3 -----> Console.WriteLine("1 - {0} {1}")
        //                    /
        //      a => a * 2 ---
        //                    \___ i => i * 5 -----> Console.WriteLine("1 - {0} {1}")
        [Test]
        public async Task Composition6()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser1 = PipeLine.Create<int, int>(i => i * 3).Next(i => Console.WriteLine("1 - {0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            var finaliser2 = PipeLine.Create<int, int>(i => i * 5).Next(i => Console.WriteLine("2 - {0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            await PipeLine.Create<int, int>(a => a * 2).Next(finaliser1, finaliser2).Consume(1);
        }


        //                     ___ i => i * 3____ 
        //                    /                  \
        //      a => a * 2 ---                    ------>Console.WriteLine("{0} {1}")
        //                    \___ i => i * 5 ___/
        [Test]
        public async Task Composition7()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser1 = PipeLine.Create<int>(i => Console.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            Func<int, int> M3 = i => { Console.WriteLine("M3 {0}",Thread.CurrentThread.ManagedThreadId); return i * 3; };
            Func<int, int> M5 = i => { Console.WriteLine("M5 {0}",Thread.CurrentThread.ManagedThreadId); return i * 5; };

            var pip1 = PipeLine.Create<int, int>(M3).Next(finaliser1);
            var pip2 = PipeLine.Create<int, int>(M5).Next(finaliser1);

            await PipeLine.Create<int, int>(a => a * 2).Next(pip1, pip2).Consume(1);
            await PipeLine.Create<int, int>(a => a * 2).Next(pip1, pip2).Consume(10);
        }

        //                     ___ i => i * 3_(5)__ 
        //                    /                    \
        //      a => a * 2 ---                      ------>Console.WriteLine("{0} {1}")
        //                    \___ i => i * 5 _____/
        [Test]
        public async Task Composition8()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser1 = PipeLine.Create<int>(i => Console.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            Func<int, int> M3 = i => { Thread.Sleep(500); Console.WriteLine("M3 {0}", Thread.CurrentThread.ManagedThreadId); return i * 3; };
            Func<int, int> M5 = i => { Console.WriteLine("M5 {0}", Thread.CurrentThread.ManagedThreadId); return i * 5; };

            var pip1 = PipeLine.Create<int, int>(M3,5).Next(finaliser1);
            var pip2 = PipeLine.Create<int, int>(M5).Next(finaliser1);

            var pipe = PipeLine.Create<int, int>(a => a * 2).Next(pip1, pip2);

            await Task.WhenAll( Enumerable.Range(0,100).Select(i => pipe.Consume(i)) );
        }
    }
}
