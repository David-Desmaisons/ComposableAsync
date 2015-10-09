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

Create an actor:

		//IFoo definition
		public interface IFoo
		{
		    Task Bar();
		}
		
		//ConcreteFoo definition
		public class ConcreteFoo : IFoo
		{
		    public Task<int> Bar()
		    {
		      //Implementation here...
		      
		      return Task.FromResult<int>(2);
		    }
		}
		...
		//Instanciate actor facory
		var fact = new ActorFactory();
		var fooActor = fact.Build<IFoo>( new ConcreteFoo());
		...
		//This will call ConcreteFoo Bar in its own thread
		var res = await fooActor.Bar();
		
###ActorFactory constructor parameter

Standard factory is  ActorFactory:

		var factory = new ActorFactory(priority:Priority.AboveNormal);

* priority (default to Normal) to choose the priority of the Thread priority where Actor methods will run. This value maps directly with Thread priority.

		public enum Priority
		{
		    Lowest,
		    BelowNormal,
		    Normal,
		    AboveNormal,
		    Highest
		}
		
EasyActor offers another factory SharedThreadActorFactory:

		var factory = new SharedThreadActorFactory(priority:Priority.AboveNormal);


All the actors created by this factory will live in the same thread. This option may be helpfull if you have to create a lot of actors which have to perform short lived methods and you do not want to create a thread for each one.

###SynchronizationContext


EasyActor also garantees that code running after awaited task will also run on the actor Thread (in a similar way than task on UI Thread):		
		
EasyActor also garantees that code running after awaited task will also run on the actor Thread (in a similar way than task on UI Thread):

		//ConcreteFoo definition
		public Task Bar()
		{
		      //Run on actor thread
		      .....
		      await AnotherTask();
		      
		      //This code also run on actor thread
		      ....
		}

[Classic Ping Pong Example here](https://github.com/David-Desmaisons/EasyActor/wiki/Ping-Pong-Example)

How it works
------------
Internally, EasyActor use [Castle Core DynamicProxy](https://github.com/castleproject/Core) to instaciate a proxy for the corresponding interface.
All calls to the interface methods are intercepted and then redirected to run on the actor Threads.

