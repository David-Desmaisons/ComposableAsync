using System.Linq;
using System.Threading.Tasks;
 
using EasyActor.Pipeline;
using System.Threading;
using EasyActor.PipeLineTest.Infra;
using FluentAssertions;
using System.Reactive.Linq;
using Xunit;

namespace EasyActor.PipeLineTest
{
     
    public class PipeLineTestor
    {
        private TestFunction<int, int>  _Func;
        private TestFunction<int, int> _Func2;
        private TestFunction<int, int> _Func3;
        private TestFunction<int, int> _Func4;
        private TestAction<int> _Act;
        private TestAction<int> _Act2;
        private TestAction<int> _Act3;

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
            var pip = PipeLine.Create(_Func.Function).Next(_Act.Action);
            await pip.Consume(iin);

            _Func.LastIn.Should().Be(iin);
            _Func.LastOut.Should().Be(iout);
            _Func.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 10)]
        [InlineData(1, 5)]
        public async Task Create_Should_Use_one_Thread(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var pip = PipeLine.Create(_Func.Function).Next(_Act.Action);

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
        }

        [Theory]
        [InlineData(0, -2)]
        [InlineData(2, 8)]
        [InlineData(1, 3)]
        public async Task Compose_Should_Compute_Result_OK(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var pip = PipeLine.Create(_Func.Function).Next(_Func2.Function).Next(_Act.Action);
            await pip.Consume(iin);

            _Func.LastIn.Should().Be(iin);

            _Func2.LastOut.Should().Be(iout);
            _Func2.CallingThread.Should().NotBe(current);
            _Func2.CallingThread.Should().NotBe(_Func.CallingThread);

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);
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
            var finaliser1 = PipeLine.Create(_Func.Function).Next(_Act.Action);
            var finaliser2 = PipeLine.Create(_Func3.Function).Next(_Act2.Action);

            await PipeLine.Create<int, int>(a => a).Next(finaliser1, finaliser2).Consume(entry);


            _Func.LastIn.Should().Be(entry);
            _Func.LastOut.Should().Be(s1);

            _Func3.LastIn.Should().Be(entry);
            _Func3.LastOut.Should().Be(s2);

            _Func3.CallingThread.Should().NotBe(current);
            _Func3.CallingThread.Should().NotBe(_Func.CallingThread);

            _Func.CallingThread.Should().NotBe(current);
            _Func.CallingThread.Should().NotBe(_Func3.CallingThread);
        }

        //                     ___ i => i * 3____ 
        //                    /                  \
        //      a => a * 2 ---                    ------>Console.WriteLine("{0} {1}")
        //                    \___ i => i * 5 ___/
        [Fact]
        public async Task Compose_Parralel_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var fin = PipeLine.Create(_Act.Action);

            var pip1 = PipeLine.Create(_Func.Function).Next(fin);
            var pip2 = PipeLine.Create(_Func2.Function).Next(fin);

            var pip = PipeLine.Create<int, int>(a => a * 2).Next(pip1, pip2);

            await pip.Consume(1);

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);
            _Act.CallingThread.Should().NotBe(_Func2.CallingThread);
        }

        [Fact]
        public async Task Create_With_Paralelism_Parameter_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var pipe = PipeLine.Create(_Func4.Function, 5).Next(_Act.Action);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);
  
            _Func4.Threads.Count.Should().Be(5);
            _Func4.Threads.Should().NotContain(_Act.CallingThread);
            _Func4.Threads.Should().NotContain(current);
        }

        [Fact]
        public async Task Next_With_Paralelism_Parameter_Should_Compute_Result_OK()
        {
            //SynchronizationContext.SetSynchronizationContext(null);
            var current = Thread.CurrentThread;

            var pipe = PipeLine.Create<int, int>(i=>i).Next(_Func4.Function, 5).Next(_Act.Action);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);

            _Func4.Threads.Count.Should().Be(5);
            _Func4.Threads.Should().NotContain(_Act.CallingThread);
            _Func4.Threads.Should().NotContain(current);
        }

        [Fact]
        public async Task Next_Action_With_Paralelism_Parameter_Should_Compute_Result_OK()
        {
            //SynchronizationContext.SetSynchronizationContext(null);
            var current = Thread.CurrentThread;

            var pipe = PipeLine.Create(_Func3.Function).Next(_Act3.Action, 5);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(i => pipe.Consume(i)));


            _Func3.Threads.Count.Should().Be(1);
            _Func3.CallingThread.Should().NotBe(current);

            _Act3.Threads.Count.Should().Be(5);
            _Act3.Threads.Should().NotContain(_Func4.CallingThread);
            _Act3.Threads.Should().NotContain(current);
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
        }

        [Fact]
        public void Connect_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var pipe = PipeLine.Create(_Func.Function).Next(_Act.Action);

            var obs = Observable.Range(0, 100);
            var res = Enumerable.Range(0,100).Select(_Func.Function);

            var disp = pipe.Connect(obs);

            Thread.Sleep(10);

            _Act.Threads.Count.Should().Be(1);
            _Act.Results.Should().BeEquivalentTo(res);

            disp.Dispose();
        }  
    }
}
