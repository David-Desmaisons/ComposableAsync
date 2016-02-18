using System;

namespace EasyActor.Helper
{
    internal static class TypeHelper
    {
        public static readonly Type IActorCompleteLifeCycleType = typeof(IActorCompleteLifeCycle);

        public static readonly Type IActorLifeCycleType = typeof(IActorLifeCycle);

        public static bool IsActorCompleteLifeCycleTypeOrBase(Type type)
        {
            return ((type == IActorCompleteLifeCycleType) || (type == IActorLifeCycleType));
        }
    }
}
