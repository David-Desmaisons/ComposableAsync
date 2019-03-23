using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Concurrent;
using Concurrent.Tasks;

namespace EasyActor.Proxy
{
    [DebuggerNonUserCode]
    internal static class DispatcherBehaviour
    {
        internal static readonly InvocationOnDispatcher DispatchFunction = Dispatch;

        private static readonly MethodInfo _Proceed = typeof(DispatcherBehaviour).GetMethod(nameof(Proceed), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _ProceedForCancellation = typeof(ProcessWithCancellationHelper).GetMethod(nameof(ProcessWithCancellationHelper.ProceedWithCancellation), BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Type _CancellationTokenSourceType = typeof(CancellationToken);
        private static readonly InvocationOnDispatcher _DynamicEnqueueFunction = DynamicEnqueue;
        private static readonly InvocationOnDispatcher _EnqueueFunction = Enqueue;

        internal static InvocationOnDispatcher GetInvocationOnDispatcherForTask(MethodInfo method)
        {
            var cancellationParameter = GetCancellationParameter(method);
            if (cancellationParameter == null)
                return _EnqueueFunction;

            var parameterIndex = cancellationParameter.Position;
            object Res(ICancellableDispatcher dispatcher, IInvocation invocation) => Enqueue(dispatcher, invocation, parameterIndex);
            return Res;
        }

        internal static InvocationOnDispatcher GetInvocationOnDispatcherForGenericTask(Type targetType, MethodInfo method)
        {
            var cancellationParameter = GetCancellationParameter(method);
            if (cancellationParameter == null)
            {
                var methodInfo = _Proceed.MakeGenericMethod(targetType);
                return (InvocationOnDispatcher)Delegate.CreateDelegate(typeof(InvocationOnDispatcher), methodInfo);
            }

            var helper = new ProcessWithCancellationHelper(cancellationParameter.Position);
            var methodInfoWithCancellation = _ProceedForCancellation.MakeGenericMethod(targetType);
            return (InvocationOnDispatcher)methodInfoWithCancellation.CreateDelegate(typeof(InvocationOnDispatcher), helper);
        }

        internal static InvocationOnDispatcher GetDynamicInvocationOnDispatcher(MethodInfo method)
        {
            var cancellationParameter = GetCancellationParameter(method);
            if (cancellationParameter == null)
                return _DynamicEnqueueFunction;

            var helper = new ProcessWithCancellationHelper(cancellationParameter.Position);
            return helper.DynamicProceedWithCancellation;
        }

        private static ParameterInfo GetCancellationParameter(MethodInfo method)
            => method.GetParameters().FirstOrDefault(p => p.ParameterType == _CancellationTokenSourceType);

        private static object Dispatch(ICancellableDispatcher dispatcher, IInvocation invocation)
        {
            dispatcher.Dispatch(invocation.Call);
            return null;
        }

        private static object Enqueue(ICancellableDispatcher dispatcher, IInvocation invocation, int cancellationTokenPosition)
        {
            var cancellationToken = (CancellationToken)invocation.Arguments[cancellationTokenPosition];
            return dispatcher.Enqueue(invocation.Call<Task>, cancellationToken);
        }

        private static object Enqueue(ICancellableDispatcher dispatcher, IInvocation invocation) =>
            dispatcher.Enqueue(invocation.Call<Task>);

        private static object Proceed<TResult>(ICancellableDispatcher dispatcher, IInvocation invocation) =>
            dispatcher.Enqueue(invocation.Call<Task<TResult>>);

        private static object DynamicEnqueue(ICancellableDispatcher dispatcher, IInvocation invocation)
            => DynamicDispatch(_Proceed, dispatcher, invocation);

        private static object DynamicDispatch(MethodInfo genericMethodInfo, ICancellableDispatcher dispatcher, IInvocation invocation, object context = null)
        {
            var solvedTask = invocation.Method.ReturnType.GetTaskType();
            var methodInfo = genericMethodInfo.MakeGenericMethod(solvedTask.Type);
            return methodInfo.Invoke(context, new object[] { dispatcher, invocation });
        }

        private struct ProcessWithCancellationHelper
        {
            private readonly int _Index;

            public ProcessWithCancellationHelper(int index)
            {
                _Index = index;
            }

            internal object ProceedWithCancellation<TResult>(ICancellableDispatcher dispatcher, IInvocation invocation)
            {
                var cancellationToken = (CancellationToken)invocation.Arguments[_Index];
                return dispatcher.Enqueue(invocation.Call<Task<TResult>>, cancellationToken);
            }

            internal object DynamicProceedWithCancellation(ICancellableDispatcher dispatcher, IInvocation invocation)
                => DynamicDispatch(_ProceedForCancellation, dispatcher, invocation, this);
        }
    }
}
