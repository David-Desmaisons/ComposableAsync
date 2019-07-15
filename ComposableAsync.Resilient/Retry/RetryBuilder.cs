using System;
using System.Collections.Generic;

namespace ComposableAsync.Retry
{
    internal class RetryBuilder: IRetryBuilder
    {
        private readonly bool _All;
        private readonly HashSet<Type> _Type = new HashSet<Type>();
        private int _MaxTry = Int32.MaxValue;

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
            return _All ? (IBasicDispatcher)new ForEverRetryDispatcher(_MaxTry) :
                new ForEverRetrySelectiveDispatcher(_Type, _MaxTry);
        }

        public IDispatcher Until(int maxTimes)
        {
            _MaxTry = maxTimes;
            return GetBasicDispatcher().ToFullDispatcher();
        }
    }
}
