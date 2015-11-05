using EasyActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public static class ActorBuilder
    {
        private static Lazy<IActorFactory> _ActorFactory = new Lazy<IActorFactory>(() => new ActorFactory(), true);


        public static  IActorFactory Factory
        {
            get { return _ActorFactory.Value; }
        }
    }
}
