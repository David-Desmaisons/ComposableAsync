using System;
using System.Threading.Tasks;
using FluentAssertions.Specialized;

namespace ComposableAsync.Resilient.Test.Helper
{
    public static class AssertionExtension
    {
        public static async Task<ExceptionAssertions<Exception>> ThrowAsync(this NonGenericAsyncFunctionAssertions assertions, Type exceptionType)
        {
            return (await assertions.ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
        }

        public static async Task<ExceptionAssertions<Exception>> ThrowAsync<T>(this GenericAsyncFunctionAssertions<T> assertions, Type exceptionType)
        {
            return (await assertions.ThrowAsync<Exception>()).Where(ex => ex.GetType() == exceptionType);
        }      
    }
}
