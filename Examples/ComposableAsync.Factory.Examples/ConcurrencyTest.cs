﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ComposableAsync.Concurrent;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
//using ComposableAsync.Concurrent.Dispatchers;
//using RateLimiter;

namespace ComposableAsync.Factory.Examples
{
    public class ConcurrencyTest : IDisposable
    {
        private readonly List<Thread> _Threads;
        private readonly int _ThreadCount = 100;
        private readonly ITestOutputHelper _TestOutput;
        private IDoStuff _IActor;

        private static readonly IProxyFactoryBuilder _ProxyFactoryBuilder = new ProxyFactoryBuilder();

        public ConcurrencyTest(ITestOutputHelper testOutput)
        {
            _TestOutput = testOutput;
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

            if (factory != null)
                await factory.DisposeAsync();
        }

        [Fact]
        public async Task Dispatcher_Can_Be_Combined()
        {
            LogWithThread($"starting Test");
            //     TODO: uncommment when new version is ready
            var composed = //TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(100)).Then(
                    Fiber.CreateMonoThreadedFiber(t => LogWithThread("starting actor Thread"))
            //         )
            ;

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(800));
            LogWithThread("Start");
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await SwitchThread();
                LogWithThread("Loop");

                await composed;
                LogWithThreadAndTime("Doing");
            }


            var task = (composed as IAsyncDisposable)?.DisposeAsync()?? Task.CompletedTask;
            await task;
            LogWithThread("Ended");
        }

        [Fact]
        public async Task Dispatcher_Can_Be_Combined_To_Form_Proxy() {
            LogWithThread($"starting Test");

            var proxyBuilder = _ProxyFactoryBuilder.GetManagedProxyFactory(
     //     TODO: uncommment when new version is ready
     //     TimeLimiter.GetFromMaxCountByInterval(1, TimeSpan.FromMilliseconds(100)),
                Fiber.CreateMonoThreadedFiber(t => LogWithThread("starting actor Thread"))
            );
            var proxyStuffer = proxyBuilder.Build<IDoStuff>(new StufferLog(_TestOutput));

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(800));
            LogWithThread("Start");
            while (!cancellationTokenSource.IsCancellationRequested) {
                await SwitchThread();
                LogWithThread("Loop");

                await proxyStuffer.DoStuff();
            }

            await proxyBuilder.DisposeAsync();
            LogWithThread("Ended");
        }

        private static YieldAwaitable SwitchThread()
        {
            SynchronizationContext.SetSynchronizationContext(null);
            return Task.Yield();
        }

        private void LogWithThreadAndTime(string message)
        {
            WriteLine($"{message}: {DateTime.Now:O} on Thread {Thread.CurrentThread.ManagedThreadId}");
        }

        private void LogWithThread(string message)
        {
            WriteLine($"{message}: on Thread {Thread.CurrentThread.ManagedThreadId}");
        }

        private void WriteLine(string message)
        {
            _TestOutput.WriteLine(message);
        }

        private static object[] BuildTestData(IProxyFactory factory, IDoStuff stuffer)
        {
            return new object[] { factory, factory.Build(stuffer), true };
        }

        private static IEnumerable<object[]> GetOkTestData(IEnumerable<Func<IProxyFactory>> factories, IEnumerable<Func<IDoStuff>> stuffers)
        {
            return factories.SelectMany(f => stuffers, (f, s) => BuildTestData(f(), s()));
        }

        private static IEnumerable<Func<IProxyFactory>> Factories
        {
            get
            {
                yield return () => _ProxyFactoryBuilder.GetActorFactory();
                yield return () => _ProxyFactoryBuilder.GetTaskBasedActorFactory();
            }
        }

        private static IEnumerable<Func<IDoStuff>> StuffDoers
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

                foreach (var td in GetOkTestData(Factories, StuffDoers))
                {
                    yield return td;
                }
            }
        }
    }
}
