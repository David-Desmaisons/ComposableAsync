# Objective

ComposableAsync.Resilient provides dispatchers that implement resilient behaviors to handle exceptions.

# Features

## CircuitBreakerPolicy

[Circuit breaker pattern]() allows to stop stressing a service under failure. 
- Circuit breaker starts with state open:
  - calls to circuit breaker calls the service
  - the number of successive exceptions raised is counted
- When this number reaches a threshold, state switches to open:
  - call to original service are stopped
  - circuit breaker emits whether exception or returns a default value
- When open, after a time-out circuit breaker enters in half-open mode:
  - service is called again 
    - in case of success the state switches to closed state
    - in case of failure the state switches to open


### Create a CircuitBreaker for all exceptions:

```C#
// Create a circuit breaker that opens after 5 consecutive exceptions and stays open 500ms before re-trying
var circuitBreaker = CircuitBreakerPolicy.ForAllException().WithRetryAndTimeout(5, TimeSpan.FromMilliseconds(500));

// Use this dispatcher for executing method DoExecute
await circuitBreaker.Enqueue(() => DoExecute());
```

### For specific exceptions:

```C#
// Create a circuit breaker for all that opens after 10 consecutive TimeoutException and stays open 500ms before re-trying
var circuitBreaker = CircuitBreakerPolicy.For<TimeoutException>().WithRetryAndTimeout(10, TimeSpan.FromMilliseconds(500));
```

```C#
// Create circuit breaker for exception that opens after 5 consecutive exceptions with the expected message and stays open 500ms before re-trying
var circuitBreaker = CircuitBreakerPolicy.ForException(ex => ex.Message == expected).WithRetryAndTimeout(5, TimeSpan.FromMilliseconds(500));
```


```C#
// Create circuit breaker that opens after 15 consecutive SystemException and stays open 5s before re-trying
var circuitBreaker = CircuitBreakerPolicy.For<SystemException>(ex => ex.Message == expected).WithRetryAndTimeout(15, TimeSpan.FromSeconds(5));
```

### Return default or specific values

By default the circuit-breaker will throw an `CircuitBreakerOpenException` when opened.

It is possible to configure a circuit-breaker to implement alternative logic when opened:

```C#
// Create circuit breaker that opens after 15 consecutive SystemException and stays open 5s before re-trying
var circuitBreaker = CircuitBreakerPolicyForAllException().ReturnsDefaultWhenOpen().WithRetryAndTimeout(15, TimeSpan.FromSeconds(5));

// When opened the res value will be 0
var res = await circuitBreaker.Enqueue(() => GetIntValue());
```

```C#
// Create circuit breaker that opens after 15 consecutive SystemException and stays open 5s before re-trying
var circuitBreaker = CircuitBreakerPolicyForAllException().ReturnsWhenOpen<string>("open circuit").WithRetryAndTimeout(15, TimeSpan.FromSeconds(5));

// When opened the res value will be "open circuit"
var res = await circuitBreaker.Enqueue(() => GetStringValue());
```

### Prevent exception for function returning void or Task

```C#
// Create circuit breaker that opens after 15 consecutive SystemException and stays open 5s before re-trying
var circuitBreaker = CircuitBreakerPolicyForAllException().DoNotThrowForVoid().WithRetryAndTimeout(15, TimeSpan.FromSeconds(5));

// This will not throw when circuit opens
await circuitBreaker.Enqueue(() => ExecuteCommand());
```

## RetryPolicy

RetryPolicy provide a dispatcher factory that will re-execute a statement in case that its previous execution raise an exception.

### For all exceptions:

```C#
// Create dispatcher that catch all exceptions and retry for ever
var forEverDispatcher = RetryPolicy.ForAllException().ForEver();

// Use this dispatcher for executing method DoExecute
await forEverDispatcher.Enqueue(() => DoExecute());
```

```C#
// Create dispatcher that catch all exceptions and retry up to 5 times, after that the original exception is re-thrown.
var forEverDispatcher = RetryPolicy.ForAllException().WithMaxRetry(5);
```

### For exception by type:

```C#
// Create dispatcher that catch all ArgumentException and retry for ever
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().ForEver();
```

```C#
// Create dispatcher that catch all ArgumentException and NullReferenceException and retry execute action for ever
var selectiveDispatcher = RetryPolicy.For<ArgumentException>().And<NullReferenceException>().ForEver();
```

### With wait between retries:

```C#
// Create dispatcher that catch all ArgumentException and retry for ever with a delay of 200 ms
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(200).ForEver();
```
```C#
// alternatively
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(TimeSpan.FromSeconds(0.2)).ForEver();
```

### With variable waits between retries:

```C#
// Create dispatcher that catch all ArgumentException exception and retry up to three times with a delay of 10ms between first and second try, then 50ms between second and third, then 200ms between third and fourth.
var forArgumentExceptionDispatcher = RetryPolicy.For<ArgumentException>().WithWaitBetweenRetry(10,50,200).WithMaxRetry(3);
```

### Match a specific exception:

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

