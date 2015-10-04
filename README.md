EasyActor
=========

Motivation
--------------
[Actor pattern](https://en.wikipedia.org/wiki/Actor_model) aims at simplify the way you build concurrent application. One Key concept is the actor that leaves in its own thread and communicate with other actors via immutable message.

EasyActor implements a subset of this pattern in C# 5.0 leveraging .Net Tasks.
EasyActor´s main aim is to provide a lightweigth, fast, easy to use API to transform POCO in actors. Thus you can write concurrent code without having to wonder about manual locking and race conditions!

By working with interfaces and task, EasyActor is easy to integrate in existing projects and do not require that code consuming actors have to have special knowledge about it.

If you are looking for a complete Actor solution including remoting, resiliency and monitoring take a look at [akka.net](http://getakka.net/).


Features
--------

EasyActor provide a factory allowing the transformation of POCO in actor that are then seen trougth a interface.
This garantees that all calls to the actor interface will occur in a separated thread, sequencially, thus in thread safe maner and result are returned via tasks.

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
		    public Task Bar()
		    {
		      //Implementation here...
		    }
		}
		...
		//Instanciate actor facory
		var fact = new ActorFactory();
		var fooActor = fact.Build<IFoo>( new ConcreteFoo());
		...
		//This will call ConcreteFoo Bar in its own thread
		await fooActor.Bar();
		
		
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

How it works
------------
Internally, EasyActor use [Castle Core DynamicProxy](https://github.com/castleproject/Core) to instaciate a proxy for the corresponding interface.
All calls to the interface methods are intercepted and then redirected to run on the actor Threads.

