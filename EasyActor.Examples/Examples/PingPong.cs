using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Concurrent.Tasks;
using EasyActor.Factories;
using Xunit;
using Xunit.Abstractions;

namespace EasyActor.Examples
{

    public class PingPong : IAsyncLifetime
    {
        private readonly ITestOutputHelper _Output;
        private IActorFactory _ActorFactory;

        public PingPong(ITestOutputHelper output = null)
        {
            _Output = output;
        }
        public Task InitializeAsync() => TaskBuilder.Completed;

        public async Task DisposeAsync()
        {
            await _ActorFactory?.DisposeAsync();
        }

        public static IEnumerable<object[]> GetFactories()
        {
            yield return new object[] { new ActorFactory() };
            yield return new object[] { new SharedThreadActorFactory() };
            yield return new object[] { new TaskPoolActorFactory() };
            yield return new object[] { new ActorTheadPoolFactory() };        
        }

        [Theory]
        [MemberData(nameof(GetFactories))]
        public async Task Test(IActorFactory fact)
        {
            Output(fact.ToString());

            var one = new PingPongerAsync("Bjorg");
            var actor1 = fact.Build<IPingPongerAsync>(one);

            var two = new PingPongerAsync("Lendl");
            var actor2 = fact.Build<IPingPongerAsync>(two);

            one.PongerAsync = actor2;
            two.PongerAsync = actor1;

            var watch = new Stopwatch();
            watch.Start();

            await actor1.Ping();
            Thread.Sleep(10000);

            await Task.WhenAll(StopActor(actor1), StopActor(actor2));

            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");

            _ActorFactory = fact;
        }

        private static Task StopActor(object actor)
        {
            var lifeCyle = actor as IAsyncDisposable;
            return lifeCyle?.DisposeAsync() ?? TaskBuilder.Completed;
        }

        [Theory]
        [MemberData(nameof(GetFactories))]
        public async Task TestNoTask(IActorFactory fact)
        {
            var one = new PingPongerSimple("Bjorg");
            var actor1 = fact.Build<IPingPonger>(one);

            var two = new PingPongerSimple("Lendl");
            var actor2 = fact.Build<IPingPonger>(two);

            one.Ponger = actor2;
            two.Ponger = actor1;

            var watch = new Stopwatch();
            watch.Start();

            actor1.Ping();
            Thread.Sleep(10000);

            await Task.WhenAll(StopActor(actor1), StopActor(actor2));

            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");

            _ActorFactory = fact;
        }

        private void Output(string message)
        {
            Console.WriteLine(message);
            _Output?.WriteLine(message);
        }
    }
}
