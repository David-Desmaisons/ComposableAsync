namespace EasyActor
{
    public interface IFiberProvider
    {
        IFiber Fiber { get; }
    }
}
