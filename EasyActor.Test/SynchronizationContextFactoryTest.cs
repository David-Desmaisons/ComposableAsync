using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Threading;
using NUnit.Framework;
using System.Windows.Threading;
using System.Windows;
using EasyActor.Test.WPFThreading;
using EasyActor.Test.TestInfra.DummyClass;

namespace EasyActor.Test
{
      [TestFixture]
    public class SynchronizationContextFactoryTest
    {
          private WPFThreadingHelper _UIMessageLoop;
          private SynchronizationContextFactory _Factory;

          [SetUp]
          public void TestUp()
          {
              _UIMessageLoop = new WPFThreadingHelper();
              _UIMessageLoop.Start().Wait();

              _Factory = _UIMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());
          }

          [TearDown]
          public void TearDown()
          {
              _UIMessageLoop.Stop();
          }

          [Test]
          public void Type_Should_Be_InCurrentContext()
          {
              _Factory.Type.Should().Be(ActorFactorType.InCurrentContext);
          }

          [Test]
          [ExpectedException(typeof(ArgumentNullException))]
          public void Creating_SynchronizationContextFactory_WithoutContext_ThrowException()
          {
                var sync = new SynchronizationContextFactory();
          }

          [Test]
          public async Task Proxy_Method_WithoutResult_Should_Be_Called()
          {
              //arrange
              var obj = new DummyClass();
              var actor = _Factory.Build<IDummyInterface2>(obj);

              //act
              await actor.DoAsync();

              //assert
              obj.Done.Should().BeTrue();
          }

          [Test]
          public async Task Proxy_Method_WithResult_Should_Called()
          {
              //arrange
              var obj = new DummyClass();
              var actor = _Factory.Build<IDummyInterface2>(obj);

              //act
              var res = await actor.ComputeAsync(5);

              //assert
              obj.Done.Should().BeTrue();
              res.Should().Be(5);
          }

          [Test]
          public async Task Proxy_Method_WithoutResult_Should_Dispatch_On_Captured_UI_Thread()
          {
              //arrange
              var obj = new DummyClass();
              var actor = _Factory.Build<IDummyInterface2>(obj);

              //act
              await actor.DoAsync();

              //assert
              obj.CallingThread.Should().Be(_UIMessageLoop.UIThread);
          }

          [Test]
          public async Task Proxy_Method_WithResult_Should_Dispatch_On_Captured_UI_Thread()
          {
              //arrange
              var obj = new DummyClass();
              var actor = _Factory.Build<IDummyInterface2>(obj);

              //act
              await actor.ComputeAsync(5);

              //assert
              obj.CallingThread.Should().Be(_UIMessageLoop.UIThread);
          }

          [Test]
          public async Task BuildAsync_Should_Call_Constructor_On_Actor_Thread()
          {
              var current = Thread.CurrentThread;
              DummyClass target = null;
              IDummyInterface2 intface = await _Factory.BuildAsync<IDummyInterface2>(() => { target = new DummyClass(); return target; });
              await intface.DoAsync();

              target.Done.Should().BeTrue();
              target.CallingConstructorThread.Should().NotBe(current);
              target.CallingConstructorThread.Should().Be(target.CallingThread);
          }
    }
}
