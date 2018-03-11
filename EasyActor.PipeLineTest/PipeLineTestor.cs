using System.Linq;
using System.Threading.Tasks;
 
using EasyActor.Pipeline;
using System.Threading;
using EasyActor.PipeLineTest.Infra;
using FluentAssertions;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace EasyActor.PipeLineTest
{
     
    public class PipeLineTestor
    {
        private readonly TestFunction<int, int>  _Func;
        private readonly TestFunction<int, int> _Func2;
        private readonly TestFunction<int, int> _Func3;
        private readonly TestFunction<int, int> _Func4;
        private readonly TestAction<int> _Act;
        private readonly TestAction<int> _Act2;
        private readonly TestAction<int> _Act3;

        public PipeLineTestor()
        {
            _Func = new TestFunction<int, int>(a => a * 5);
            _Func2 = new TestFunction<int, int>(a => a - 2);
            _Func3 = new TestFunction<int, int>(a => a + 2);
            _Func4 = new TestFunction<int, int>(a => { Thread.Sleep(300); return a * 3; });
            _Act = new TestAction<int>();
            _Act2 = new TestAction<int>();
            _Act3 = new TestAction<int>(a => { Thread.Sleep(300); });
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2,10)]
        [InlineData(1, 5)]
        public async Task Create_Should_Compute_Result_OK(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var first = PipeLine.Create(_Func.Function);
            var pip = first.Next(_Act.Action);
            await pip.Consume(iin);

            _Func.LastIn.Should().Be(iin);
            _Func.LastOut.Should().Be(iout);
            _Func.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);

            await pip.DisposeAsync();
            await first.DisposeAsync();
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 10)]
        [InlineData(1, 5)]
        public async Task Create_Should_Use_one_Thread(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var first = PipeLine.Create(_Func.Function);
            var pip = first.Next(_Act.Action);

            await pip.Consume(iin);
            await pip.Consume(iin);
            await pip.Consume(iin);

            _Func.LastIn.Should().Be(iin);
            _Func.LastOut.Should().Be(iout);
            _Func.Threads.Count.Should().Be(1);
            _Func.CallingThread.Should().NotBe(current);

            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);
            _Act.Threads.Count.Should().Be(1);

            await pip.DisposeAsync();
            await first.DisposeAsync();
        }

        [Theory]
        [InlineData(0, -2)]
        [InlineData(2, 8)]
        [InlineData(1, 3)]
        public async Task Compose_Should_Compute_Result_OK(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var first = PipeLine.Create(_Func.Function);
            var second = first.Next(_Func2.Function);
            var pip = second.Next(_Act.Action);
            await pip.Consume(iin);

            _Func.LastIn.Should().Be(iin);

            _Func2.LastOut.Should().Be(iout);
            _Func2.CallingThread.Should().NotBe(current);
            _Func2.CallingThread.Should().NotBe(_Func.CallingThread);

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);

            await pip.DisposeAsync();
            await second.DisposeAsync();
            await first.DisposeAsync();
        }

        //                     ___ i => i * 3 -----> Console.WriteLine("1 - {0} {1}")
        //                    /
        //      a => a * 2 ---
        //                    \___ i => i * 5 -----> Console.WriteLine("1 - {0} {1}")
        [Theory]
        [InlineData(0, 0, 2)]
        [InlineData(1, 5, 3)]
        [InlineData(2, 10, 4)]
        [InlineData(10, 50, 12)]
        public async Task Compose_Parralel_Should_Compute_Result_OK(int entry, int s1, int s2)
        {
            var current = Thread.CurrentThread;

            var first = PipeLine.Create(_Func.Function);
            var finaliser1 = first.Next(_Act.Action);

            var second = PipeLine.Create(_Func3.Function);
            var finaliser2 = second.Next(_Act2.Action);

            var third = PipeLine.Create<int, int>(a => a);
            var pipe = third.Next(finaliser1, finaliser2);

            await pipe.Consume(entry);

            _Func.LastIn.Should().Be(entry);
            _Func.LastOut.Should().Be(s1);

            _Func3.LastIn.Should().Be(entry);
            _Func3.LastOut.Should().Be(s2);

            _Func3.CallingThread.Should().NotBe(current);
            _Func3.CallingThread.Should().NotBe(_Func.CallingThread);

            _Func.CallingThread.Should().NotBe(current);
            _Func.CallingThread.Should().NotBe(_Func3.CallingThread);

            await finaliser1.DisposeAsync();
            await finaliser2.DisposeAsync();
            await second.DisposeAsync();
            await first.DisposeAsync();
            await third.DisposeAsync();
            await pipe.DisposeAsync();
        }

        //                     ___ i => i * 3____ 
        //                    /                  \
        //      a => a * 2 ---                    ------>Console.WriteLine("{0} {1}")
        //                    \___ i => i * 5 ___/
        [Fact]
        public async Task Compose_Parralel_Should_Compute_Result_OK_NoParameter()
        {
            var current = Thread.CurrentThread;

            var fin = PipeLine.Create(_Act.Action);

            var first = PipeLine.Create(_Func.Function);
            var pip1 = first.Next(fin);
            var second = PipeLine.Create(_Func2.Function);
            var pip2 = second.Next(fin);

            var third = PipeLine.Create<int, int>(a => a * 2);
            var pip = third.Next(pip1, pip2);

            await pip.Consume(1);

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);
            _Act.CallingThread.Should().NotBe(_Func2.CallingThread);


            await fin.DisposeAsync();
            await pip1.DisposeAsync();
            await pip2.DisposeAsync();
            await first.DisposeAsync();
            await second.DisposeAsync();
            await third.DisposeAsync();
        }

        [Fact]
        public async Task Create_With_Paralelism_Parameter_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var first = PipeLine.Create(_Func4.Function, 5);
            var pipe = first.Next(_Act.Action);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);
  
            _Func4.Threads.Count.Should().Be(5);
            _Func4.Threads.Should().NotContain(_Act.CallingThread);
            _Func4.Threads.Should().NotContain(current);

            await first.DisposeAsync();
            await pipe.DisposeAsync();
        }

        [Fact]
        public async Task Next_With_Paralelism_Parameter_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var first = PipeLine.Create<int, int>(i => i);
            var second = first.Next(_Func4.Function, 5);
            var pipe = second.Next(_Act.Action);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);

            _Func4.Threads.Count.Should().Be(5);
            _Func4.Threads.Should().NotContain(_Act.CallingThread);
            _Func4.Threads.Should().NotContain(current);

            await first.DisposeAsync();
            await second.DisposeAsync();
            await pipe.DisposeAsync();
        }

        [Fact]
        public async Task Next_Action_With_Paralelism_Parameter_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var first = PipeLine.Create(_Func3.Function);
            var pipe = first.Next(_Act3.Action, 5);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));


            _Func3.Threads.Count.Should().Be(1);
            _Func3.CallingThread.Should().NotBe(current);

            _Act3.Threads.Count.Should().Be(5);
            _Act3.Threads.Should().NotContain(_Func4.CallingThread);
            _Act3.Threads.Should().NotContain(current);

            await first.DisposeAsync();
            await pipe.DisposeAsync();
        }

        [Fact]
        public async Task Create_Action_With_Paralelism_Parameter_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var pipe = PipeLine.Create(_Act3.Action, 5);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));

            _Act3.Threads.Count.Should().Be(5);
            _Act3.Threads.Should().NotContain(_Func4.CallingThread);
            _Act3.Threads.Should().NotContain(current);

            await pipe.DisposeAsync();
        }

        [Fact]
        public async Task Connect_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var first = PipeLine.Create(_Func.Function);
            var pipe = first.Next(_Act.Action);

            var obs = Observable.Range(0, 100);
            var res = Enumerable.Range(0,100).Select(_Func.Function);

            var disp = pipe.Connect(obs);

            Thread.Sleep(10);

            _Act.Threads.Count.Should().Be(1);
            _Act.Results.Should().BeEquivalentTo(res);

            disp.Dispose();

            await pipe.DisposeAsync();
            await first.DisposeAsync();
        }  
    }
}
