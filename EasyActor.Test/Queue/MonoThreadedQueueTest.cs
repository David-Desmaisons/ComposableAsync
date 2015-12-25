using System;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using EasyActor.Queue;
using EasyActor.TaskHelper;

namespace EasyActor.Test
{
    [TestFixture]
    public class MonoThreadQueueTest
    {
        private Thread _RunningThread;


        private Task TaskFactory(int sleep=1)
        {
            _RunningThread = Thread.CurrentThread;
            Thread.Sleep(sleep * 1000);
            return TaskBuilder.Completed;
        }

        private Task<T> TaskFactory<T>(T result, int sleep = 1)
        {
            _RunningThread = Thread.CurrentThread;
            Thread.Sleep(sleep * 1000);
            return Task.FromResult(result);
        }

        private Task Throw()
        {
            throw new Exception();
        }

        private Task<T> Throw<T>()
        {
            throw new Exception();
        }

        [SetUp]
        public void SetUp()
        {
            _RunningThread = null;
        }

          [Test]
        public async Task Enqueue_Should_Run_OnSeparatedThread()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {

                //act
                await target.Enqueue(() => TaskFactory());

                //assert
                _RunningThread.Should().NotBeNull();
                _RunningThread.Should().NotBe(current);
            }
        }

           [Test]
        public async Task Enqueue_Should_Run_OnSameThread()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {

                //act
                await target.Enqueue(() => TaskFactory());
                var first = _RunningThread;
                await target.Enqueue(() => TaskFactory());

                //assert
                _RunningThread.Should().Be(first);
            }
        }

         [Test]
        public async Task Enqueue_Should_DispatchException()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {
                Exception error=null;
                //act
                try
                {
                    await target.Enqueue(() => Throw());
                }
                catch(Exception e)
                {
                    error = e;
                }

                //assert
                error.Should().NotBeNull();
            }
        }

       

         [Test]
        
        public async Task Enqueue_Exception_Should_Not_Kill_MainThead()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {   
                //act
                try
                {
                    await target.Enqueue(() => Throw());
                }
                catch
                {
                }

                await target.Enqueue(() => TaskFactory());      
               
            }
        }

        [Test]
        public async Task Enqueue_Should_Work_With_Result()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {

                //act
                var res = await target.Enqueue(() => TaskFactory<int>(25));

                //assert
                res.Should().Be(25);
                _RunningThread.Should().NotBeNull();
                _RunningThread.Should().NotBe(current);
            }
        }

        [Test]
        public async Task Enqueue_Func_T_Should_Work_AsExpected_With_Result()
        {

            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {
                Func<int> func = () => { _RunningThread = Thread.CurrentThread; return 25; };

                //act
                var res = await target.Enqueue(func);

                //assert
                res.Should().Be(25);
                _RunningThread.Should().NotBeNull();
                _RunningThread.Should().NotBe(current);
            }
        }


         [Test]
        public async Task Enqueue_Should_DispatchException_With_Result()
        {
            //arrange
            using (var target = new MonoThreadedQueue())
            {
                Exception error = null;
                //act
                try
                {
                    await target.Enqueue(() => Throw<string>());
                }
                catch (Exception e)
                {
                    error = e;
                }

                //assert
                error.Should().NotBeNull();
            }
        }

        [Test]
        public async Task Enqueue_Exception_Should_Not_Kill_MainThead_With_Result()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {
                //act
                try
                {
                    await target.Enqueue(() => Throw<int>());
                }
                catch
                {
                }

                var res = await target.Enqueue(() => TaskFactory<int>(25));

                //assert
                res.Should().Be(25);

            }
        }

        [Test]
        public async Task Enqueue_Should_Work_OnAction()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {
                bool done = false;
                Action act = () => done=true;
                //act
                
                await target.Enqueue(act);
              
                //assert
                done.Should().BeTrue();
            }
        }


        [Test]
        public async Task Enqueue_Should_ReDispatch_Exception_OnAction()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new MonoThreadedQueue())
            {
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
        }


        [Test]
        public async Task Enqueue_Should_Not_Cancel_Already_Runing_Task_OnDispose()
        {
            Task newTask = null;

            //arrange
            using (var target = new MonoThreadedQueue(t => t.Priority=ThreadPriority.Highest))
            {
                newTask =  target.Enqueue(() => TaskFactory(3));

                while (_RunningThread == null)
                {
                    Thread.Sleep(100);
                }
            }

            await newTask;
            
            _RunningThread.Should().NotBeNull();
       
            newTask.IsCanceled.Should().BeFalse();
        }


        [Test]
        public async Task Enqueue_Task_Should_Cancel_Not_Started_Task_When_OnDispose()
        {
            Task newTask = null, notstarted = null;

            //arrange
            using (var target = new MonoThreadedQueue(t => t.Priority = ThreadPriority.Highest))
            {
                newTask = target.Enqueue(() => TaskFactory(3));

                while (_RunningThread == null)
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


        [Test]
        public async Task Enqueue_Action_Should_Cancel_Not_Started_Task_When_OnDispose()
        {
            Task newTask = null, notstarted = null;
            bool Done = false;

            //arrange
            using (var target = new MonoThreadedQueue(t=>t.Priority=ThreadPriority.Highest))
            {
                newTask = target.Enqueue(() => TaskFactory(3));

                while (_RunningThread == null)
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

        [Test]
        public async Task Enqueue_Task_Should_Cancel_Task_When_Added_On_Disposed_Queue()
        {
            MonoThreadedQueue queue = null;
            //arrange
            using (queue = new MonoThreadedQueue())
            {
                var task = queue.Enqueue(() => TaskFactory());
            }

            Task newesttask = queue.Enqueue(() => TaskFactory());

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

        [Test]
        public async Task Enqueue_Action_Should_Cancel_Task_When_On_Disposed_Queue()
        {
            MonoThreadedQueue queue = null;
            //arrange
            using (queue = new MonoThreadedQueue())
            {
                var task = queue.Enqueue(() => TaskFactory());
            }

            bool Done = false;
            Task newesttask = queue.Enqueue(() => { Done = true; });

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

        [Test]
        public async Task Enqueue_Func_T_Should_Cancel_Task_When_On_Disposed_Queue()
        {
            MonoThreadedQueue queue = null;
            //arrange
            using (queue = new MonoThreadedQueue())
            {
                var task = queue.Enqueue(() => TaskFactory());
            }


            Func<int> func = () => { return 25; };
            Task newesttask = queue.Enqueue(func);

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

        [Test]
        public async Task Stop_Running_Task_Should_Continue_After_Stoping_Queue()
        {
            //arrange  
            MonoThreadedQueue queue = new MonoThreadedQueue();
            var task = queue.Enqueue(() => TaskFactory(3));
            while (_RunningThread == null)
            {
                Thread.Sleep(100);
            }

            //act
            queue.Stop();
            await task;

            //assert
            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public async Task Stop_Enqueued_Task_Should_Continue_After_Stoping_Queue()
        {
            //arrange  
            MonoThreadedQueue queue = new MonoThreadedQueue();
            Task task = queue.Enqueue(() => TaskFactory(3)) ,enqueuedtask = null;
            while (_RunningThread == null)
            {
                Thread.Sleep(100);
            }
            enqueuedtask = queue.Enqueue(() => TaskFactory(3));

            //act
            queue.Stop();
            await enqueuedtask;

            //assert
            task.IsCompleted.Should().BeTrue();
            enqueuedtask.IsCompleted.Should().BeTrue();
        }


        [Test]
        public async Task Stop_Enqueue_Items_Should_Return_Canceled_Task_After_Stoping_Queue()
        {       
            //arrange  
            MonoThreadedQueue queue = new MonoThreadedQueue();
            queue.Stop();

            bool Done = false;

            TaskCanceledException error = null;
            try
            {
                await queue.Enqueue(() => { Done = true; });
            }
            catch (TaskCanceledException e)
            {
                error = e;
            }

            error.Should().NotBeNull();
            Done.Should().BeFalse();
        }
    }
}
