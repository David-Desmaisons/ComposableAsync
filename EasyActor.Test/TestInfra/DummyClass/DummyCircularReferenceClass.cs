using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public class DummyCircularReferenceClass : IDummyInterface3
    {
        public IDummyInterface3 DummyInterface { get; set; }

        public async Task<int> DoAsync(bool First)
        {
            if (First == false)
                return 1;

            var res = DummyInterface.DoAsync(false).Result + 1;
            res += await DummyInterface.DoAsync(false);
            return res;
        }
    }
}
