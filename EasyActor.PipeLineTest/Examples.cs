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

            await pip.DisposeAsync();
            await final.DisposeAsync();
            await final.DisposeAsync();
        }

        [Fact]
        public async Task Composition2()
        {
            var pip = PipeLine.Create<int, int>(a => a * 2);
            var final = pip.Next(Console.WriteLine);
            await final.Consume(25);

            await pip.DisposeAsync();
            await final.DisposeAsync();
        }

        [Fact]
        public async Task Composition3()
        {
            var pip = PipeLine.Create<int, int>(a => a * 2);
            var final = pip.Next(Console.WriteLine);

            await final.Consume(25);

            await pip.DisposeAsync();
            await final.DisposeAsync();
        }

        [Fact]
        public async Task Composition4()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser = PipeLine.Create<int>(i => _Output.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            var first = PipeLine.Create<int, int>(a => a * 2);
            var pipe = first.Next(finaliser);
            await pipe.Consume(25);

            await finaliser.Consume(40);

            await finaliser.DisposeAsync();
            await pipe.DisposeAsync();
            await first.DisposeAsync();
        }

        [Fact]
        public async Task Composition5()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var finaliser = PipeLine.Create<int>(i => _Output.WriteLine("{0} {1}", Thread.CurrentThread.ManagedThreadId, i));
            var first = PipeLine.Create<int, int>(a => a * 2);
            var second = first.Next(a => a - 2);
            var pipe = second.Next(finaliser);
            await pipe.Consume(25);

            await finaliser.Consume(40);

            await finaliser.DisposeAsync();
            await first.DisposeAsync();
            await second.DisposeAsync();
            await pipe.DisposeAsync();
        }

        //                     ___ i => i * 3 -----> Console.WriteLine("1 - {0} {1}")
        //                    /
        //      a => a * 2 ---
        //                    \___ i => i * 5 -----> Console.WriteLine("1 - {0} {1}")
        [Fact]
        public async Task Composition6()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var f1 = PipeLine.Create<int, int>(i => i * 3);
            var finaliser1 = f1.Next(i => _Output.WriteLine("1 - {0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            var f2 = PipeLine.Create<int, int>(i => i * 5);
            var finaliser2 = f2.Next(i => _Output.WriteLine("2 - {0} {1}", Thread.CurrentThread.ManagedThreadId, i));

            var first = PipeLine.Create<int, int>(a => a * 2);
            var pipe = first.Next(finaliser1, finaliser2);
            await pipe.Consume(1);


            await finaliser1.DisposeAsync();
            await finaliser2.DisposeAsync();
            await f1.DisposeAsync();
            await f2.DisposeAsync();
            await pipe.DisposeAsync();
            await first.DisposeAsync();
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

            var f1 = PipeLine.Create<int, int>(M3);
            var pip1 = f1.Next(finaliser1);
            var f2 = PipeLine.Create<int, int>(M5);
            var pip2 = f2.Next(finaliser1);

            var f = PipeLine.Create<int, int>(a => a * 2);
            var pip = f.Next(pip1, pip2);

            await pip.Consume(1);
            await pip.Consume(10);

            await f1.DisposeAsync();
            await f2.DisposeAsync();
            await f.DisposeAsync();
            await finaliser1.DisposeAsync();
            await pip1.DisposeAsync();
            await pip2.DisposeAsync();
            await pip.DisposeAsync();
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

            var f1 = PipeLine.Create(M3, 5);
            var pip1 = f1.Next(finaliser1);
            var f2 = PipeLine.Create(M5);
            var pip2 = f2.Next(finaliser1);

            var init = PipeLine.Create<int, int>(a => a * 2);
            var pipe = init.Next(pip1, pip2);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));

            await init.DisposeAsync();
            await f1.DisposeAsync();
            await f2.DisposeAsync();
            await pip2.DisposeAsync();
            await pip1.DisposeAsync();
            await pipe.DisposeAsync();
            await finaliser1.DisposeAsync();
        }
    }
}
