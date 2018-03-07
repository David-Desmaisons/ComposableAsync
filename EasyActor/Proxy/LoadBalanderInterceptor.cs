using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyActor.Queue;
using EasyActor.TaskHelper;
using EasyActor.Collections;
using EasyActor.Factories;
using EasyActor.Helper;

namespace EasyActor.Proxy
{
    internal sealed class LoadBalanderInterceptor<T> : IInterceptor where T : class
    {
        private readonly Func<T> _Builder;
        private readonly int _ParrallelLimitation;
        private readonly BalancingOption _BalancingOption;
        private bool _IsCancelled = false;

        private readonly ConcurrentBag<Tuple<T, MonoThreadedQueue>> _Actors = new ConcurrentBag<Tuple<T, MonoThreadedQueue>>();
        private readonly ActorFactory _ActorFactory;
        private readonly object _Syncobject = new object();

        internal LoadBalanderInterceptor(Func<T> builder, BalancingOption balancingOption, ActorFactory actorFactory, int parrallelLimitation)
        {
            _Builder = builder;
            _ParrallelLimitation = parrallelLimitation;
            _ActorFactory = actorFactory;
            _BalancingOption = balancingOption;
        }

        private Tuple<int,T> GetBestActor()
        {
            return  _Actors.Select(act => new Tuple<int,T>(act.Item2.EnqueuedTasksNumber, act.Item1))
                                    .OrderBy(act => act.Item1).FirstOrDefault();
        }

        private T CreateNewActor()
        {
            var n = _ActorFactory.InternalBuildAsync<T>(_Builder).Result;
            _Actors.Add(n);
            return n.Item1;
        }

        private T GetCorrectActor()
        {
            var alreadyfull = _Actors.Count == _ParrallelLimitation;
            var candidate = GetBestActor();

            if ((candidate != null) && (alreadyfull))
            {
                return candidate.Item2;
            }

            lock (_Syncobject) 
            {
                if (_IsCancelled)
                    return null;

                //check again for fullness under lock for thread safety 
                if (_Actors.Count==_ParrallelLimitation)
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
            lock (_Syncobject)
            {
                _IsCancelled = true;
                var tasks = _Actors.Select(a => (Task)invocation.CallOn(a.Item1 as IActorCompleteLifeCycle)).ToArray();
                _Actors.Clear();
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
            invocation.ReturnValue = actor!=null ? invocation.CallOn(actor) : TaskBuilder.GetCancelled(invocation.Method.ReturnType);
        }
    }
}
