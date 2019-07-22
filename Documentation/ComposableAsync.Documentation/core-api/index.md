# Objective

ComposableAsync.Core provides dispatcher interface definition and extensions methods.

# Features

## `IDispatcher` abstraction 

A dispatcher takes an Action, a Function, or a Task and replay it in a different context.

```C#
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

## Extension methods
In order to transform, compose and await `IDispatcher`

- Await a dispatcher:

The code following a await on a dispatcher will be executed in the dispatcher context.

```C#
await fiberDispatcher;
// After the await, the code executes in the dispatcher context
// In this case the code will execute on the fiber thread
Console.WriteLine($"This is fiber thread {Thread.CurrentThread.ManagedThreadId}");
```

- Compose two dispatchers:

```C#
var composed = dispatcher1.Then(dispatcher2);
```

- Transform a dispatcher into [HttpMessageHandler](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpmessagehandler?view=netframework-4.8) with AsDelegatingHandler extension method:
```C#
/// Using timelimiter nuget
var handler = TimeLimiter
	.GetFromMaxCountByInterval(60, TimeSpan.FromMinutes(1))
	.AsDelegatingHandler();
var client = new HttpClient(handler)
```

# Nuget

```
Install-Package ComposableAsync.Core
```

[Go nuget packages](https://www.nuget.org/packages/ComposableAsync.Core/)
