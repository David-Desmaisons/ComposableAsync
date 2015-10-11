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

namespace EasyActor.Test
{
      [TestFixture]
    public class SynchronizationContextFactoryTest
    {
          private WPFThreadingHelper _UIMessageLoop;
          private SynchronizationContextFactory _Target;

          [SetUp]
          public void TestUp()
          {
              TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

              _UIMessageLoop = new WPFThreadingHelper();
              _UIMessageLoop.Start().Wait();

              _Target = _UIMessageLoop.Dispatcher.Invoke(() => new SynchronizationContextFactory());
          }

          [TearDown]
          public void TearDown()
          {
              _UIMessageLoop.Stop();
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
              var obj = new Class();
              var actor = _Target.Build<Interface>(obj);

              //act
              await actor.DoAsync();

              //assert
              obj.Done.Should().BeTrue();
          }

          [Test]
          public async Task Proxy_Method_WithResult_Should_Called()
          {
              //arrange
              var obj = new Class();
              var actor = _Target.Build<Interface>(obj);

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
              var obj = new Class();
              var actor = _Target.Build<Interface>(obj);

              //act
              await actor.DoAsync();

              //assert
              obj.CallingThread.Should().Be(_UIMessageLoop.UIThread);
          }

          [Test]
          public async Task Proxy_Method_WithResult_Should_Dispatch_On_Captured_UI_Thread()
          {
              //arrange
              var obj = new Class();
              var actor = _Target.Build<Interface>(obj);

              //act
              await actor.ComputeAsync(5);

              //assert
              obj.CallingThread.Should().Be(_UIMessageLoop.UIThread);
          }

    }
}
