﻿using Castle.DynamicProxy;
using EasyActor.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor
{
    public class SynchronizationContextFactory : ActorFactoryBase, IActorFactory
    {
        private SynchronizationContextQueue _Context;

        public SynchronizationContextFactory(): this(SynchronizationContext.Current)
        {
        }

        public SynchronizationContextFactory(SynchronizationContext synchronizationContext)
        {
            if (synchronizationContext == null)
                throw new ArgumentNullException("synchronizationContext can not be null");

            _Context = new SynchronizationContextQueue(synchronizationContext);
        }

        public T Build<T>(T concrete) where T : class
        {
            return Create(concrete, _Context); 
        }
    }
}
