using System;
using System.Collections.Concurrent;

namespace ComposableAsync.Factory.Test.TestInfra.DummyClass
{
    public class DummyClassFactory
    {
        public int Created => DummyClasses.Count;
        public ConcurrentBag<ComposableAsync.Factory.Test.TestInfra.DummyClass.DummyClass> DummyClasses { get; } = new ConcurrentBag<ComposableAsync.Factory.Test.TestInfra.DummyClass.DummyClass>();

        public Func<ComposableAsync.Factory.Test.TestInfra.DummyClass.DummyClass> Factory => Create;

        private ComposableAsync.Factory.Test.TestInfra.DummyClass.DummyClass Create()
        {
            var res = new ComposableAsync.Factory.Test.TestInfra.DummyClass.DummyClass();
            DummyClasses.Add(res);
            return res;
        }
    }
}
