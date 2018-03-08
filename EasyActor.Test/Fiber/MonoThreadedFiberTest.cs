using System;
using System.Threading;
using System.Threading.Tasks;
using EasyActor.Fiber;
using EasyActor.TaskHelper;
using FluentAssertions;
using Xunit;

namespace EasyActor.Test.Fiber
{

    public class MonoThreadFiberTest : MonoThreadedFiberBaseTest
    {
        protected override IMonoThreadFiber GetQueue(Action<Thread> onCreate = null) => new MonoThreadedFiber(onCreate);

        [Fact]
        public async Task Dispose_Enqueued_Task_Should_Continue_After_Stoping_Queue()
        {
            //arrange  
            var queue = new MonoThreadedFiber();
            var task = queue.Enqueue(() => TaskFactory(3));
            while (RunningThread == null)
            {
                Thread.Sleep(100);
            }

            var enqueuedtask = queue.Enqueue(() => TaskFactory(3));
            //act
            await queue.Stop(() => TaskBuilder.Completed);
            await enqueuedtask;

            //assert
            task.IsCompleted.Should().BeTrue();
            enqueuedtask.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Enqueue_Task_Should_Cancel_Not_Started_Task_When_OnDispose()
        {
            Task newTask = null, notstarted = null;

            //arrange
            using (var target = GetQueue(t => t.Priority = ThreadPriority.Highest))
            {
                newTask = target.Enqueue(() => TaskFactory(3));

                while (RunningThread == null)
                {
                    Thread.Sleep(100);
                }

                //act
                notstarted = target.Enqueue(() => TaskFactory(3));
            }

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
            bool Done = false;

            //arrange
            using (var target = GetQueue(t => t.Priority = ThreadPriority.Highest))
            {
                newTask = target.Enqueue(() => TaskFactory(3));

                while (RunningThread == null)
                {
                    Thread.Sleep(100);
                }

                notstarted = target.Enqueue(() => { Done = true; });
            }

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
            Done.Should().BeFalse();
        }
    }
}
