using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using System.Threading;

namespace EasyActor.Test
{
    [TestFixture]
    public class LoadBalancerFactoryTest
    {
        private LoadBalancerFactory _Factory;
        private Func<Class> _Fact = () => new Class();


        [SetUp]
        public void TestUp()
        {
            _Factory = new LoadBalancerFactory(BalancingOption.MinizeObjectCreation);
            Class.ResetCount();
        }

        [Test]
        public void ShouldCreateBasicLB()
        {
            var target = _Factory.Build<Interface>(_Fact,2);

            target.Should().NotBeNull();

        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ShouldNotWorkWithNegativeOrNullParalelism()
        {
            var target = _Factory.Build<Interface>(_Fact, 0);
        }

     
        [Test]
        public async Task ShouldDelegateToPoco()
        {
            var target = _Factory.Build<Interface>(_Fact, 2);
            var res = await target.ComputeAsync(23);

            res.Should().Be(23);
            Class.Count.Should().Be(1);
        }


        [Test]
        public async Task MinizeObjectCreation_Should_Create_Poco_IfNeeded_Simultaneous()
        {
            var target = _Factory.Build<Interface>(_Fact, 2);
            await Task.WhenAll(target.SlowDoAsync(), target.SlowDoAsync());

            Class.Count.Should().Be(2);
        }

        [Test]
        public async Task MinizeObjectCreation_Should_Create_Poco_IfNeeded()
        {
            var target = _Factory.Build<Interface>(_Fact, 2);
            var t1= target.SlowDoAsync();
            Thread.Sleep(200);
            var t2 = target.SlowDoAsync();
            await Task.WhenAll(t1, t2);

            Class.Count.Should().Be(2);
        }

        [Test]
        public async Task MinizeObjectCreation_Should_Not_Create_More_Poco_Than_LimitedParallelism()
        {
            var target = _Factory.Build<Interface>(_Fact, 2);
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

            Class.Count.Should().Be(2);
        }



        [Test]
        public async Task MinizeObjectCreation_ShouldNotInstanciateVariousPocoIfActorIsWithoutActivity()
        {
            var target = _Factory.Build<Interface>(_Fact, 2);
            await target.SlowDoAsync();
            Thread.Sleep(10);
            await target.SlowDoAsync();

            Class.Count.Should().Be(1);
        }

        [Test]
        public async Task PreferParralelism_ShouldInstanciateVariousPocoEvenIfActorIsWithoutActivity()
        {
            _Factory = new LoadBalancerFactory(BalancingOption.PreferParralelism);
            var target = _Factory.Build<Interface>(_Fact, 2);
            await target.SlowDoAsync();
            Thread.Sleep(10);
            await target.SlowDoAsync();

            Class.Count.Should().Be(2);
        }

        [Test]
        public void Stress_Paralellism()
        {

            var target = _Factory.Build<Interface>(_Fact, 5);

            ThreadStart ts = () => Start(target);

            var treads = Enumerable.Range(0,100).Select(i => new Thread(ts)).ToList();
            treads.ForEach(t => t.Start());
            treads.ForEach(t => t.Join());

            Class.Count.Should().Be(5);
        }

        private void Start(Interface dummy)
        {
            Thread.Sleep(400);
            dummy.SlowDoAsync().Wait();

        }

    }
}
