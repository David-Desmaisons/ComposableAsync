using System;

namespace EasyActor.Helper
{
    internal static class TypeHelper
    {
        public static readonly Type ActorCompleteLifeCycleType = typeof(IActorCompleteLifeCycle);
        public static readonly Type ActorLifeCycleType = typeof(IActorLifeCycle);

        public static bool IsActorCompleteLifeCycleTypeOrBase(Type type)
        {
            return ((type == ActorCompleteLifeCycleType) || (type == ActorLifeCycleType));
        }
    }
}
