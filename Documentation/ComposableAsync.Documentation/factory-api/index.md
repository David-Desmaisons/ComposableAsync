# Objective

ComposableAsync.Factory provides factories and factory builder to inject dispatcher code into [POCO](https://en.wikipedia.org/wiki/Actor_model).


# Features

## ProxyFactory

- Use a dispatcher to create a proxy:

Given:

```C#
public interface IBusinessObject
{
	Task Execute();

	Task<int> Execute(CancellationToken token);
}
```
Note that all methods of the IBusinessObject interface should return `Task` or `Task<T>`.

```C#
IDispatcher dispatcher = //get a dispatcher from factory included in ComposableAsync.Resilient, ComposableAsync.Concurrent or RateLimiter for example
var originalObject = new BusinessObject();
var proxyFactory = new ProxyFactory(dispatcher);
var proxyObject = proxyFactory.Build<IBusinessObject>(originalObject);

// The call to the originalObject will be wrapped into an dispatcher Enqueue call
var res = proxyObject.Execute(cancellationToken);
```

`proxyObject` will expose all `originalObject` methods wrapped with the given `dispatcher`.

For example if the dispatcher is:

- a fiber: all methods will be executed on the fiber thread
- a rate limiter: all methods will be executed respecting the time constraint

Note that ComposableAsync.Concurrent library provides simplified API to create an actor. See below.

# How it works
Internally, `ComposableAsync.Factory` uses [Castle Core DynamicProxy](https://github.com/castleproject/Core) to instantiate a proxy for the corresponding interface.
All calls to the interface methods are intercepted and then redirected to run on the actor Threads.

# Nuget

```
Install-Package ComposableAsync.Factory
```

[Go nuget packages](https://www.nuget.org/packages/ComposableAsync.Factory/)

