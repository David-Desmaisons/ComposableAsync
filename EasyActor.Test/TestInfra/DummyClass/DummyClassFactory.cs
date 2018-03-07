using System;
using System.Collections.Concurrent;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public class DummyClassFactory
    {
        public int Created => DummyClasses.Count;
        public ConcurrentBag<DummyClass> DummyClasses { get; private set; } = new ConcurrentBag<DummyClass>();

        public Func<DummyClass> Factory => Create;

        private DummyClass Create()
        {
            var res = new DummyClass();
            DummyClasses.Add(res);
            return res;
        }
    }
}
