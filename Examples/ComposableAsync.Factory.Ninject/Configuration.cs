using ComposableAsync.Concurrent;
using Ninject;
using Ninject.Syntax;

namespace ComposableAsync.Factory.Ninject
{
    public static class Configuration
    {
        public static IKernel GetKernel()
        {
            var kernel = new StandardKernel(new NinjectSettings { UseReflectionBasedInjection = true });
            RegisterDependency(kernel);
            return kernel;
        }

        private static void RegisterDependency(IKernel standardKernel)
        {
            var factoryBuilder = new ActorFactoryBuilder();
            var actorFactory = factoryBuilder.GetActorFactory();
            BindAsActor<IActor, Actor>(standardKernel, actorFactory).InSingletonScope();
        }

        private static IBindingWhenInNamedWithOrOnSyntax<TInterface> BindAsActor<TInterface, T>(IKernel standardKernel, IProxyFactory factory) 
            where T: TInterface
            where TInterface: class
        {
            return standardKernel.Bind<TInterface>().ToMethod(ctx => factory.Build<TInterface>(ctx.Kernel.Get<T>()));
        }
    }
}
