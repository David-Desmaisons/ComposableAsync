Composable Async
================

[![build](https://img.shields.io/appveyor/ci/David-Desmaisons/ComposableAsync.svg)](https://ci.appveyor.com/project/David-Desmaisons/ComposableAsync)
[![NuGet Badge](https://buildstats.info/nuget/ComposableAsync.Core?includePreReleases=true)](https://www.nuget.org/packages/ComposableAsync.Core/)
[![MIT License](https://img.shields.io/github/license/David-Desmaisons/ComposableAsync.svg)](https://github.com/David-Desmaisons/ComposableAsync/blob/master/LICENSE)

Create, compose and inject asynchronous behaviors in .Net Framework and .Net Core.

# Goal

* Create asynchronous behavior such as [fiber](https://www.wikiwand.com/en/Fiber_(computer_science)), rate limiter, [circuit breaker](https://www.wikiwand.com/en/Circuit_breaker_design_pattern).

* Compose these behaviors and use them as building blocks with [aspect oriented programming](https://www.wikiwand.com/en/Aspect-oriented_programming).

* Provide a lightweight way inject these behaviors to transform POCOs in [actors](https://en.wikipedia.org/wiki/Actor_model).


# Create asynchronous behavior

Asynchronous behaviors are implemented using [IDispatcher abstraction](./core-api/ComposableAsync.IDispatcher.html). 

Composable Async provides various dispatchers implementation:


## Retry

```C#
// Create dispatcher that catch all ArgumentException and retry for ever with a delay of 200 ms
var retryDispatcher = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(TimeSpan.FromSeconds(0.2)).ForEver();
```

See more at [ComposableAsync.Resilient](./resilient-api/index.html#retrypolicy)

## Circuit-Breaker

```C#
// Create dispatcher that catch all ArgumentException and retry for ever with a delay of 200 ms
var retryDispatcher = CircuitBreakerPolicy.For<TimeoutException>().WithRetryAndTimeout(10, TimeSpan.FromMilliseconds(500));
```

See more at [ComposableAsync.Resilient](./resilient-api/index.html#circuitbreakerpolicy)

## Fiber

```C#
// Create dispatcher that dispatch all action on the same thread
var fiberDispatcher = Fiber.CreateMonoThreadedFiber();
```

See more at [ComposableAsync.Concurrent](./concurrent-api/index.html)

##  RateLimiter

```C#
// Create dispatcher that dispatch all action on the same thread
var timeConstraint = TimeLimiter.GetFromMaxCountByInterval(5, TimeSpan.FromSeconds(1));
```

See more at [RateLimiter](https://github.com/David-Desmaisons/RateLimiter)


# Compose dispatchers

Use then extension methods to create a dispatcher that will execute sequentially dispatchers

```C#
/// <summary>
/// Returns a composed dispatcher applying the given dispatchers sequentially
/// </summary>
/// <param name="dispatcher"></param>
/// <param name="others"></param>
/// <returns></returns>
public static IDispatcher Then(this IDispatcher dispatcher, IEnumerable<IDispatcher> others)
```

```C#
var composed = fiberDispatcher.Then(timeConstraint);
```

# Use dispatchers

## Await dispatcher

```C#
await fiberDispatcher;
// After the await, the code executes in the dispatcher context
// In this case the code will execute on the fiber thread
Console.WriteLine($"This is fiber thread {Thread.CurrentThread.ManagedThreadId}");
```

## As httpDelegateHandler

Transform a dispatcher into [HttpMessageHandler](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpmessagehandler?view=netframework-4.8) with AsDelegatingHandler extension method:
```C#
/// Using time limiter nuget
var handler = TimeLimiter
	.GetFromMaxCountByInterval(60, TimeSpan.FromMinutes(1))
	.AsDelegatingHandler();
var client = new HttpClient(handler);
```

## As wrapper for proxy Factory

Using `ComposableAsync.Factory`, with this option all methods call to the proxyfied object are wrapped using the provided dispatcher.

```C#
var retryDispatcher = RetryPolicy.For<SystemException>().ForEver();

var originalObject = new BusinessObject();
var proxyFactory = new ProxyFactory(retryDispatcher);
var proxyObject = proxyFactory.Build<IBusinessObject>(originalObject);

// The call to the originalObject will be wrapped into a retry policy for SystemException
var res = await proxyObject.Execute(cancellationToken);
```

# Actors

`ComposableAsync.Concurrent` also provides an [actor](https://en.wikipedia.org/wiki/Actor_model) factory based on fiber and proxy factory.

```C#
// Instantiate actor factory
var builder = new ActorFactoryBuilder();
var factory = builder.GetActorFactory(shared: false);
// When shared is true, all actor leaves in the same thread,
// when shared is false, each actor leaves in its own thread.

// Instantiate an actor from a POCO
var fooActor = fact.Build<IFoo>(new ConcreteFoo());
```

See more at [ComposableAsync.Concurrent](./concurrent-api/index.html)


# Nuget

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

