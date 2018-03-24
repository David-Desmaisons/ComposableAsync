using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 
using FluentAssertions;
using System.Threading;
using EasyActor.Factories;
using Xunit;

namespace EasyActor.Examples
{
    public class ConcurencyTest : IDisposable
    {
        private readonly List<Thread> _Threads;
        private readonly int _ThreadCount = 100;
        private IDoStuff _IActor;

        public ConcurencyTest()
        {
            _Threads = Enumerable.Range(0, _ThreadCount).Select(_ => new Thread(() => { Thread.Sleep(5); TestActor().Wait(); })).ToList();
        }

        public void Dispose()
        {
            _Threads.ForEach(t => t.Abort());
        }

        private async Task TestActor()
        {
            await _IActor.DoStuff();
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task NoActor_Should_Generate_Random_Output(IAsyncDisposable factory, IDoStuff stuffer, bool safe)
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

            if (factory!=null)
                await factory.DisposeAsync();
        }

        private static object[] BuildTestData(IActorFactory factory, IDoStuff stuffer) 
        {
            return new object[] { factory, factory.Build(stuffer), true };
        }

        private static IEnumerable<object[]> GetOkTestData(IEnumerable<Func<IActorFactory>> factories, IEnumerable<Func<IDoStuff>> stuffers) 
        {
            return factories.SelectMany(f => stuffers, (f, s) => BuildTestData(f(), s()));
        }

        private static IEnumerable<Func<IActorFactory>> Factories 
        {
            get 
            {
                yield return () => new ActorFactory();
                yield return () => new TaskPoolActorFactory();
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

        public static IEnumerable<object[]> TestCases
        {
            get 
            {
                yield return new object[] { null, new StufferSleep(), false };
                yield return new object[] { null, new StufferAwait(), false };

                foreach (var td in GetOkTestData(Factories, Stuffers)) 
                {
                    yield return td;
                }
            }
        }
    }
}
