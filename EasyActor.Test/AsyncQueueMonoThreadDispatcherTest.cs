using System;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using EasyActor.Queue;

namespace EasyActor.Test
{
    [TestFixture]
    public class AsyncQueueMonoThreadDispatcherTest
    {
        private Thread _RunningThread;


        private Task TaskFactory(int sleep=1)
        {
            _RunningThread = Thread.CurrentThread;
            Thread.Sleep(sleep * 1000);
            return Task.FromResult<object>(null);
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

          [Test]
        public async Task Enqueue_Should_Run_OnSeparatedThread()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
        public async Task Enqueue_Should_DispatchException_With_Result()
        {
            //arrange
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
        public async Task Enqueue_Should_DispatchException_OnAction()
        {
            Thread current = Thread.CurrentThread;
            //arrange
            using (var target = new AsyncQueueMonoThreadDispatcher())
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
    }
}
