﻿using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Test.TestInfra.DummyClass
{
    public interface ICancellableInterface
    {
        Task<int> GetIntResult(int delay, CancellationToken cancellationToken);

        Task Do(int delay, CancellationToken cancellationToken);
    }
}
