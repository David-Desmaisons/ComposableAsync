using System;

namespace EasyActor.Helper
{
    internal static class TypeHelper
    {
        public static readonly Type FiberProviderType = typeof(IFiberProvider);
        public static readonly Type AsyncDisposable = typeof(IAsyncDisposable);
    }
}
