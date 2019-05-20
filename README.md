Composable Async
================

[![build](https://img.shields.io/appveyor/ci/David-Desmaisons/EasyActor.svg)](https://ci.appveyor.com/project/David-Desmaisons/EasyActor)
[![NuGet Badge](https://buildstats.info/nuget/EasyActor)](https://www.nuget.org/packages/EasyActor/)
[![MIT License](https://img.shields.io/github/license/David-Desmaisons/EasyActor.svg)](https://github.com/David-Desmaisons/EasyActor/blob/master/LICENSE)

## Goal

Create and compose complex asynchronous behavior in .Net.

Re-use these building blocks using [aspect oriented programming](https://www.wikiwand.com/en/Aspect-oriented_programming).

Composable Async first appears to provide a lightweight way to transform POCOs in [actors](https://en.wikipedia.org/wiki/Actor_model) and then evolves to become generic.


Motivation
----------

* Leverage C# 5.0 asynchronous API (Task, async , await)
* Simplify concurrent programing getting using Actor model.
* Transparent for consumer: factories transform any POCO by adding behaviors and return user defined interface.
* Fast: performance overhead should be minimum

## Features

Composable Async provides:

1. `IDispatcher` abstraction that take an Action or Function and replay it in a different context.

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

2. Pre-built `IDispatchers` such as:
	- Rate limiting
	- Fiber implementation to build object that uses [Actor pattern](https://en.wikipedia.org/wiki/Actor_model).
	- Circuit-breaker (incoming)

3. Extension methods to compose and await `IDispatcher`

4. Factories to add `IDispatcher` behaviors to [plain old CLR Objects](https://www.wikipedia.org//wiki/Plain_old_CLR_object)


## Usage - Example

### Actor

This library notably can be used as a way that transform POCO in actors. 

Actor leaves in their own thread and communicate with immutable message. They communicate with other objects asynchronously using Task and Task<T>.

Composable.Async provide a factory allowing the transformation of POCO in actor that are then seen through an interface.
Actor guarantees that all calls to the actor interface will occur in a separated thread, sequentially.

The target interface should only expose methods returning Task or Task<T>.
If this not the case, an exception will be raised at runtime when calling a none compliant method.
Make also sure that all method parameters and return values are immutable to avoid concurrency problems.

To create an actor:

1) Define an interface

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
	var fooActor = fact.Build<IFoo>(new ConcreteFoo());
```	
4) Use the actor: all methods call will be executed on a dedicated thread

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

