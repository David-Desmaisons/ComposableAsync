using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public class GenericImplementation : IGenericInterface<string>
    {
        public Thread LastCallingThread { get; private set; }
        public Task<string> GetResult(string inArgument)
        {
            LastCallingThread = Thread.CurrentThread;
            return Task.FromResult($"{inArgument}-transformed");
        }

        public Task<string> GetResultString<TOther>(TOther other)
        {
            LastCallingThread = Thread.CurrentThread;
            return Task.FromResult(other.ToString());
        }

        public Task<TOther> GetResult<TOther>(TOther other)
        {
            LastCallingThread = Thread.CurrentThread;
            return Task.FromResult(other);
        }
    }
}
