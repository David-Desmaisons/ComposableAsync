Composable Async
================

[![build](https://img.shields.io/appveyor/ci/David-Desmaisons/EasyActor.svg)](https://ci.appveyor.com/project/David-Desmaisons/EasyActor)
[![NuGet Badge](https://buildstats.info/nuget/EasyActor)](https://www.nuget.org/packages/EasyActor/)
[![MIT License](https://img.shields.io/github/license/David-Desmaisons/EasyActor.svg)](https://github.com/David-Desmaisons/EasyActor/blob/master/LICENSE)

Composable Async is a library which aims at simplifying asynchronous programming in C# by providing:

- An abstraction:  `IDispatcher` that take an Action or Function and replay it in a different context.

```CSharp
public interface IDispatcher
{
	///...

	/// <summary>
	/// Enqueue the action and return a task corresponding to
	/// the completion of the action
	/// </summary>
	Task Enqueue(Action action);

	/// <summary>
	/// Enqueue the function and return a task corresponding to
	/// the result of the function
	/// </summary>
	Task<T> Enqueue<T>(Func<T> action);

}
```

- Some pre-built `IDispatchers` such as:
	- Rate limiting
	- Fiber implementation to build object that uses [Actor pattern](https://en.wikipedia.org/wiki/Actor_model).
	- Circuit-breaker (incoming)

- Extension methods to compose and await `IDispatcher`

- A factory to add `IDispatcher` behaviors to [plain old CLR Objects](https://www.wikipedia.org//wiki/Plain_old_CLR_object)


This library notably can be used as a lightweight, fast, easy to use framework that transform POCO in actors. 

If you are looking for a complete Actor solution including remoting, resiliency and monitoring take a look at [akka.net](http://getakka.net/).


Motivation
----------

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
	var fooActor = fact.Build<IFoo>( new ConcreteFoo());
```	
4) Use the actor: all method call will be executed on a dedicated thread

```CSharp
	//This will call ConcreteFoo Bar in its own thread
	var res = await fooActor.Bar();
```		


How it works
------------
Internally, ComposableAsync.Factory uses [Castle Core DynamicProxy](https://github.com/castleproject/Core) to instantiate a proxy for the corresponding interface.
All calls to the interface methods are intercepted and then redirected to run on the actor Threads.

Nuget
-----

[Go nuget package](https://www.nuget.org/packages/EasyActor/)

