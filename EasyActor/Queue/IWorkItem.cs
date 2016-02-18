namespace EasyActor.Queue
{
    internal interface IWorkItem
    {
        void Cancel();

        void Do();
    }
}
