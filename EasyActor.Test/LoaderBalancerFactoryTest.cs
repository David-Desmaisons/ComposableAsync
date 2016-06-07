using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
 
using System.Threading;
using EasyActor.Test.TestInfra.DummyClass;
using Xunit;

namespace EasyActor.Test
{
     
    public class LoadBalancerFactoryTest
    {
        private LoadBalancerFactory _Factory;
        private Func<DummyClass> _Fact = () => new DummyClass();

        public LoadBalancerFactoryTest()
        {
            _Factory = new LoadBalancerFactory(BalancingOption.MinizeObjectCreation);
            DummyClass.ResetCount();
        }

        [Fact]
        public void ShouldCreateBasicLB()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact,2);

            target.Should().NotBeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShouldNotWorkWithNegativeOrNullParalelism(int parrallelism)
        {
            Action Do = () => _Factory.Build<IDummyInterface2>(_Fact, parrallelism);
            Do.ShouldThrow<ArgumentOutOfRangeException>();
        }
     
        [Fact]
        public async Task ShouldDelegateToPoco()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            var res = await target.ComputeAsync(23);

            res.Should().Be(23);
            DummyClass.Count.Should().Be(1);
        }

        [Fact]
        public async Task MinizeObjectCreation_Should_Create_Poco_IfNeeded_Simultaneous() 
        {
            var count = DummyClass.Count;
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            await Task.WhenAll(target.SlowDoAsync(), target.SlowDoAsync());

            var delta = DummyClass.Count -count;
            delta.Should().Be(2);
        }

        [Fact]
        public async Task MinizeObjectCreation_Should_Create_Poco_IfNeeded() 
        {
            var count = DummyClass.Count;
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            var t1= target.SlowDoAsync();
            Thread.Sleep(200);
            var t2 = target.SlowDoAsync();
            await Task.WhenAll(t1, t2);

            var delta = DummyClass.Count - count;

            delta.Should().Be(2);
        }

        [Fact]
        public async Task MinizeObjectCreation_Should_Not_Create_More_Poco_Than_LimitedParallelism()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            var t1 = target.SlowDoAsync();
            Thread.Sleep(200);
            var t2 = target.SlowDoAsync();
            Thread.Sleep(200);
            var t3 = target.SlowDoAsync();
            Thread.Sleep(200);
            var t4 = target.SlowDoAsync();
            Thread.Sleep(200);
            var t5 = target.SlowDoAsync();
            await Task.WhenAll(t1, t2,t3,t4,t5);

            DummyClass.Count.Should().Be(2);
        }

        [Fact]
        public async Task MinizeObjectCreation_ShouldNotInstanciateVariousPocoIfActorIsWithoutActivity()
        {
            var count = DummyClass.Count;
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            await target.SlowDoAsync();
            Thread.Sleep(10);
            await target.SlowDoAsync();
            var delta = DummyClass.Count - count;
            delta.Should().Be(1);
        }

        [Fact]
        public async Task PreferParralelism_ShouldInstanciateVariousPocoEvenIfActorIsWithoutActivity()
        {
            _Factory = new LoadBalancerFactory(BalancingOption.PreferParralelism);
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            await target.SlowDoAsync();
            Thread.Sleep(10);
            await target.SlowDoAsync();

            DummyClass.Count.Should().Be(2);
        }

        [Fact]
        public void LoadBalancer_Should_Implement_IActorLifeCycle()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            target.Should().BeAssignableTo<IActorCompleteLifeCycle>();
        }

        [Fact]
        public async Task LoadBalancer_IActorLifeCycle_Abort_Should_NotThrowException()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2) as IActorCompleteLifeCycle;
            await target.Abort();
        }

        [Fact]
        public async Task LoadBalancer_IActorLifeCycle_Abort_Should_CancelTask()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            var lf = target as IActorCompleteLifeCycle;
            await lf.Abort();

            Action Do = () => target.SlowDoAsync().Wait();

            Do.ShouldThrow<TaskCanceledException>();
        }

        [Fact]
        public async Task LoadBalancer_IActorLifeCycle_Abort_Should_CancelTask_Not_Run()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);

            var t1 = target.SlowDoAsync();
            var t2 = target.SlowDoAsync();
            var t3 = target.SlowDoAsync();
            var t4 = target.SlowDoAsync();

            var lf = target as IActorCompleteLifeCycle;
            await lf.Abort();

            try
            {
                await Task.WhenAll(t1,t2,t3,t4);
            }
            catch
            {
            }

            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeTrue();

            t3.IsCanceled.Should().BeTrue();
            t4.IsCanceled.Should().BeTrue();
        }

        [Fact]
        public async Task LoadBalancer_IActorLifeCycle_Stop_Should_NotThrowException()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2) as IActorCompleteLifeCycle;
            await target.Stop();
        }

        [Fact]
        public async Task LoadBalancer_IActorLifeCycle_Stop_Should_CancelTask()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            var lf = target as IActorCompleteLifeCycle;
            await lf.Stop();

            Action Do = () => target.SlowDoAsync().Wait();

            Do.ShouldThrow<TaskCanceledException>();
        }

        [Fact]
        public void Stress_Paralellism()
        {
            DummyClass.ResetObjects();
            var target = _Factory.Build<IDummyInterface2>(_Fact, 5);

            ThreadStart ts = () => Start(target);

            var treads = Enumerable.Range(0,100).Select(i => new Thread(ts)).ToList();
            treads.ForEach(t => t.Start());
            treads.ForEach(t => t.Join());

            DummyClass.Count.Should().Be(5);

            int j = 0;
            foreach(var ob in DummyClass.GetObjects())
            {
                Console.WriteLine("actor {0}: {1}",j++,ob.SlowDoAsyncCount);
            }

            var count = DummyClass.GetObjects().Select(o => o.SlowDoAsyncCount).Max();
            count.Should().BeLessOrEqualTo(21);
            count.Should().BeGreaterOrEqualTo(19);
        }

        private void Start(IDummyInterface2 dummy)
        {
            Thread.Sleep(400);
            dummy.SlowDoAsync().Wait();
        }
    }
}
