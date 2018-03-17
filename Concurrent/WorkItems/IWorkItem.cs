namespace Concurrent.WorkItems
{
    public interface IWorkItem
    {
        void Cancel();

        void Do();
    }
}
