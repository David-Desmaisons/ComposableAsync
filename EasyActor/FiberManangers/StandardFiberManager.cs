using System;
using System.Threading;
using System.Threading.Tasks;
using Concurrent;
using Concurrent.Tasks;
using EasyActor.Options;

namespace EasyActor.FiberManangers
{
    internal class StandardFiberManager : IFiberMananger
    {
        public ActorFactorType Type => ActorFactorType.Standard;
        public bool DisposeFiber => true;

        private readonly Action<Thread> _OnCreate;
        public StandardFiberManager(Action<Thread> onCreate = null)
        {
            _OnCreate = onCreate;
        }

        public IFiber GetFiber() => Fiber.CreateMonoThreadedFiber(_OnCreate);

        public Task DisposeAsync() => TaskBuilder.Completed;
    }
}
