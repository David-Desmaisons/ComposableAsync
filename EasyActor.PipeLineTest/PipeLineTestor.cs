﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EasyActor.Pipeline;
using System.Threading;
using EasyActor.PipeLineTest.Infra;
using FluentAssertions;

namespace EasyActor.PipeLineTest
{
    [TestFixture]
    public class PipeLineTestor
    {
        private TestFunction<int, int>  _Func;
        private TestFunction<int, int> _Func2;
        private TestFunction<int, int> _Func3;
        private TestAction<int> _Act;
        private TestAction<int> _Act2;

        [SetUp]
        public void SetUp()
        {
            _Func = new TestFunction<int, int>(a => a * 5);
            _Func2 = new TestFunction<int, int>(a => a - 2);
            _Func3 = new TestFunction<int, int>(a => a + 2);
            _Act = new TestAction<int>();
            _Act2 = new TestAction<int>();
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(2,10)]
        [TestCase(1, 5)]
        public async Task Create_Should_Compute_Result_OK(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var pip = PipeLine.Create<int, int>(_Func.Function).Next(_Act.Action);
            await pip.Consume(iin);

            _Func.LastIn.Should().Be(iin);
            _Func.LastOut.Should().Be(iout);
            _Func.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(2, 10)]
        [TestCase(1, 5)]
        public async Task Create_Should_Use_one_Thread(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var pip = PipeLine.Create<int, int>(_Func.Function).Next(_Act.Action);

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


        [Test]
        [TestCase(0, -2)]
        [TestCase(2, 8)]
        [TestCase(1, 3)]
        public async Task Compose_Should_Compute_Result_OK(int iin, int iout)
        {
            var current = Thread.CurrentThread;
            var pip = PipeLine.Create<int, int>(_Func.Function).Next(_Func2.Function).Next(_Act.Action);
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
        [TestCase(0, 0, 2)]
        [TestCase(1, 5, 3)]
        [TestCase(2, 10, 4)]
        [TestCase(10, 50, 12)]
        [Test]
        public async Task Compose_Parralel_Should_Compute_Result_OK(int entry, int s1, int s2)
        {

            var current = Thread.CurrentThread;
            var finaliser1 = PipeLine.Create<int, int>(_Func.Function).Next(_Act.Action);
            var finaliser2 = PipeLine.Create<int, int>(_Func3.Function).Next(_Act2.Action);

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
        [Test]
        public async Task Compose_Parralel_Should_Compute_Result_OK()
        {
            var current = Thread.CurrentThread;

            var fin = PipeLine.Create<int>(_Act.Action);

            var pip1 = PipeLine.Create<int, int>(_Func.Function).Next(fin);
            var pip2 = PipeLine.Create<int, int>(_Func2.Function).Next(fin);

            var pip = PipeLine.Create<int, int>(a => a * 2).Next(pip1, pip2);

            await pip.Consume(1);

            _Act.Threads.Count.Should().Be(1);
            _Act.CallingThread.Should().NotBe(current);
            _Act.CallingThread.Should().NotBe(_Func.CallingThread);
            _Act.CallingThread.Should().NotBe(_Func2.CallingThread);
        }
      
    }
}
