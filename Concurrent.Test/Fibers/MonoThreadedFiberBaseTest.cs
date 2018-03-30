using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Disposable;
using Concurrent.Tasks;
using Concurrent.Test.TestHelper;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Concurrent.Test.Fibers
{
    public abstract class MonoThreadedFiberBaseTest : IAsyncLifetime
    {
        protected Thread RunningThread;
        private readonly ComposableAsyncDisposable _Disposables = new ComposableAsyncDisposable();

        protected MonoThreadedFiberBaseTest()
        {
            RunningThread = null;
        }

        public Task InitializeAsync() => TaskBuilder.Completed;

        public Task DisposeAsync() => _Disposables.DisposeAsync();

        protected abstract IMonoThreadFiber GetFiber(Action<Thread> onCreate = null);

        protected IMonoThreadFiber GetSafeFiber(Action<Thread> onCreate = null)
        {
            var fiber = GetFiber(onCreate);
            return _Disposables.Add(fiber);
        }

        protected Task TaskFactory(int sleep = 1)
        {
            RunningThread = Thread.CurrentThread;
            Thread.Sleep(sleep * 1000);
            return TaskBuilder.Completed;
        }

        protected Task<T> TaskFactory<T>(T result, int sleep = 1)
        {
            RunningThread = Thread.CurrentThread;
            Thread.Sleep(sleep * 1000);
            return Task.FromResult(result);
        }

        protected Task Throw()
        {
            throw new Exception();
        }

        protected Task<T> Throw<T>()
        {
            throw new Exception();
        }

        [Fact]
        public async Task Enqueue_Task_Should_Run_OnSeparatedThread()
        {
            var current = Thread.CurrentThread;
            //arrange
            var target = GetSafeFiber();

            //act
            await target.Enqueue(() => TaskFactory());

            //assert
            RunningThread.Should().NotBeNull();
            RunningThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Enqueue_Task_Should_Run_OnSameThread()
        {
            //arrange
            var target = GetSafeFiber();

            //act
            await target.Enqueue(() => TaskFactory());
            var first = RunningThread;
            RunningThread = null;
            await target.Enqueue(() => TaskFactory());

            //assert
            RunningThread.Should().Be(first);
        }

        [Fact]
        public async Task Enqueue_Task_With_Cancellation_Should_Run_OnSameThread()
        {
            //arrange
            var target = GetSafeFiber();

            //act
            await target.Enqueue(() => TaskFactory(), CancellationToken.None);
            var first = RunningThread;
            RunningThread = null;
            await target.Enqueue(() => TaskFactory(), CancellationToken.None);

            //assert
            RunningThread.Should().Be(first);
        }

        [Fact]
        public async Task Enqueue_Task_With_Cancellation_Should_Run_OnSeparatedThread()
        {
            var current = Thread.CurrentThread;
            //arrange
            var target = GetSafeFiber();

            //act
            await target.Enqueue(() => TaskFactory(), CancellationToken.None);

            //assert
            RunningThread.Should().NotBeNull();
            RunningThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Enqueue_Task_With_Cancellation_Imediatelly_Cancel_Tasks_Enqueued()
        {
            var target = GetSafeFiber();
            var cancellationTokenSource = new CancellationTokenSource();
            var watch = Stopwatch.StartNew();

            var taskEnqueued = target.Enqueue(() => TaskFactory(2));
            var newTask = target.Enqueue(() => TaskFactory(), cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await AwaitForCancellation(newTask);
           
            var time = watch.Elapsed;

            await taskEnqueued;

            time.Should().BeLessThan(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Enqueue_Task_With_Cancellation_Do_Not_Run_Task_Cancelled_Enqueued()
        {
            var target = GetSafeFiber();
            var cancellationTokenSource = new CancellationTokenSource();
            var taskHasbeenCalled = false;

            var taskEnqueued = target.Enqueue(() => TaskFactory(2));
            var newTask = target.Enqueue(() => {
                taskHasbeenCalled = true;
                return TaskBuilder.Completed;
            }, cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await AwaitForCancellation(newTask);

            await taskEnqueued;

            await Task.Delay(200);

            taskHasbeenCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Enqueue_after_await_does_not_continue_on_actor_thread()
        {
            //arrange
            var target = GetSafeFiber();

            //act
            var thread = await target.Enqueue(() => Thread.CurrentThread);

            thread.Should().NotBe(Thread.CurrentThread);
        }

        [Fact]
        public async Task Dispatch_And_Enqueue_Should_Run_OnSameThread()
        {
            //arrange
            var target = GetSafeFiber();

            //act
            await target.Enqueue(() => TaskFactory());
            var first = RunningThread;
            RunningThread = null;
            target.Dispatch(() => { RunningThread = Thread.CurrentThread; });
            await Task.Delay(200);

            //assert
            RunningThread.Should().Be(first);
        }

        [Fact]
        public async Task Enqueue_Should_DispatchException()
        {
            //arrange
            var target = GetSafeFiber();
            Exception error = null;
            //act
            try
            {
                await target.Enqueue(Throw);
            }
            catch (Exception e)
            {
                error = e;
            }

            //assert
            error.Should().NotBeNull();
        }

        [Fact]
        public async Task Enqueue_Task_Exception_Should_Not_Kill_MainThead()
        {
            //arrange
            var target = GetSafeFiber();
            //act
            try
            {
                await target.Enqueue(Throw);
            }
            catch
            {
            }

            await target.Enqueue(() => TaskFactory());
        }

        [Theory, AutoData]
        public async Task Enqueue_Task_T_Return_Result(int data)
        {
            //arrange
            var target = GetSafeFiber();

            //act
            var res = await target.Enqueue(() => TaskFactory<int>(data));

            //assert
            res.Should().Be(data);
        }

        [Fact]
        public async Task Enqueue_Task_T_Run_On_Separated_Thread()
        {
            var current = Thread.CurrentThread;
            //arrange
            var target = GetSafeFiber();

            //act
            await target.Enqueue(() => TaskFactory<int>(12));

            var thread = RunningThread;
            thread.Should().NotBeNull();
            thread.Should().NotBe(current);

            //assert
            await target.Enqueue(() => TaskFactory<int>(12));
            RunningThread.Should().NotBeNull();
            RunningThread.Should().Be(thread);
        }

        [Theory, AutoData]
        public async Task Enqueue_Task_T_With_Cancellation_Return_Result(int data)
        {
            var current = Thread.CurrentThread;
            //arrange
            var target = GetSafeFiber();

            //act
            var res = await target.Enqueue(() => TaskFactory<int>(data), CancellationToken.None);

            //assert
            res.Should().Be(data);
            RunningThread.Should().NotBeNull();
            RunningThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Enqueue_Task_T_With_Cancellation_Run_On_Separated_Thread()
        {
            var current = Thread.CurrentThread;
            //arrange
            var target = GetSafeFiber();

            //act
            await target.Enqueue(() => TaskFactory<int>(12));

            var thread = RunningThread;
            thread.Should().NotBeNull();
            thread.Should().NotBe(current);

            //assert
            await target.Enqueue(() => TaskFactory<int>(12), CancellationToken.None);
            RunningThread.Should().NotBeNull();
            RunningThread.Should().Be(thread);
        }

        [Fact]
        public async Task Enqueue_Task_T_With_Cancellation_Imediatelly_Cancel_Tasks_Enqueued()
        {
            var target = GetSafeFiber();
            var cancellationTokenSource = new CancellationTokenSource();
            var watch = Stopwatch.StartNew();

            var taskEnqueued = target.Enqueue(() => TaskFactory<int>(12, 2));
            var newTask = target.Enqueue(() => TaskFactory<int>(12), cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            await AwaitForCancellation(newTask);

            var time = watch.Elapsed;

            await taskEnqueued;
          
            time.Should().BeLessThan(TimeSpan.FromSeconds(1));
        }

        [Theory, AutoData]
        public async Task Enqueue_Func_T_Should_Work_AsExpected_With_Result(int value)
        {

            Thread current = Thread.CurrentThread;
            //arrange
            var target = GetSafeFiber();
            Func<int> func = () => { RunningThread = Thread.CurrentThread; return value; };

            //act
           await target.Enqueue(func);

            //assert
            value.Should().Be(value);
            RunningThread.Should().NotBeNull();
            RunningThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Enqueue_Task_T_Should_DispatchException_With_Result()
        {
            //arrange
            var target = GetSafeFiber();
            Exception error = null;
            //act
            try
            {
                await target.Enqueue(Throw<string>);
            }
            catch (Exception e)
            {
                error = e;
            }

            //assert
            error.Should().NotBeNull();
        }

        [Fact]
        public async Task Enqueue_Task_T_Exception_Should_Not_Kill_MainThead_With_Result()
        {
            //arrange
            var target = GetSafeFiber();
            //act
            try
            {
                await target.Enqueue(Throw<int>);
            }
            catch
            {
            }

            var res = await target.Enqueue(() => TaskFactory<int>(25));

            //assert
            res.Should().Be(25);
        }

        [Fact]
        public async Task Enqueue_Action_Should_Work_OnAction()
        {
            //arrange
            var target = GetSafeFiber();

            bool done = false;
            Action act = () => done = true;
            //act

            await target.Enqueue(act);

            //assert
            done.Should().BeTrue();
        }


        [Fact]
        public async Task Enqueue_Action_Should_ReDispatch_Exception_OnAction()
        {
            //arrange
            var target = GetSafeFiber();
            Exception error = null;
            Action act = () => Throw();
            //act
            try
            {
                await target.Enqueue(act);
            }
            catch (Exception e)
            {
                error = e;
            }

            //assert
            error.Should().NotBeNull();
        }

        [Fact]
        public async Task Enqueue_Task_Should_Not_Cancel_Already_Runing_Task_OnDispose()
        {
            Task newTask = null;

            //arrange
            var target = GetSafeFiber(t => t.Priority = ThreadPriority.Highest);
            newTask = target.Enqueue(() => TaskFactory(3));

            while (RunningThread == null)
            {
                Thread.Sleep(100);
            }

            await newTask;

            RunningThread.Should().NotBeNull();

            newTask.IsCanceled.Should().BeFalse();
        }

        [Fact]
        public async Task Enqueue_Task_Should_Cancel_Task_When_Added_On_Disposed_Queue()
        {
            //arrange
            var fiber = GetSafeFiber();
            var task = fiber.Enqueue(() => TaskFactory());
            await fiber.DisposeAsync();

            var newesttask = fiber.Enqueue(() => TaskFactory());

            TaskCanceledException error = null;
            try
            {
                await newesttask;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            newesttask.IsCanceled.Should().BeTrue();
            error.Should().NotBeNull();
        }

        [Fact]
        public async Task Enqueue_Action_Should_Cancel_Task_When_On_Disposed()
        {
            var fiber = GetSafeFiber();
            var task = fiber.Enqueue(() => TaskFactory());
            await fiber.DisposeAsync();

            var done = false;
            var newesttask = fiber.Enqueue(() => { done = true; });

            TaskCanceledException error = null;
            try
            {
                await newesttask;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            newesttask.IsCanceled.Should().BeTrue();
            error.Should().NotBeNull();
            done.Should().BeFalse();
        }

        [Fact]
        public async Task Enqueue_Func_T_Should_Cancel_Task_When_On_Disposed_Queue()
        {
            var fiber = GetSafeFiber();
            var task = fiber.Enqueue(() => TaskFactory());
            await fiber.DisposeAsync();

            Func<int> func = () => 25;
            Task newesttask = fiber.Enqueue(func);

            TaskCanceledException error = null;
            try
            {
                await newesttask;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            newesttask.IsCanceled.Should().BeTrue();
            error.Should().NotBeNull();
        }

        [Fact]
        public async Task Dispose_Running_Task_Should_Continue_After_Stoping_Queue()
        {
            //arrange  
            var fiber = GetSafeFiber();
            var task = fiber.Enqueue(() => TaskFactory(3));
            while (RunningThread == null)
            {
                Thread.Sleep(100);
            }

            //act
            await fiber.DisposeAsync();
            await task;

            //assert
            task.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Dispose_Enqueue_Items_Should_Return_Canceled_Task_After_Stoping_Queue()
        {
            //arrange  
            var fiber = GetSafeFiber();
            await fiber.DisposeAsync();

            var done = false;

            TaskCanceledException error = null;
            try
            {
                await fiber.Enqueue(() => { done = true; });
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            error.Should().NotBeNull();
            done.Should().BeFalse();
        }

        [Fact]
        public async Task Enqueue_Task_Should_Cancel_Not_Started_Task_When_OnDispose()
        {
            Task newTask = null, notstarted = null;

            //arrange
            var target = GetSafeFiber(t => t.Priority = ThreadPriority.Highest);
            newTask = target.Enqueue(() => TaskFactory(3));

            while (RunningThread == null)
            {
                Thread.Sleep(100);
            }
            await target.DisposeAsync();

            //act
            notstarted = target.Enqueue(() => TaskFactory(3));

            await newTask;

            //assert
            TaskCanceledException error = null;
            try
            {
                await notstarted;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            notstarted.IsCanceled.Should().BeTrue();
            error.Should().NotBeNull();
        }


        [Fact]
        public async Task Enqueue_Action_Should_Cancel_Not_Started_Task_When_OnDispose()
        {
            Task newTask = null, notstarted = null;
            var done = false;

            //arrange
            var target = GetSafeFiber(t => t.Priority = ThreadPriority.Highest);
            newTask = target.Enqueue(() => TaskFactory(3));

            while (RunningThread == null)
            {
                Thread.Sleep(100);
            }
            await target.DisposeAsync();

            notstarted = target.Enqueue(() => { done = true; });

            await newTask;

            TaskCanceledException error = null;
            try
            {
                await notstarted;
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            notstarted.IsCanceled.Should().BeTrue();
            error.Should().NotBeNull();
            done.Should().BeFalse();
        }

        [Fact]
        public async Task Enqueue_Action_Runs_Actions_Sequencially()
        {
            var target = GetSafeFiber();
            var tester = new SequenceTester(target);
            await tester.Stress();
            tester.Count.Should().Be(tester.MaxThreads);
        }

        [Fact]
        public async Task Enqueue_Task_Runs_Actions_Sequencially_after_await()
        {
            var target = GetSafeFiber();
            var tester = new SequenceTester(target);
            await tester.StressTask();
            tester.Count.Should().Be(tester.MaxThreads);
        }

        private static async Task AwaitForCancellation(Task toBeCancelled)
        {
            var cancelled = false;
            try
            {
                await toBeCancelled;
            }
            catch (TaskCanceledException)
            {
                cancelled = true;
            }
            cancelled.Should().BeTrue();
        }
    }
}
