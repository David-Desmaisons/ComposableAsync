using System;
using System.Linq;
using System.Threading.Tasks;
using EasyActor.Pipeline;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace EasyActor.PipeLineTest
{  
    public class Examples
    {
        private readonly ITestOutputHelper _Output;
        public Examples(ITestOutputHelper output) 
        {
            _Output = output;
        }

        [Fact]
        public async Task Composition()
        {
            var trans = Transformer.Create<int, int>(a => a * 2);
            var print = Consumer.Create<int>(Console.WriteLine);
            var pip = PipeLine.Create(trans);
            var final = pip.Next(print);

            await final.Consume(25);
        }

        [Fact]
        public async Task Composition2()
        {
            var pip = PipeLine.Create<int, int>(a => a * 2);
            var final = pip.Next(Console.WriteLine);
            await final.Consume(25);
        }

        [Fact]
        public async Task Composition3()
        {
            var pip = PipeLine.Create<int, int>(a => a * 2).Next(Console.WriteLine);
            await pip.Consume(25);
        }


        [Fact]
        public async Task Composition4()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser = PipeLine.Create<int>(i => _Output.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            await PipeLine.Create<int, int>(a => a * 2).Next(finaliser).Consume(25);

            await finaliser.Consume(40);
        }

        [Fact]
        public async Task Composition5()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser = PipeLine.Create<int>(i => _Output.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            await PipeLine.Create<int, int>(a => a * 2).Next(a => a - 2).Next(finaliser).Consume(25);

            await finaliser.Consume(40);
        }

        //                     ___ i => i * 3 -----> Console.WriteLine("1 - {0} {1}")
        //                    /
        //      a => a * 2 ---
        //                    \___ i => i * 5 -----> Console.WriteLine("1 - {0} {1}")
        [Fact]
        public async Task Composition6()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser1 = PipeLine.Create<int, int>(i => i * 3).Next(i => _Output.WriteLine("1 - {0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            var finaliser2 = PipeLine.Create<int, int>(i => i * 5).Next(i => _Output.WriteLine("2 - {0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            await PipeLine.Create<int, int>(a => a * 2).Next(finaliser1, finaliser2).Consume(1);
        }


        //                     ___ i => i * 3____ 
        //                    /                  \
        //      a => a * 2 ---                    ------>Console.WriteLine("{0} {1}")
        //                    \___ i => i * 5 ___/
        [Fact]
        public async Task Composition7()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser1 = PipeLine.Create<int>(i => _Output.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            Func<int, int> M3 = i => { _Output.WriteLine("M3 {0}", Thread.CurrentThread.ManagedThreadId); return i * 3; };
            Func<int, int> M5 = i => { _Output.WriteLine("M5 {0}", Thread.CurrentThread.ManagedThreadId); return i * 5; };

            var pip1 = PipeLine.Create<int, int>(M3).Next(finaliser1);
            var pip2 = PipeLine.Create<int, int>(M5).Next(finaliser1);

            var pip = PipeLine.Create<int, int>(a => a * 2).Next(pip1, pip2);

            await pip.Consume(1);
            await pip.Consume(10);
        }

        //                     ___ i => i * 3_(5)__ 
        //                    /                    \
        //      a => a * 2 ---                      ------>Console.WriteLine("{0} {1}")
        //                    \___ i => i * 5 _____/
        [Fact]
        public async Task Composition8()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser1 = PipeLine.Create<int>(i => _Output.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            Func<int, int> M3 = i => { Thread.Sleep(500); _Output.WriteLine("M3:{0} Thread:{1} (5 threads)", i, Thread.CurrentThread.ManagedThreadId); return i * 3; };
            Func<int, int> M5 = i => { _Output.WriteLine("M5:{0} Thread:{1} (monothreaded)", i, Thread.CurrentThread.ManagedThreadId); return i * 5; };

            var pip1 = PipeLine.Create(M3, 5).Next(finaliser1);
            var pip2 = PipeLine.Create(M5).Next(finaliser1);

            var pipe = PipeLine.Create<int, int>(a => a * 2).Next(pip1, pip2);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));
        }
    }
}
