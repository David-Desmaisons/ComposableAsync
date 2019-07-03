Composable Async
================

[![build](https://img.shields.io/appveyor/ci/David-Desmaisons/ComposableAsync.svg)](https://ci.appveyor.com/project/David-Desmaisons/ComposableAsync)
[![NuGet Badge](https://buildstats.info/nuget/ComposableAsync.Core?includePreReleases=true)](https://www.nuget.org/packages/ComposableAsync.Core/)
[![MIT License](https://img.shields.io/github/license/David-Desmaisons/ComposableAsync.svg)](https://github.com/David-Desmaisons/ComposableAsync/blob/master/LICENSE)


# Goal

* Create and compose complex asynchronous behavior in .Net.

* Use these behaviors as building blocks with [aspect oriented programming](https://www.wikiwand.com/en/Aspect-oriented_programming).

* Provide a lightweight way to transform POCOs in [actors](https://en.wikipedia.org/wiki/Actor_model).

* For .Net Framework and .Net Core


# Motivation

* Leverage C# 5.0 asynchronous API (Task, async , await)

* Simplify concurrent programing getting using Actor model.

* Transparent for consumer: factories transform any POCO by adding behaviors and return user defined interface.

* Fast: performance overhead should be minimum

# Features

## `IDispatcher` abstraction 

A dispatcher takes an Action, a Function, or a Task and replay it in a different context.

```CSharp
public interface IDispatcher
{
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

	/// <summary>
	/// Enqueue the task and return a task corresponding
	/// to the execution of the original task
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="action"></param>
	/// <returns></returns>
	Task<T> Enqueue<T>(Func<Task<T>> action);

	///...Additional signatures including Task and CancellationToken
}
```

## Pre-built `IDispatchers`:
- Rate limiting with [RateLimiter](http://david-desmaisons.github.io/RateLimiter/index.html)

- [Fiber](https://www.wikiwand.com/en/Fiber_(computer_science)) implementation to build object that uses [Actor pattern](https://en.wikipedia.org/wiki/Actor_model).

- Circuit-breaker (incoming)

## Extension methods
In order to transform, compose and await `IDispatcher`

## Factories 
In order to add `IDispatcher` behaviors to [plain old CLR Objects](https://www.wikipedia.org//wiki/Plain_old_CLR_object)

# Usage - Example

## Dispatchers

### Core functions (ComposableAsync.Core nuget)
- Create a dispatcher:

```CSharp
var fiberDispatcher = Fiber.CreateMonoThreadedFiber();
```

- Basic usage

```CSharp
for(int i=0; i<1000; i++)
{
	await fiberDispatcher.Enqueue(ConsoleIt);
}

//...
private void ConsoleIt()
{
	Console.WriteLine($"This is fiber thread {Thread.CurrentThread.ManagedThreadId}");
}
```

- Await a dispatcher:

```CSharp
await fiberDispatcher;
// After the await, the code executes in the dispatcher context
// In this case the code will execute on the fiber thread
Console.WriteLine($"This is fiber thread {Thread.CurrentThread.ManagedThreadId}");
```

- Compose two dispatchers:

```CSharp
var composed = dispatcher1.Then(dispatcher2);
```

- Use a dispatcher as a [HttpMessageHandler](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpmessagehandler?view=netframework-4.8):
```CSharp
var handler = TimeLimiter
	.GetFromMaxCountByInterval(60, TimeSpan.FromMinutes(1))
	.AsDelegatingHandler();
var client = new HttpClient(handler)
```

### With ComposableAsync.Factory nuget:

- Use a dispatcher to create a proxy:

```CSharp
var proxyFactory = new ProxyFactory(dispatcher);
var proxyObject = proxyFactory.Build<IBusinessObject>(new BusinessObject());
```

- Release all factory resources:

```CSharp
await proxyFactory.DisposeAsync();
```

Note that ComposableAsync.Concurrent library provides simplified API to create an actor. See below.

### Actor (ComposableAsync.Concurrent nuget)

Actor leaves in their own thread and communicate with immutable message. They communicate with other objects asynchronously using Task and Task<T>.

 ComposableAsync.Concurrent provides a factory allowing the transformation of POCO in actor that are then seen through an interface.
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
var builder = new ActorFactoryBuilder();
var factory = builder.GetActorFactory(shared: false);
// When shared is true, all actor leaves in the same thread,
// when shared is false, each actor leaves in its own thread.

// Instantiate an actor from a POCO
var fooActor = fact.Build<IFoo>(new ConcreteFoo());
```	
4) Use the actor: all methods call will be executed on a dedicated thread

```CSharp
//This will call ConcreteFoo Bar in its own thread
var res = await fooActor.Bar();
```

5) Life cycle

To release all resources linked to thread management call `DisposeAsync` on the factory (typically called when closing application and actors won't be used):


```CSharp
await proxyFactory.DisposeAsync();
```

How it works
------------
Internally, `ComposableAsync.Factory` uses [Castle Core DynamicProxy](https://github.com/castleproject/Core) to instantiate a proxy for the corresponding interface.
All calls to the interface methods are intercepted and then redirected to run on the actor Threads.

Nuget
-----
For core functionality:

```
Install-Package ComposableAsync.Core
```

For factories:

```
Install-Package ComposableAsync.Factory
```

For actors:

```
Install-Package ComposableAsync.Concurrent
```

[Go nuget packages](https://www.nuget.org/packages/ComposableAsync.Core/)

