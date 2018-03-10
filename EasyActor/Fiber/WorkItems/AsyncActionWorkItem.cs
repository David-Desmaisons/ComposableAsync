﻿using System;
using System.Threading.Tasks;

namespace EasyActor.Fiber.WorkItems
{

    internal class AsyncActionWorkItem : AsyncWorkItem<object>, IWorkItem
    {
        public AsyncActionWorkItem(Func<Task> @do)
            : base(async () => { await @do(); return null; })
        {
        }
    }
}
