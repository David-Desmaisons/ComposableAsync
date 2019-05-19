using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using ComposableAsync.Concurrent.Collections;
using ComposableAsync.Concurrent.Fibers;
using ComposableAsync.Concurrent.WorkItems;
using ComposableAsync.Factory;
using Xunit;
using Xunit.Abstractions;

namespace EasyActor.Examples
{

    public class PingPong 
    {
        private readonly ITestOutputHelper _Output;

        public PingPong(ITestOutputHelper output = null)
        {
            _Output = output;
        }

        private static readonly ProxyFactoryBuilder _ProxyFactoryBuilder = new ProxyFactoryBuilder();

        public static IEnumerable<object[]> GetFactories()
        {
            yield return new object[] { "Not shared", _ProxyFactoryBuilder.GetActorFactory() };
            yield return new object[] { "Shared", _ProxyFactoryBuilder.GetActorFactory(shared:true) };
            yield return new object[] { "TaskBased", _ProxyFactoryBuilder.GetTaskBasedActorFactory() };
            yield return new object[] { "ThreadPool",  _ProxyFactoryBuilder.GetThreadPoolBasedActorFactory() };        
        }

        [Theory]
        [MemberData(nameof(GetFactories))]
        public async Task TestTask(string name, IProxyFactory fact)
        {
            Output(name);

            var one = new PingPongerAsync("Bjorg");
            var actor1 = fact.Build<IPingPongerAsync>(one);

            var two = new PingPongerAsync("Lendl");
            var actor2 = fact.Build<IPingPongerAsync>(two);

            one.PongerAsync = actor2;
            two.PongerAsync = actor1;

            var watch = Stopwatch.StartNew();

            await actor1.Ping();
            Thread.Sleep(10000);

            await fact.DisposeAsync();

            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");
        }

        [Theory]
        [MemberData(nameof(GetFactories))]
        public async Task TestNoTask(string name, IProxyFactory fact)
        {
            Output(name);

            var one = new PingPongerSimple("Bjorg");
            var actor1 = fact.Build<IPingPonger>(one);

            var two = new PingPongerSimple("Lendl");
            var actor2 = fact.Build<IPingPonger>(two);

            one.Ponger = actor2;
            two.Ponger = actor1;

            var watch = Stopwatch.StartNew();

            actor1.Ping();
            Thread.Sleep(10000);

            await fact.DisposeAsync();
            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");
        }

        public static IEnumerable<object[]> GetQueues() 
        {
            yield return new object[] { new SpinningMpscQueue<IWorkItem>() };
            yield return new object[] { new StandardMpscQueue<IWorkItem>() };
            yield return new object[] { new BlockingMpscQueue<IWorkItem>() };      
        }

        [Theory]
        [MemberData(nameof(GetQueues))]
        public async Task Test_Queue_Performance(IMpScQueue<IWorkItem> queueWorkItem)
        {
            Output(queueWorkItem.GetType().Name);

            var fiber = new MonoThreadedFiber(null, queueWorkItem);
            var factory = _ProxyFactoryBuilder.GetActorFactoryFrom(fiber);

            await TestNoTask("Fiber", factory);

            await fiber.DisposeAsync();
        }

        [Theory]
        [MemberData(nameof(GetFactories))]
        public async Task TestTask_T(string name, IProxyFactory fact)
        {
            Output(name);

            var one = new PingPongerBoolAsync("Bjorg");
            var actor1 = fact.Build<IPingPongerBoolAsync>(one);

            var two = new PingPongerBoolAsync("Lendl");
            var actor2 = fact.Build<IPingPongerBoolAsync>(two);

            one.PongerAsync = actor2;
            two.PongerAsync = actor1;

            var watch = Stopwatch.StartNew();

            await actor1.Ping();
            Thread.Sleep(10000);

            await fact.DisposeAsync();
            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");
        }



        [Theory]
        [MemberData(nameof(GetFactories))]
        public async Task TestTask_T_CancellationToken(string name, IProxyFactory fact)
        {
            Output(name);

            var one = new PingPongerAsyncCancellable("Noa");
            var actor1 = fact.Build<IPingPongerAsyncCancellable>(one);

            var two = new PingPongerAsyncCancellable("Wilander");
            var actor2 = fact.Build<IPingPongerAsyncCancellable>(two);

            one.PongerAsync = actor2;
            two.PongerAsync = actor1;

            var watch = Stopwatch.StartNew();

            await actor1.Ping(CancellationToken.None);
            Thread.Sleep(10000);

            await fact.DisposeAsync();
            watch.Stop();

            Output($"Total Ping:{one.Count}, Total Pong:{two.Count} Total Time: {watch.ElapsedMilliseconds} ms");
            Output($"Operation/ms:{(one.Count + two.Count) / watch.ElapsedMilliseconds}");
        }

        private void Output(string message)
        {
            Console.WriteLine(message);
            _Output?.WriteLine(message);
        }
    }
}
