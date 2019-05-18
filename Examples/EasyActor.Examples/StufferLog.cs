using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace EasyActor.Examples
{
    public class StufferLog : IDoStuff
    {
        private readonly ITestOutputHelper _TestOutput;

        public StufferLog(ITestOutputHelper testOutput)
        {
            _TestOutput = testOutput;
        }

        public Task DoStuff()
        {
            _TestOutput.WriteLine($"Doing: {DateTime.Now:O} on Thread {Thread.CurrentThread.ManagedThreadId}");
            return Task.CompletedTask;
        }

        public Task<int> GetCount()
        {
            return Task.FromResult(0);
        }
    }
}
