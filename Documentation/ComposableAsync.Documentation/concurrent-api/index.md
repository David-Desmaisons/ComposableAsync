# Objective

ComposableAsync.Concurrent provides API to create [Actors](https://en.wikipedia.org/wiki/Actor_model) and [fiber](https://www.wikiwand.com/en/Fiber_(computer_science)) dispatcher.

# Features

## Fiber dispatcher:

For a complete definition of fiber see [wiki definition](https://www.wikiwand.com/en/Fiber_(computer_science)).

`ComposableAsync` fibers:
- dispatches all action to the same thread
- manages work queue to allow parallelism 

Internally `ComposableAsync.Concurrent` use a multiple producer, single consumer queue that ensure better performance than .Net 
`BlockingCollection<T>`.

This collection was adapted from [1024cores.net article](http://www.1024cores.net/home/lock-free-algorithms/queues/non-intrusive-mpsc-node-based-queue).

### Create a fiber:

```C#
var fiberDispatcher = Fiber.CreateMonoThreadedFiber();
```

- Basic usage

```C#
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

```C#
await fiberDispatcher;
// After the await, the code executes in the dispatcher context
// In this case the code will execute on the fiber thread
Console.WriteLine($"This is fiber thread {Thread.CurrentThread.ManagedThreadId}");
```

## Actor

### Definition

Actor are object that
- leaves in their own thread.
- receives immutable message as input. 
- responds asynchronously using `Task` and `Task<T>`.


### Factory and FactoryBuilder

`ComposableAsync.Concurrent` factories transform POCO in actor.

To create an actor:

1) Define an interface

```C#
// IFoo definition
public interface IFoo
{
	Task Bar();
}
```
Note that all methods of the actor interface should return `Task` or `Task<T>`.

2) Implement the interface in a POCO	

```C#
// ConcreteFoo definition
public class ConcreteFoo : IFoo
{
	public Task<int> Bar()
	{
		return Task.FromResult<int>(2);
	}
}
```

3) Use an create a factory from the factory builder and instantiate an actor from the POCO

```C#
// Instantiate actor factory
var builder = new ActorFactoryBuilder();
var factory = builder.GetActorFactory(shared: false);
// When shared is true, all actor leaves in the same thread,
// when shared is false, each actor leaves in its own thread.

// Instantiate an actor from a POCO
var fooActor = fact.Build<IFoo>(new ConcreteFoo());
```	
4) Use the actor: all methods call will be executed on a dedicated thread

```C#
//This will call ConcreteFoo Bar in its own thread
var res = await fooActor.Bar();
```

### Life cycle

To release all resources linked to thread management call `DisposeAsync` on the factory (typically called when closing application and actors won't be used):


```C#
await proxyFactory.DisposeAsync();
```

Internally a `ComposableAsync.Concurrent` use fiber to implement actors.


# Nuget

```
Install-Package ComposableAsync.Concurrent
```

[Go nuget packages](https://www.nuget.org/packages/ComposableAsync.Concurrent/)