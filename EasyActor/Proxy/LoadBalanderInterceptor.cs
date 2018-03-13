using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EasyActor.TaskHelper;
using EasyActor.Factories;
using EasyActor.Fiber;
using EasyActor.Helper;

namespace EasyActor.Proxy
{
    internal sealed class LoadBalanderInterceptor<T> : IInterceptor where T : class
    {
        private readonly Func<T> _Builder;
        private readonly int _ParrallelLimitation;
        private readonly BalancingOption _BalancingOption;
        private bool _IsCancelled = false;
        private int _Count = 0;

        private readonly List<Tuple<T, IMonoThreadFiber>> _Actors;
        private readonly ActorFactory _ActorFactory;
        private readonly object _Syncobject = new object();

        internal LoadBalanderInterceptor(Func<T> builder, BalancingOption balancingOption, ActorFactory actorFactory, int parrallelLimitation)
        {
            _Actors = new List<Tuple<T, IMonoThreadFiber>>(parrallelLimitation);
            _Builder = builder;
            _ParrallelLimitation = parrallelLimitation;
            _ActorFactory = actorFactory;
            _BalancingOption = balancingOption;
        }

        private T GetInactiveActor()
        {
            return _Actors.FirstOrDefault(act => act.Item2.EnqueuedTasksNumber == 0)?.Item1;
        }

        private T GetActorWithLessActivity()
        {
            T result = null;
            var minTaskCount = 0;
            foreach (var actor in _Actors)
            {
                var taskCount = actor.Item2.EnqueuedTasksNumber;
                if ((result == null) || taskCount < minTaskCount)
                {
                    result = actor.Item1;
                    minTaskCount = taskCount;
                }
            }
            return result;
        }

        private T CreateNewActor()
        {
            _Count++;
            var n = _ActorFactory.InternalBuildAsync<T>(_Builder).Result;
            var tuple = new Tuple<T,IMonoThreadFiber>(n.Item1, n.Item2 as IMonoThreadFiber);
            _Actors.Add(tuple);
            return n.Item1;
        }

        private T GetCorrectActor()
        {
            lock (_Syncobject)
            {
                if (_IsCancelled)
                    return null;

                if (_Count == _ParrallelLimitation)
                    return GetActorWithLessActivity();

                if (_BalancingOption == BalancingOption.PreferParralelism)
                    return CreateNewActor();

                return GetInactiveActor() ?? CreateNewActor();
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
            invocation.ReturnValue = actor != null ? invocation.CallOn(actor) : TaskBuilder.GetCancelled(invocation.Method.ReturnType);
        }
    }
}
