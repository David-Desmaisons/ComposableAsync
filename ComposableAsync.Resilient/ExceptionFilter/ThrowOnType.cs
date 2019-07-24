using System;
using System.Collections.Generic;
using System.Linq;

namespace ComposableAsync.Resilient.ExceptionFilter
{
    internal class ThrowOnType : IExceptionFilter
    {
        private readonly HashSet<Type> _Types;

        internal ThrowOnType(HashSet<Type> types)
        {
            this._Types = types;
        }

        public bool IsFiltered(Exception exception)
        {
            var type = exception.GetType();
            return _Types.All(t => t != type && !type.IsSubclassOf(t));
        }
    }
}
