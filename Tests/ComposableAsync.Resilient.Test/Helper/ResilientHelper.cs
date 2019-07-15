using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComposableAsync.Resilient.Test.Helper
{
    public static class ResilientHelper
    {

        public static void SetUpExceptions(this Action action, int times, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.When(f => f.Invoke()).Do(_ =>
            {
                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);
            });
        }

        public static void SetUpExceptions<T>(this Func<T> action, int times, T result, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.Invoke().Returns(_ =>
            {
                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return result;
            });
        }

        public static void SetUpExceptions(this Func<Task> action, int times, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.Invoke().Returns(_ =>
            {
                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.CompletedTask;
            });
        }

        public static void SetUpExceptions<T>(this Func<Task<T>> action, int times, T result, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.Invoke().Returns(_ =>
            {
                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.FromResult(result);
            });
        }

        public static void SetUpExceptionsWithCancellation(this Action action, int times, int cancelAfter, CancellationTokenSource tokenSource, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.When(f => f.Invoke()).Do(_ =>
            {
                if (count == cancelAfter)
                    tokenSource.Cancel();

                if (count++ >= times)
                    return;

                throw (Exception)Activator.CreateInstance(exceptionType);
            });
        }

        public static void SetUpExceptionsWithCancellation<T>(this Func<T> action, int times, int cancelAfter, T result, CancellationTokenSource tokenSource, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.Invoke().Returns(_ =>
            {
                if (count == cancelAfter)
                    tokenSource.Cancel();

                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return result;
            });
        }

        public static void SetUpExceptionsWithCancellation(this Func<Task> action, int times, int cancelAfter, CancellationTokenSource tokenSource, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.Invoke().Returns(_ =>
            {
                if (count == cancelAfter)
                    tokenSource.Cancel();

                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.CompletedTask;
            });
        }

        public static void SetUpExceptionsWithCancellation<T>(this Func<Task<T>> action, int times, int cancelAfter, T result, CancellationTokenSource tokenSource, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(Exception);
            var count = 0;
            action.Invoke().Returns(_ =>
            {
                if (count == cancelAfter)
                    tokenSource.Cancel();

                if (count++ < times)
                    throw (Exception)Activator.CreateInstance(exceptionType);

                return Task.FromResult(result);
            });
        }
    }
}
