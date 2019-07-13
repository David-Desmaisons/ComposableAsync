using System;
using System.Collections.Generic;

namespace ComposableAsync.Retry
{
    internal class RetryBuilder: IRetryBuilder
    {
        private readonly bool _All;
        private readonly HashSet<Type> _Type = new HashSet<Type>();

        public RetryBuilder()
        {
            _All = true;
        }

        public RetryBuilder(Type type)
        {
            _Type.Add(type);
        }

        public IRetryBuilder And<T>() where T : Exception
        {
            _Type.Add(typeof(T));
            return this;
        }

        public IDispatcher ForEver()
        {
            return GetBasicDispatcher().ToFullDispatcher();
        }

        private IBasicDispatcher GetBasicDispatcher()
        {
            return _All ? (IBasicDispatcher)new ForEverRetryDispatcher() :
                new ForEverRetrySelectiveDispatcher(_Type);
        }

        public IDispatcher Until(int maxTimes)
        {
            throw new NotImplementedException();
        }
    }
}
