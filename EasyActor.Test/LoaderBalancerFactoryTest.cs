using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using System.Threading;
using EasyActor.Factories;
using EasyActor.Test.TestInfra.DummyClass;
using Xunit;
using Xunit.Abstractions;

namespace EasyActor.Test
{
    public class LoadBalancerFactoryTest
    {
        private LoadBalancerFactory _Factory;
        private readonly Func<DummyClass> _Fact;
        private readonly DummyClassFactory _DummyFactory;
        private readonly ITestOutputHelper _Output;

        public LoadBalancerFactoryTest(ITestOutputHelper output)
        {
            _Output = output;
            _DummyFactory = new DummyClassFactory();
            _Fact = _DummyFactory.Factory;
            _Factory = new LoadBalancerFactory(BalancingOption.MinizeObjectCreation);
        }

        [Fact]
        public async Task ShouldCreateBasicLB()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);

            target.Should().NotBeNull();

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
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
            _DummyFactory.Created.Should().Be(1);

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
        }

        [Fact]
        public async Task MinizeObjectCreation_Should_Create_Poco_IfNeeded_Simultaneous()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            await Task.WhenAll(target.SlowDoAsync(), target.SlowDoAsync());

            _DummyFactory.Created.Should().Be(2);

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
        }

        [Fact]
        public async Task MinizeObjectCreation_Should_Create_Poco_IfNeeded()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            var t1 = target.SlowDoAsync();
            Thread.Sleep(200);
            var t2 = target.SlowDoAsync();
            await Task.WhenAll(t1, t2);

            _DummyFactory.Created.Should().Be(2);

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
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
            await Task.WhenAll(t1, t2, t3, t4, t5);

            _DummyFactory.Created.Should().Be(2);

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
        }

        [Fact]
        public async Task MinizeObjectCreation_ShouldNotInstanciateVariousPocoIfActorIsWithoutActivity()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            await target.SlowDoAsync();
            Thread.Sleep(10);
            await target.SlowDoAsync();

            _DummyFactory.Created.Should().Be(1);

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
        }

        [Fact]
        public async Task PreferParralelism_ShouldInstanciateVariousPocoEvenIfActorIsWithoutActivity()
        {
            _Factory = new LoadBalancerFactory(BalancingOption.PreferParralelism);
            var target = _Factory.Build<IDummyInterface2>(_Fact, 2);
            await target.SlowDoAsync();
            Thread.Sleep(10);
            await target.SlowDoAsync();

            _DummyFactory.Created.Should().Be(2);

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
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
                await Task.WhenAll(t1, t2, t3, t4);
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

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
        }

        [Fact]
        public async Task Stress_Paralellism()
        {
            var target = _Factory.Build<IDummyInterface2>(_Fact, 5);

            ThreadStart ts = () => Start(target);

            var treads = Enumerable.Range(0, 100).Select(i => new Thread(ts) {Name = $"ThreadTest-{i}"}).ToList();
            var watch = Stopwatch.StartNew();
            treads.ForEach(t => t.Start());
            treads.ForEach(t => t.Join());
            watch.Stop();
            _Output.WriteLine($"Time spend: {watch.ElapsedMilliseconds} ms");

            var targetTime = 400 + (100 / 5) * 1000;
            _Output.WriteLine($"Overhead: {watch.ElapsedMilliseconds - targetTime} ms");

            _DummyFactory.Created.Should().Be(5);

            var j = 0;
            foreach (var ob in _DummyFactory.DummyClasses)
            {
                _Output.WriteLine($"actor {j++}: {ob.SlowDoAsyncCount}");
            }

            var count = _DummyFactory.DummyClasses.Select(o => o.SlowDoAsyncCount).Max();
            count.Should().BeOneOf(19, 20, 21);

            var stop = target as IActorCompleteLifeCycle;
            await stop.Abort();
        }

        private void Start(IDummyInterface2 dummy)
        {
            Thread.Sleep(400);
            dummy.SlowDoAsync().Wait();
        }
    }
}
