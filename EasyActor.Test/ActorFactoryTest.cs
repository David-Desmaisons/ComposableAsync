using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Concurrent.Tasks;
using Concurrent.Test.TestHelper;
using EasyActor.Options;
using FluentAssertions;
using EasyActor.Test.TestInfra.DummyClass;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace EasyActor.Test
{
    public class ActorFactoryTest : IAsyncLifetime
    {
        private readonly IActorFactory _Factory;

        public ActorFactoryTest()
        {
            _Factory = new FactoryBuilder().GetFactory();
        }

        public Task InitializeAsync() => TaskBuilder.Completed;

        public Task DisposeAsync() => _Factory.DisposeAsync();

        private static readonly FactoryBuilder _FactoryBuilder = new FactoryBuilder();

        [Fact]
        public void Type_Should_Be_Standard()
        {
            _Factory.Type.Should().Be(ActorFactorType.Standard);
        }

        [Fact]
        public async Task Method_Should_Run_On_Separated_Thread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(target);
            await actor.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Method_Should_Always_Run_On_Same_Thread()
        {
            var target = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(target);
            await actor.DoAsync();

            var thread = target.CallingThread;

            await actor.DoAsync();

            target.CallingThread.Should().Be(thread);
        }

        [Fact]
        public async Task Each_Actor_Should_Run_On_Separated_Thread_When_Shared_Thread_Is_False()
        {
            //arrange
            var target1 = new DummyClass();
            var target2 = new DummyClass();
            var actor1 = _Factory.Build<IDummyInterface2>(target1);
            var actor2 = _Factory.Build<IDummyInterface2>(target2);

            //act
            await actor1.DoAsync();
            await actor2.DoAsync();

            //assert
            target1.CallingThread.Should().NotBeNull();
            target2.CallingThread.Should().NotBeNull();
            target1.CallingThread.Should().NotBe(target2.CallingThread);
        }

        [Fact]
        public async Task Method_Should_Run_On_Same_Thread_After_Await()
        {
            var target = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(target);

            var res = await actor.DoAnRedoAsync();

            res.Item1.Should().Be(res.Item2);
        }

        [Fact]
        public async Task Method_Task_Should_Be_Cancelled_As_Soon_As_Possible()
        {
            var target = new CancellableImplementation();
            var actor = _Factory.Build<ICancellableInterface>(target);

            var longRunningTask = actor.Do(1, CancellationToken.None);

            var cancellationTokenSource = new CancellationTokenSource();

            var stopWatch = Stopwatch.StartNew();
            var taskToBeCancelled = actor.Do(1, cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
            stopWatch.Stop();

            var exception = await TaskEnqueueWithCancellationTester.AwaitForException(taskToBeCancelled);
            var delay = stopWatch.Elapsed;

            await _Factory.DisposeAsync();
            try
            {
                await longRunningTask;
            }
            catch
            {
                // ignored
            }

            exception.Should().NotBeNull();
            exception.Should().BeAssignableTo<TaskCanceledException>();
            delay.Should().BeLessThan(TimeSpan.FromSeconds(0.5));
        }

        [Fact]
        public async Task Method_Task_T_Should_Be_Cancelled_As_Soon_As_Possible()
        {
            var target = new CancellableImplementation();
            var actor = _Factory.Build<ICancellableInterface>(target);

            var longRunningTask = actor.GetIntResult(1, CancellationToken.None);

            var cancellationTokenSource = new CancellationTokenSource();

            var stopWatch = Stopwatch.StartNew();
            var taskToBeCancelled = actor.GetIntResult(1, cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
            stopWatch.Stop();

            var exception = await TaskEnqueueWithCancellationTester.AwaitForException(taskToBeCancelled);
            var delay = stopWatch.Elapsed;

            await _Factory.DisposeAsync();
            try
            {
                await longRunningTask;
            }
            catch
            {
                // ignored
            }

            exception.Should().NotBeNull();
            exception.Should().BeAssignableTo<TaskCanceledException>();
            delay.Should().BeLessThan(TimeSpan.FromSeconds(0.5));
        }


        [Fact]
        public async Task Method_Task_T_Generic_Should_Be_Cancelled_As_Soon_As_Possible()
        {
            var target = new CancellableImplementation();
            var actor = _Factory.Build<ICancellableInterface>(target);

            var longRunningTask = actor.GetResult(1, string.Empty, CancellationToken.None);

            var cancellationTokenSource = new CancellationTokenSource();

            var stopWatch = Stopwatch.StartNew();
            var taskToBeCancelled = actor.GetResult(1, string.Empty, cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();
            stopWatch.Stop();

            var exception = await TaskEnqueueWithCancellationTester.AwaitForException(taskToBeCancelled);
            var delay = stopWatch.Elapsed;

            await _Factory.DisposeAsync();
            try
            {
                await longRunningTask;
            }
            catch
            {
                // ignored
            }

            exception.Should().NotBeNull();
            exception.Should().BeAssignableTo<TaskCanceledException>();
            delay.Should().BeLessThan(TimeSpan.FromSeconds(0.5));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Build_Should_Create_Proxy_That_Handles_Generic_Interface(bool value)
        {
            var target = new GenericImplementation();
            var actor = _Factory.Build<IGenericInterface<string>>(target);

            var res = await actor.GetResult(value);

            res.Should().Be(value);
            target.LastCallingThread.Should().NotBe(Thread.CurrentThread);
        }

        [Theory, AutoData]
        public async Task Build_Should_Create_Proxy_That_Handles_Generic_Interface_2(string value)
        {
            var target = new GenericImplementation();
            var actor = _Factory.Build<IGenericInterface<string>>(target);

            var res = await actor.GetResult(value);

            res.Should().Be($"{value}-transformed");
            target.LastCallingThread.Should().NotBe(Thread.CurrentThread);
        }

        [Theory, AutoData]
        public async Task Build_Should_Create_Proxy_That_Handles_Generic_Interface_3(int value)
        {
            var target = new GenericImplementation();
            var actor = _Factory.Build<IGenericInterface<string>>(target);

            var res = await actor.GetResultString(value);

            res.Should().Be($"{value}");
            target.LastCallingThread.Should().NotBe(Thread.CurrentThread);
        }

        [Fact]
        public void Build_Should_CreateSameInterface_ForSamePOCO()
        {
            var target = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(target);
            var intface2 = _Factory.Build<IDummyInterface2>(target);

            actor.Should().BeSameAs(intface2);
        }

        [Fact]
        public async Task Build_Should_Throw_Exception_IsSamePOCO_HasBeenUsedWithOtherFactory()
        {
            var target = new DummyClass();
            var sharedFactory = _FactoryBuilder.GetFactory(shared: true);
            var actor = sharedFactory.Build<IDummyInterface2>(target);

            Action Do = () => _Factory.Build<IDummyInterface2>(target);

            Do.Should().Throw<ArgumentException>().And.Message.Should().Contain("Shared");

            await sharedFactory.DisposeAsync();
        }

        [Fact]
        public async Task Task_returned_By_Method_Should_Be_Awaited()
        {
            var target = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(target);
            await actor.SlowDoAsync();

            target.Done.Should().BeTrue();
        }

        [Fact]
        public async Task Method_With_Task_T_Should_Run_On_Separated_Tread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            var actor = _Factory.Build<IDummyInterface2>(target);
            var result = await actor.ComputeAsync(25);

            result.Should().Be(25);
            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public void Method_returning_void_Task_Should_Not_Throw_Exception()
        {
            var actor = _Factory.Build<IDummyInterface2>(new DummyClass());
            Action Do = () => actor.Do();
            Do.Should().NotThrow();
        }

        [Fact]
        public async Task BuildAsync_Should_Call_Constructor_On_Actor_Thread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            var actor = await _Factory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await actor.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.Should().Be(target.CallingThread);
        }

        [Fact]
        public void Actor_Should_Implement_IFiberProvider()
        {
            var actor = _Factory.Build<IDummyInterface2>(new DummyClass());
            actor.Should().BeAssignableTo<IFiberProvider>();
        }

        [Fact]
        public void Actor_Should_Returns_Fiber()
        {
            var actor = _Factory.Build<IDummyInterface2>(new DummyClass());
            var fp = actor as IFiberProvider;

            var fiber = fp?.Fiber;

            fiber.Should().NotBeNull();
        }

        [Fact]
        public async Task Actor_IAsyncDisposable_DisposeAsync_Should_Call_Proxified_Class_On_IAsyncDisposable_Case_2()
        {
            //arrange
            var dispclass = new DisposableClass();
            var actor = _Factory.Build<IDummyInterface4>(dispclass);

            await actor.DisposeAsync();
            //assert
            dispclass.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_IAsyncDisposable_DisposeAsync_Should_Call_Proxified_Class_On_ActorThread_Case_2()
        {
            //arrange
            var testThread = Thread.CurrentThread;

            var dispclass = new DisposableClass();
            var actor = _Factory.Build<IDummyInterface4>(dispclass);

            await actor.DoAsync();

            var thread = dispclass.LastCallingThread;
            //act

            await actor.DisposeAsync();
            //assert
            var disposableThread = dispclass.LastCallingThread;

            disposableThread.Should().NotBe(testThread);
            disposableThread.Should().Be(thread);
        }
    }
}
