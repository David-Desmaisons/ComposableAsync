using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System.Threading;
using EasyActor.TaskHelper;
using System.Collections;

namespace EasyActor.Examples
{
    [TestFixture]
    public class ConcurencyTest
    {
        private List<Thread> _Threads;
        private int _ThreadCount = 1000;
        private IDoStuff _IActor;

        [SetUp]
        public void SetUp()
        {
            _Threads = Enumerable.Range(0, _ThreadCount).Select(_ => new Thread(() => { Thread.Sleep(5); TestActor().Wait(); })).ToList();
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

        private class DataTestFactory
        {

            private static TestCaseData BuildTestData(IActorFactory factory, IDoStuff stuffer)
            {
                return new TestCaseData(factory.Build(stuffer), true).SetName(string.Format("{0} {1} Should Be Ok", factory, stuffer));
            }

            private static IEnumerable<TestCaseData> GetOKTestData(IEnumerable<IActorFactory> factories, IEnumerable<Func<IDoStuff>> stuffers)
            {
                return factories.SelectMany(f => stuffers, (f, s) => BuildTestData(f,s()));
            }

            private static IEnumerable<IActorFactory> Factories 
            {
                get
                {
                    yield return new ActorFactory();
                    yield return new TaskPoolActorFactory();
                }
            }

            private static IEnumerable<Func<IDoStuff>> Stuffers
            {
                get
                {
                    yield return () => new StufferSleep();
                    yield return () => new StufferAwait();
                }
            }

            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(new StufferSleep(), false).SetName("No actor Sleep Should be KO");
                    yield return new TestCaseData(new StufferAwait(), false).SetName("No actor Await Should be KO");

                    foreach (var td in GetOKTestData(Factories, Stuffers))
                    {
                        yield return td;
                    }
                }
            }  
        }

        [Test, TestCaseSource(typeof(DataTestFactory), "TestCases")]
        public async Task NoActor_Should_Generate_Random_Output(IDoStuff stuffer, bool safe)
        {
            _IActor = stuffer;
            //act
            _Threads.ForEach(t => t.Start());
            _Threads.ForEach(t => t.Join());

            //assert
            var res = await stuffer.GetCount();
            if (safe)
                res.Should().Be(_ThreadCount); 
            else
                res.Should().NotBe(_ThreadCount); 
        }
    }
}
