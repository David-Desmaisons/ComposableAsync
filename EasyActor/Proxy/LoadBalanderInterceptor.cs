using Castle.DynamicProxy;
using EasyActor.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Proxy
{
    internal class LoadBalanderInterceptor<T> : IInterceptor where T : class
    {

        private readonly Func<T> _Builder;
        private readonly int _ParrallelLimitation;
        private readonly BalancingOption _BalancingOption;

        private readonly ConcurrentBag<Tuple<T, MonoThreadedQueue>> _actors = new ConcurrentBag<Tuple<T, MonoThreadedQueue>>();
        private readonly ActorFactory _ActorFactory;
        private readonly object _syncobject = new object();

        internal LoadBalanderInterceptor(Func<T> builder, BalancingOption balancingOption, ActorFactory actorFactory, int parrallelLimitation)
        {
            _Builder = builder;
            _ParrallelLimitation = parrallelLimitation;
            _ActorFactory = actorFactory;
            _BalancingOption = balancingOption;
        }

        private Tuple<int,T> GetBestActor()
        {
            return  _actors.Select(act => new Tuple<int,T>(act.Item2.EnqueuedTasksNumber, act.Item1))
                                    .OrderBy(act => act.Item1).FirstOrDefault();
        }

        private T CreateNewActor()
        {
            var n = _ActorFactory.InternalBuildAsync<T>(_Builder).Result;
            _actors.Add(n);
            return n.Item1;
        }

        private T GetCorrectActor()
        {
            var alreadyfull = _actors.Count == _ParrallelLimitation;
            var candidate = GetBestActor();

            if ((candidate != null) && (alreadyfull))
            {
                return candidate.Item2;
            }

            lock(_syncobject)
            {
                //check again for fullness under lock for thread safety 
                if (_actors.Count==_ParrallelLimitation)
                    return GetBestActor().Item2;

                if (_BalancingOption==BalancingOption.PreferParralelism)
                        return CreateNewActor();

                var best = GetBestActor();
                if ((best!=null) && (best.Item1 == 0))
                    return best.Item2;

                return CreateNewActor();
            }
        }

        [DebuggerNonUserCode]
        public void Intercept(IInvocation invocation)
        {
            var actor = GetCorrectActor();
            invocation.ReturnValue =  invocation.CallOn(actor);
        }
    }
}
