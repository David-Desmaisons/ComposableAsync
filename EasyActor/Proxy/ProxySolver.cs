namespace EasyActor.Proxy 
{
    internal struct ProxyFiberSolver 
    {
        public bool Continue { get; }

        public InvocationOnDispatcher Transform { get; }

        public ProxyFiberSolver(InvocationOnDispatcher transform, bool @continue)
        {
            Continue = @continue;
            Transform = transform;
        }
    }
}
