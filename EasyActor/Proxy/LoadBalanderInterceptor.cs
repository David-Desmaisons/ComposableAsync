using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using EasyActor.Queue;
using EasyActor.TaskHelper;
using EasyActor.Collections;
using EasyActor.Helper;

namespace EasyActor.Proxy
{
    internal class LoadBalanderInterceptor<T> : IInterceptor where T : class
    {

        private readonly Func<T> _Builder;
        private readonly int _ParrallelLimitation;
        private readonly BalancingOption _BalancingOption;
        private bool _isCancelled = false;

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
                if (_isCancelled)
                    return null;

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

        private void Cancel(IInvocation invocation)
        {
            lock (_syncobject)
            {
                _isCancelled = true;
                var tasks = _actors.Select(a => (Task)invocation.CallOn(a.Item1 as IActorCompleteLifeCycle)).ToArray();
                _actors.Clear();
                invocation.ReturnValue = Task.WhenAll(tasks);
            }
        }

        [DebuggerNonUserCode]
        public void Intercept(IInvocation invocation)
        {
            if (TypeHelper.IsActorCompleteLifeCycleTypeOrBase(invocation.Method.DeclaringType))
            {
                Cancel(invocation);
                return;
            }

            var actor = GetCorrectActor();
            if (actor!=null)
                invocation.ReturnValue = invocation.CallOn(actor);
            else
                invocation.ReturnValue = TaskBuilder.GetCancelled(invocation.Method.ReturnType);
        }
    }
}
