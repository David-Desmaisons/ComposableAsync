using System;
namespace EasyActor
{
    public interface IActorFactory
    {
        T Build<T>(T concrete) where T : class;
    }
}
