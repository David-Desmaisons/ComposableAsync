# Objective

ComposableAsync.Resilient provides dispatchers that implement resilient behaviors to handle exceptions.

# Features

## RetryPolicy

RetryPolicy provide a dispatcher factory that will re-execute a statement in case that its previous execution raise an exception.

- For all exceptions:

```C#
// Create dispatcher that catch all exceptions and retry for ever
var forEverDispatcher = RetryPolicy.ForAllException().ForEver();

// Use this dispatcher for executing method DoExecute
forEverDispatcher.Enqueue(() => DoExecute());
```

```C#
// Create dispatcher that catch all exceptions and retry up to 5 times, after that the original exception is re-thrown.
var forEverDispatcher = RetryPolicy.ForAllException().WithMaxRetry(5);
```

- For exception by type:

```C#
// Create dispatcher that catch all ArgumentException and retry for ever
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().ForEver();
```

```C#
// Create dispatcher that catch all ArgumentException and NullReferenceException and retry execute action for ever
var selectiveDispatcher = RetryPolicy.For<ArgumentException>().And<NullReferenceException>().ForEver();
```

- With wait between retries:

```C#
// Create dispatcher that catch all ArgumentException and retry for ever with a delay of 200 ms
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(200).ForEver();
```
```C#
// alternatively
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(TimeSpan.FromSeconds(0.2)).ForEver();
```

- With variable waits between retries:

```C#
// Create dispatcher that catch all ArgumentException exception and retry up to three times with a delay of 10ms between first and second try, then 50ms between second and third, then 200ms between third and fourth.
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(10,50,200).WithMaxRetry(3);
```

- Match a specific exception:

```C#
// Create dispatcher that catch all Exception with a specific message for ever.
var forArgumentExceptionDispatcher = RetryPolicy.ForException(ex => ex.Message == expectedMessageString).ForEver();
```

```C#
// Create dispatcher that catch all ArgumentException with a specific message for ever.
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>(ex => ex.Message == expectedMessageString).ForEver();
```

As any dispatcher, `ComposableAsync.Resilient` dispatchers can also be transformed into httpHandler or used ro proxify POCO thanks to `ComposableAsync.Resilient` and `ComposableAsync.Resilient` respectively.

# Nuget

```
Install-Package ComposableAsync.Resilient
```

[Go nuget packages](https://www.nuget.org/packages/ComposableAsync.Resilient/)

