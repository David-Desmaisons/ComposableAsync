using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Helper
{
    internal static class TypeHelper
    {
        public static Type IActorCompleteLifeCycleType = typeof(IActorCompleteLifeCycle);

        public static Type IActorLifeCycleType = typeof(IActorLifeCycle);

        public static bool IsActorCompleteLifeCycleTypeOrBase(Type type)
        {
            return ((type == IActorCompleteLifeCycleType) || (type == IActorLifeCycleType));
        }
    }
}
