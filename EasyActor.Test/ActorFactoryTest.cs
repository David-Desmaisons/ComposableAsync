﻿using System;
using System.Threading.Tasks;
using System.Threading;
using EasyActor.Factories;
using EasyActor.Options;
using FluentAssertions;
using EasyActor.Test.TestInfra.DummyClass;
using Xunit;
using Concurrent.Tasks;

namespace EasyActor.Test
{  
    public class ActorFactoryTest : IAsyncLifetime
    {
        private readonly ActorFactory _Factory;
        private IDummyInterface2 _Actor1;
        private IDummyInterface2 _Actor2;
        private IDummyInterface1 _Actor3;
        private IDummyInterface4 _Actor4;

        public ActorFactoryTest()
        {
            _Factory = new ActorFactory();
        }

        public Task InitializeAsync() => TaskBuilder.Completed;

        public async Task DisposeAsync() 
        {
            await Wait(_Actor1);
            await Wait(_Actor2);
            await Wait(_Actor3);
            await Wait(_Actor4?.DisposeAsync());
        }

        private static Task Wait<T>(T actor) where T: class
        {
            return (actor as IAsyncDisposable)?.DisposeAsync() ?? TaskBuilder.Completed;
        }

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
            _Actor1 = _Factory.Build<IDummyInterface2>(target);
            await _Actor1.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public async Task Method_Should_Always_Run_On_Same_Thread()
        {
            var target = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target);
            await _Actor1.DoAsync();

            var thread = target.CallingThread;

            await _Actor1.DoAsync();

            target.CallingThread.Should().Be(thread);
        }

        [Fact]
        public async Task Each_Actor_Should_Run_On_Separated_Thread_When_Shared_Thread_Is_False()
        {
            //arrange
            var target1 = new DummyClass();
            var target2 = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target1);
            _Actor2 = _Factory.Build<IDummyInterface2>(target2);

            //act
            await _Actor1.DoAsync();
            await _Actor2.DoAsync();

            //assert
            target1.CallingThread.Should().NotBe(target2.CallingThread);
        }

        [Fact]
        public async Task Method_Should_Run_On_Same_Thread_After_Await()
        {
            var target = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target);

            var res = await _Actor1.DoAnRedoAsync();

            res.Item1.Should().Be(res.Item2);
        }

        [Fact]
        public void Build_Should_CreateSameInterface_ForSamePOCO()
        {
            var target = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target);
            var intface2 = _Factory.Build<IDummyInterface2>(target);

            _Actor1.Should().BeSameAs(intface2);
        }

        [Fact]
        public async Task Build_Should_Throw_Exception_IsSamePOCO_HasBeenUsedWithOtherFactory()
        {
            var target = new DummyClass();
            var sharedFactory = new SharedThreadActorFactory();
            _Actor1 = sharedFactory.Build<IDummyInterface2>(target);

            Action Do = () => _Factory.Build<IDummyInterface2>(target);

            Do.Should().Throw<ArgumentException>().And.Message.Should().Contain("Shared");

            await sharedFactory.DisposeAsync();
        }

        [Fact]
        public async Task Task_returned_By_Method_Should_Be_Awaited()
        {
            var target = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target);
            await _Actor1.SlowDoAsync();

            target.Done.Should().BeTrue();
        }

        [Fact]
        public async Task Method_With_Task_T_Should_Run_On_Separated_Tread()
        {
            var current = Thread.CurrentThread;
            var target = new DummyClass();
            _Actor1 = _Factory.Build<IDummyInterface2>(target);
            var result = await _Actor1.ComputeAsync(25);

            result.Should().Be(25);
            target.Done.Should().BeTrue();
            target.CallingThread.Should().NotBeNull();
            target.CallingThread.Should().NotBe(current);
        }

        [Fact]
        public void Method_returning_void_Task_Should_Not_Throw_Exception()
        {
            _Actor1 = _Factory.Build<IDummyInterface2>(new DummyClass());
            Action Do = () => _Actor1.Do();
            Do.Should().NotThrow();
        }

        [Fact]
        public async Task BuildAsync_Should_Call_Constructor_On_Actor_Thread()
        {
            var current = Thread.CurrentThread;
            DummyClass target = null;
            _Actor1 = await _Factory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
            await _Actor1.DoAsync();

            target.Done.Should().BeTrue();
            target.CallingConstructorThread.Should().NotBe(current);
            target.CallingConstructorThread.Should().Be(target.CallingThread);
        }

        [Fact]
        public void Actor_Should_Implement_IFiberProvider()
        {
            _Actor1 = _Factory.Build<IDummyInterface2>(new DummyClass());
            _Actor1.Should().BeAssignableTo<IFiberProvider>();
        }

        [Fact]
        public void Actor_Should_Returns_Fiber()
        {
            _Actor1 = _Factory.Build<IDummyInterface2>(new DummyClass());
            var fp = _Actor1 as IFiberProvider;

            var fiber = fp.Fiber;

            fiber.Should().NotBeNull();
        }

        [Fact]
        public async Task Actor_IAsyncDisposable_DisposeAsync_Should_Call_Proxified_Class_On_IAsyncDisposable_Case_2() 
        {
            //arrange
            var dispclass = new DisposableClass();
            _Actor4 = _Factory.Build<IDummyInterface4>(dispclass);

            await _Actor4.DisposeAsync();
            //assert
            dispclass.IsDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task Actor_IAsyncDisposable_DisposeAsync_Should_Call_Proxified_Class_On_ActorThread_Case_2() 
        {
            //arrange
            var testThread = Thread.CurrentThread;

            var dispclass = new DisposableClass();
            _Actor4 = _Factory.Build<IDummyInterface4>(dispclass);

            await _Actor4.DoAsync();

            var thread = dispclass.LastCallingThread;
            //act

            await _Actor4.DisposeAsync();
            //assert
            var disposableThread = dispclass.LastCallingThread;

            disposableThread.Should().NotBe(testThread);
            disposableThread.Should().Be(thread);
        }
    }
}
