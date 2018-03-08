using System.Threading.Tasks;

 
using EasyActor.TaskHelper;
using FluentAssertions;
using System.Threading;
using EasyActor.Factories;
using EasyActor.Fiber;
using EasyActor.Test.TestInfra.DummyClass;
using EasyActor.Test.TestInfra.WPFThreading;
using Xunit;

namespace EasyActor.Test
{
     
    public class ActorContextTest
    {
        private IDummyInterface2 _Interface;
        private DummyClass _Proxified;
        private ActorContext _ActorContext;

        public ActorContextTest()
        {
            _ActorContext = new ActorContext();
            var factory = new ActorFactory();
            _Proxified = new DummyClass();
            _Interface = factory.Build<IDummyInterface2>(_Proxified);
        }

        [Fact]
        public void TaskFactory_Should_Return_A_Valid_TaskFactory_With_A_None_Proxied_Object()
        {
            //arrange
            object random = new object();

            //act
            var res = _ActorContext.GetTaskFactory(random);

            //assert
            res.Should().NotBeNull();
        }

        [Fact]
        public void TaskFactory_Should_Return_A_Valid_TaskFactory_With_A_Proxied_Object()
        {
            //act
            var res = _ActorContext.GetTaskFactory(_Proxified);

            //assert
            res.Should().NotBeNull();
        }

        [Fact]
        public async Task TaskFactory_Should_Return_TaskFactory_Compatible_With_Proxy_Thread_ActorFactory_Context()
        {
            //arrange
            await _Interface.DoAsync();

            //act
            var res = _ActorContext.GetTaskFactory(_Proxified);
            Thread tfthread = await res.StartNew(()=> Thread.CurrentThread);

            //assert
            tfthread.Should().NotBeNull();
            tfthread.Should().Be(_Proxified.CallingThread);
        }

      
        [Fact]
        public async Task TaskFactory_Should_Return_TaskFactory_Compatible_With_Proxy_Thread_SynchronizationContextFactory_Context()
        {
            //arrange
            var UIMessageLoop = new WpfThreadingHelper();
            UIMessageLoop.Start().Wait();

            var scf = UIMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());

            _Proxified = new DummyClass();
            _Interface = scf.Build<IDummyInterface2>(_Proxified);

            //act
            var res = _ActorContext.GetTaskFactory(_Proxified);
            Thread tfthread = await res.StartNew(() => Thread.CurrentThread);

            //assert
            tfthread.Should().Be(UIMessageLoop.UiThread);

            UIMessageLoop.Stop();
        }


        [Fact]
        public void GetTaskScheduler_Should_Return_TaskScheduler_Default_With_A_None_Proxied_Object()
        {
            //arrange
            object random = new object();

            //act
            var res = _ActorContext.GetTaskScheduler(random);

            //assert
            res.Should().NotBeNull();
            res.Should().Be(TaskScheduler.Default);
        }

        [Fact]
        public async Task GetTaskScheduler_Should_Return_SynchronizationContext_Compatible_With_Proxy_Thread_ActorFactory_Context()
        {
            //arrange
            await _Interface.DoAsync();

            //act
            var res = _ActorContext.GetTaskScheduler(_Proxified);

            //assert
            res.Should().NotBeNull();
            res.Should().BeOfType<SynchronizationContextTaskScheduler>();
            var ressync = res as SynchronizationContextTaskScheduler;
            ressync.SynchronizationContext.Should().BeAssignableTo<MonoThreadedFiberSynchronizationContext>();
        }

        [Fact]
        public void GetTaskScheduler_Should_Return_SynchronizationContext_Compatible_With_Proxy_Thread_SynchronizationContextFactory_Context()
        {
            //arrange
            var UIMessageLoop = new WpfThreadingHelper();
            UIMessageLoop.Start().Wait();

            var scf = UIMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());

            _Proxified = new DummyClass();
            _Interface = scf.Build<IDummyInterface2>(_Proxified);

            //act
            var res = _ActorContext.GetTaskScheduler(_Proxified);
            var uisc = UIMessageLoop.Dispatcher.Invoke(()=>  SynchronizationContext.Current);
         

            //assert
            res.Should().NotBeNull();
            res.Should().BeOfType<SynchronizationContextTaskScheduler>();
            var ressync = res as SynchronizationContextTaskScheduler;
            ressync.SynchronizationContext.Should().BeOfType(uisc.GetType());

            UIMessageLoop.Stop();
        }
    }
}
