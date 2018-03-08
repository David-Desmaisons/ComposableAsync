namespace EasyActor.Fiber
{
    internal interface IWorkItem
    {
        void Cancel();

        void Do();
    }
}
