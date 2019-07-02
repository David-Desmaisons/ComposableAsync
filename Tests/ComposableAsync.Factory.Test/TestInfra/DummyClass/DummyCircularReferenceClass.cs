using System.Threading.Tasks;

namespace ComposableAsync.Factory.Test.TestInfra.DummyClass
{
    public class DummyCircularReferenceClass : IDummyInterface3
    {
        public IDummyInterface3 DummyInterface { get; set; }

        public async Task<int> DoAsync(bool first)
        {
            if (first == false)
                return 1;

            var res = DummyInterface.DoAsync(false).Result + 1;
            res += await DummyInterface.DoAsync(false);
            return res;
        }
    }
}
