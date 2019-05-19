Composable Async
================

[![build](https://img.shields.io/appveyor/ci/David-Desmaisons/EasyActor.svg)](https://ci.appveyor.com/project/David-Desmaisons/EasyActor)
[![NuGet Badge](https://buildstats.info/nuget/EasyActor)](https://www.nuget.org/packages/EasyActor/)
[![MIT License](https://img.shields.io/github/license/David-Desmaisons/EasyActor.svg)](https://github.com/David-Desmaisons/EasyActor/blob/master/LICENSE)

Composable Async implements a subset of [Actor pattern](https://en.wikipedia.org/wiki/Actor_model) for the .Net platform.

Composable Async is a lightweight, fast, easy to use framework that transform POCO in actors. 

If you are looking for a complete Actor solution including remoting, resiliency and monitoring take a look at [akka.net](http://getakka.net/).


Motivation
--------------

* Simplify concurrent programing getting rid of manual lock hell.
* Use actor concept: actor leaves in their own thread and communicate with immutable message.
* Leverage C# 5.0 asynchronous API (Task, async , await): actors communicate with other component with Task
* Receive return from actor with Task<T>
* Transparent for consumer: EasyActor actors can be any C# interface returning Task.
* Fast: performance overhead should be minimum

Features
--------

Composable.Async provide a factory allowing the transformation of POCO in actor that are then seen through an interface.
Actor guarantees that all calls to the actor interface will occur in a separated thread, sequentially.

In order to work, The target interface should only expose methods returning Task or Task<T>.
If this not the case, an exception will be raised at runtime when calling a none compliant method.
Make also sure that all method parameters and return values are immutable to avoid concurrency problems.


Usage - Example
--------------

To create an actor:

1) Create an interface

```CSharp
	// IFoo definition
	public interface IFoo
	{
	    Task Bar();
	}
```

2) Implement the interface in a POCO	

```CSharp
	// ConcreteFoo definition
	public class ConcreteFoo : IFoo
	{
	    public Task<int> Bar()
	    {
		return Task.FromResult<int>(2);
	    }
	}
```

3) Use an ComposableAsync.Factory factory to create an actor from the POCO

```CSharp
	// Instantiate actor factory
	var builder = new ProxyFactoryBuilder();
  var factory = builder.GetActorFactory();
		
	// Instantiate an actor from a POCO
	IFoo fooActor = fact.Build<IFoo>( new ConcreteFoo());
```	
4) Use the actor: all method call will be executed on a dedicated thread

```CSharp
	//This will call ConcreteFoo Bar in its own thread
	var res = await fooActor.Bar();
```		
### Actor factories

ComposableAsync provide currently two Actor factories. An actor factory implements the IActorFactory

```CSharp
	// Factory to create actor from POCO
	public interface IActorFactory
	{
    /// Returns the type of the factory.
    ActorFactorType Type { get; }
            
    // Build an actor from a POCO
    // T should an interface through which the actor will be seen
    T Build<T>(T concrete) where T : class;
            
    ///  Build asynchronously an actor from a POCO
    ///  using the actor thread to call the function creating the POCO.
    ///  T should an interface through which the actor will be seen
    Task<T> BuildAsync<T>(Func<T> concrete) where T : class;
	}
```

FactoryBuilder that implements IFactoryBuilder give access to all the different kind of factories.

#### ActorFactory 

ActorFactory is the standard factory for EasyFactor. Each actor created by this factory lives each in its own separated thread:

```CSharp
	var factory = new ActorFactory();
```

ActorFactory constructor accepts an optional argument of type Action<Thread> that can be used to initialize Threads created by the factory (for example setting its priority or STA property).

```CSharp
	var factory = new ActorFactory(t => t.Priority= Priority.AboveNormal);
	// or
	var factory = FactoryBuilder.GetFactory(false, t => t.Priority= Priority.AboveNormal);
```

####  SharedThreadActorFactory 

This factory creates actors that will share the same thread:

```CSharp
	var factory = new SharedThreadActorFactory(t => t.Priority= Priority.Normal);
	// or
	var factory = FactoryBuilder.GetFactory(true, t => t.Priority= Priority.Normal);
```

This option may be helpful if you have to create a lot of actors which have to perform short lived methods and you do not want to create a thread for each one. Same as ActorFactory, an Action<Thread> can be furnished to initialize the thread.

####  TaskPoolActorFactory 

This factory creates actors that will be called on thread pool tasks using [ConcurrentExclusiveSchedulerPair](https://msdn.microsoft.com/en-us/library/system.threading.tasks.concurrentexclusiveschedulerpair(v=vs.110).aspx). Note that in this case, whereas none concurrency of method calls is guaranteed, actor methods may run on different threads other time. Usage:

```CSharp
	var factory = new TaskPoolActorFactory();
	// or
	var factory = FactoryBuilder.GetTaskBasedFactory(); 
```

This option may be helpful if you want no concurrency for given actors but don't want to allocate a dedicated thread for them.

####  SynchronizationContextFactory 

SynchronizationContextFactory factory instantiate actors that will use the current synchronization context as the threading context. This means that if you instantiate a SynchronizationContextFactory in an UI thread (WPF or Windows Form), all the calls of the corresponding actors will happens in the same UI thread.

```CSharp
	var factory = new SynchronizationContextFactory();
	// or 
	var factory = FactoryBuilder.GetInContextFactory(); 	
```

Please note that if there is no SynchronizationContext associated with the thread calling SynchronizationContextFactory an exception will be raised.

### SynchronizationContext

EasyActor also guarantees that code running after awaited task will also run on the actor Thread (in a similar way than task on UI Thread). This is done by setting the actor´s thread SynchronizationContext. For example: 		

```CSharp
	// ConcreteFoo definition
	public Task Bar()
	{
	      // Run on actor thread
	      .....
	      await AnotherTask();
	      
	      // This code also run on actor thread
	      ....
	}
```

### Life Cycle

Actor created by ActorFactory have a life cycle that can be controlled independently one from each other using the IActorCompleteLifeCycle interface that they implement.
Actor created by TaskPoolActorFactory have a life cycle that can be controlled independently one from each other using the IActorLifeCycle interface that they implement.
Actor created by SharedThreadActorFactory have the same life cycle that can be controlled using the IActorCompleteLifeCycle implemented on SharedThreadActorFactory.
Note that it is not possible to control life cycle of actors created by SynchronizationContextFactory.

IActorCompleteLifeCycle is defined as below:

```CSharp
	public interface IActorCompleteLifeCycle : IActorLifeCycle
	{
	      Task Abort();
	}
```

IActorLifeCycle has two methods:

```CSharp
	public interface IActorLifeCycle
	{
	      Task Stop();
	}
```

- Abort:
	- Cancel all the queued tasks: only currently executing task will be run to completion if any
	- Call IAsyncDisposable.DisposeAsync on the proxified actor(s) if implemented
	- Prevent further call to actor to be executed: actor will only return cancelled Tasks
	- Terminate actor thread 
- Stop:
	- Prevent further call to actor to be executed: actor will only return cancelled Tasks
	- Wait for completion of all the queued tasks
	- Call IAsyncDisposable.DisposeAsync on the proxified actor(s) if implemented
	- Terminate actor thread 	
	
[IAsyncDisposable](https://github.com/dotnet/roslyn/issues/114) is the asynchronous version of IDisposable:

```CSharp
	public interface IAsyncDisposable : IDisposable
	{
		/// <summary>
		///  Performs asynchronously application-defined tasks associated with freeing,
		///  releasing, or resetting unmanaged resources.
		/// </summary>
		Task DisposeAsync();
	}
```	

[Classic Ping Pong Example here](https://github.com/David-Desmaisons/EasyActor/wiki/Ping-Pong-Example)

Comparaison with other frameworks
---------------------------------

- [Akka.net](http://getakka.net/) is a very complete actor solution: use it if you need a solution that includes remoting, resiliency and monitoring. Use EasyActor if you need small-footprint, fast, in process actor model. 
- [n-act](https://code.google.com/p/n-act/) is similar to EasyActor, providing the same philosophy and similar API. That said, EasyActor provides more lifecycle options (via factories and IActorLifeCycle) and is faster than N-act (8x faster on the ping-pong example).

Code coverage
-------------

98.94%

How it works
------------
Internally, EasyActor use [Castle Core DynamicProxy](https://github.com/castleproject/Core) to instantiate a proxy for the corresponding interface.
All calls to the interface methods are intercepted and then redirected to run on the actor Threads.

Nuget
-----

[Go nuget package](https://www.nuget.org/packages/EasyActor/)

