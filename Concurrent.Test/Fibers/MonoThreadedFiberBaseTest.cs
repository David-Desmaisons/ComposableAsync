using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent.Tasks;
using Concurrent.Test.TestHelper;
using FluentAssertions;
using Xunit;

namespace Concurrent.Test.Fibers
{
    public abstract class MonoThreadedFiberBaseTest
    {
        protected Thread RunningThread;

        protected MonoThreadedFiberBaseTest()
        {
            RunningThread = null;
        }

        protected abstract IMonoThreadFiber Getfiber(Action<Thread> onCreate = null);

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
        public async Task Enqueue_Should_Run_OnSeparatedThread()
        {
            var current = Thread.CurrentThread;
            //arrange
            var target = Getfiber();

            //act
            await target.Enqueue(() => TaskFactory());

            //assert
            RunningThread.Should().NotBeNull();
            RunningThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Enqueue_Should_Run_OnSameThread()
        {
            //arrange
            var target = Getfiber();

            //act
            await target.Enqueue(() => TaskFactory());
            var first = RunningThread;
            RunningThread = null;
            await target.Enqueue(() => TaskFactory());

            //assert
            RunningThread.Should().Be(first);
        }

        [Fact]
        public async Task Dispatch_And_Enqueue_Should_Run_OnSameThread()
        {
            //arrange
            var target = Getfiber();

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
            var target = Getfiber();
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
        public async Task Enqueue_Exception_Should_Not_Kill_MainThead()
        {
            //arrange
            var target = Getfiber();
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

        [Fact]
        public async Task Enqueue_Should_Work_With_Result()
        {
            var current = Thread.CurrentThread;
            //arrange
            var target = Getfiber();

            //act
            var res = await target.Enqueue(() => TaskFactory<int>(25));

            //assert
            res.Should().Be(25);
            RunningThread.Should().NotBeNull();
            RunningThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Enqueue_Func_T_Should_Work_AsExpected_With_Result()
        {

            Thread current = Thread.CurrentThread;
            //arrange
            var target = Getfiber();
            Func<int> func = () => { RunningThread = Thread.CurrentThread; return 25; };

            //act
            var res = await target.Enqueue(func);

            //assert
            res.Should().Be(25);
            RunningThread.Should().NotBeNull();
            RunningThread.Should().NotBe(current);
        }


        [Fact]
        public async Task Enqueue_Should_DispatchException_With_Result()
        {
            //arrange
            var target = Getfiber();
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
        public async Task Enqueue_Exception_Should_Not_Kill_MainThead_With_Result()
        {
            //arrange
            var target = Getfiber();
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
        public async Task Enqueue_Should_Work_OnAction()
        {
            //arrange
            var target = Getfiber();

            bool done = false;
            Action act = () => done = true;
            //act

            await target.Enqueue(act);

            //assert
            done.Should().BeTrue();
        }


        [Fact]
        public async Task Enqueue_Should_ReDispatch_Exception_OnAction()
        {
            //arrange
            var target = Getfiber();
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
        public async Task Enqueue_Should_Not_Cancel_Already_Runing_Task_OnDispose()
        {
            Task newTask = null;

            //arrange
            var target = Getfiber(t => t.Priority = ThreadPriority.Highest);
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
            var fiber = Getfiber();
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
        public async Task Enqueue_Action_Should_Cancel_Task_When_On_Disposed_Queue()
        {
            var fiber = Getfiber();
            var task = fiber.Enqueue(() => TaskFactory());
            await fiber.DisposeAsync();

            var Done = false;
            var newesttask = fiber.Enqueue(() => { Done = true; });

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
            Done.Should().BeFalse();
        }

        [Fact]
        public async Task Enqueue_Func_T_Should_Cancel_Task_When_On_Disposed_Queue()
        {
            var fiber = Getfiber();
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
            var fiber = Getfiber();
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
            var fiber = Getfiber();
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
            var target = Getfiber(t => t.Priority = ThreadPriority.Highest);
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
            var target = Getfiber(t => t.Priority = ThreadPriority.Highest);
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
            var target = Getfiber();
            var tester = new SequenceTester(target);
            await tester.Stress();
            tester.Count.Should().Be(tester.MaxThreads);
        }

        [Fact]
        public async Task Enqueue_Task_Runs_Actions_Sequencially_after_await()
        {
            var target = Getfiber();
            var tester = new SequenceTester(target);
            await tester.StressTask();
            tester.Count.Should().Be(tester.MaxThreads);
        }
    }
}
