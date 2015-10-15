EasyActor
=========

EasyActor implements a subset of [Actor pattern](https://en.wikipedia.org/wiki/Actor_model) for the .Net platform.

EasyActor´s main goal is to provide a lightweigth, fast, easy to use framework to transform POCO in actors. 

If you are looking for a complete Actor solution including remoting, resiliency and monitoring take a look at [akka.net](http://getakka.net/).


Motivation
--------------

* Simplify concurent programing getting rid of manual lock hell.
* Use actor concept: actor leaves in their own thread and comunicate with immutable message.
* Leverage C# 5.0 ansychroneous API (Task, async , await): actors comunicate with other component with Task
* Receive return from actor with Task<T>
* Transparent for consumer: EasyActor actors can be any C# interface returning Task.
* Fast: performance overhead should be minimum

Features
--------

EasyActor provide a factory allowing the transformation of POCO in actor that are then seen trougth an interface.
Actor guarantees that all calls to the actor interface will occur in a separated thread, sequencially.

In order to work, The target interface should only expose methods returning Task or Task<T>.
If this not the case, an exception will be raised at runtime when calling a none compliant method.
Make also sure that all method parameters and return values are immutable to avoid concurrency problems.


Usage - Example
--------------

To create an actor:

1) Create an interface

	//IFoo definition
	public interface IFoo
	{
	    Task Bar();
	}
	
2) Implement the interface in a POCO	

	//ConcreteFoo definition
	public class ConcreteFoo : IFoo
	{
	    public Task<int> Bar()
	    {
	      return Task.FromResult<int>(2);
	    }
	}

3) Use an EasyActor fatory to create an actor from the POCO

	//Instanciate actor facory
	var fact = new ActorFactory();
		
	//Instanciate an actor from a POCO
	IFoo fooActor = fact.Build<IFoo>( new ConcreteFoo());
	
4) Use the actor: all method call will be executed on a dedicated thread

	//This will call ConcreteFoo Bar in its own thread
	var res = await fooActor.Bar();
		
### Actor factories

EasyActor provide currently two Actor factories. An actory factory implements the IActorFactory

	//Factory to create actor from POCO
	public interface IActorFactory
	{
            //Build an actor from a POCO
            //T should an interface througth which the actor will be seen
            T Build<T>(T concrete) where T : class;
	}

#### ActorFactory 

ActorFactory is the standard factory for EasyFactor. Each actor created by this factory lives each in its own separated thread:

	var factory = new ActorFactory(priority:Priority.AboveNormal);

Priority argument (default to Normal) define the priority of the threads where Actor methods will run. This value maps directly with Thread priority.

	public enum Priority
	{
	    Lowest,
	    BelowNormal,
	    Normal,
	    AboveNormal,
	    Highest
	}
	
####  SharedThreadActorFactory 

This factory creates actors that will share the same thread:

	var factory = new SharedThreadActorFactory(priority:Priority.AboveNormal);


This option may be helpfull if you have to create a lot of actors which have to perform short lived methods and you do not want to create a thread for each one. Same as ActorFactory priority argument (default to Normal) define the priority of the thread where Actor methods will run.

####  SynchronizationContextFactory 

SynchronizationContextFactory factory instanciate actors that will use the current synchronization context as the threading context. This means that if you instanciate a SynchronizationContextFactory in an UI thread (WPF or Windows Form), all the calls of the corresponding actors will happens in the same UI thread.

	var factory = new SynchronizationContextFactory();

Please note that if there is no SynchronizationContext associated with the thread calling SynchronizationContextFactory an exception will be raised.

### SynchronizationContext

EasyActor also guarantees that code running after awaited task will also run on the actor Thread (in a similar way than task on UI Thread). This is done by setting the actor´s thread SynchronizationContext. For example: 		

	//ConcreteFoo definition
	public Task Bar()
	{
	      //Run on actor thread
	      .....
	      await AnotherTask();
	      
	      //This code also run on actor thread
	      ....
	}

### Life Cycle

Actor created by ActorFactory have a life cycle that can be controlled independently one from each other using the IActorLifeCycle interface that they implement.
Actor created by SharedThreadActorFactory have the same life cycle that can be controlled using the IActorLifeCycle implemented on SharedThreadActorFactory.

IActorLifeCycle has two methods:

	public interface IActorLifeCycle
	{
	      Task Abort();

	      Task Stop();
	}
	
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
	
[IAsyncDisposable](https://github.com/dotnet/roslyn/issues/114) is the asynchroneous version of IDisposable:

	public interface IAsyncDisposable : IDisposable
	{
		/// <summary>
		///  Performs asyncronesouly application-defined tasks associated with freeing,
		///  releasing, or resetting unmanaged resources.
		/// </summary>
		Task DisposeAsync();
	}
	

[Classic Ping Pong Example here](https://github.com/David-Desmaisons/EasyActor/wiki/Ping-Pong-Example)

Comparaison with other frameworks
---------------------------------

- [Akka.net](http://getakka.net/) is a very complete actor solution: use it if you need a solution that includes remoting, resiliency and monitoring. Use EasyActor if you need small-footprint, fast, in process actor model. 
- [n-act](https://code.google.com/p/n-act/) is similar to EasyActor, providing the same philosophy and similar API. That said, EasyActor provides more lifecycle options (via factories and IActorLifeCycle) and is faster than N-act (8x faster on the pingpong example).

Code coverage
-------------

100%

How it works
------------
Internally, EasyActor use [Castle Core DynamicProxy](https://github.com/castleproject/Core) to instaciate a proxy for the corresponding interface.
All calls to the interface methods are intercepted and then redirected to run on the actor Threads.

