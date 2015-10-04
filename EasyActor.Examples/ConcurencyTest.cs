using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System.Threading;

namespace EasyActor.Examples
{

    public interface IDoStuff
    {
        Task DoStuff();

        Task<int> GetCount();
    }

    public class Stuffer : IDoStuff
    {
        private int _Count = 0;

        //Thread unsafe code
        public Task DoStuff()
        {
            var c = _Count;
            Thread.Sleep(5);
            _Count  = c + 1;
            return Task.FromResult<object>(null);
        }

        public Task<int> GetCount()
        {
            return  Task.FromResult<int>(_Count);
        }
    }


    [TestFixture]
    public class ConcurencyTest
    {
        private List<Thread> _Threads;
        private IDoStuff _IActor;
        private int _ThreadCount = 500;

        [SetUp]
        public void SetUp()
        {
            _Threads = Enumerable.Range(0, _ThreadCount).Select(_ => new Thread(() => {TestActor().Wait();})).ToList();
        }

        [TearDown]
        public void TearDown()
        {
            _Threads.ForEach(t => t.Abort());
        }


        private async Task TestActor()
        {
            await _IActor.DoStuff();
        }


      
        [Test]
        public async Task NoActor_Should_Generate_Random_Output()
        {
            //arrange
            _IActor = new Stuffer();

            //act
            _Threads.ForEach(t => t.Start());
            _Threads.ForEach(t => t.Join());

            //assert
            var res = await _IActor.GetCount();
            res.Should().NotBe(_ThreadCount);
           
        }

        [Test]
        public async Task Actor_Should_Generate_Correct_Output()
        {
            //arrange
            var fact = new ActorFactory();
            _IActor = fact.Build<IDoStuff>( new Stuffer());

            //act
            _Threads.ForEach(t => t.Start());
            _Threads.ForEach(t => t.Join());

            //assert
            var res = await _IActor.GetCount();
            res.Should().Be(_ThreadCount);

        }
    }
}
