namespace EasyActor.Fiber.WorkItems
{
    public interface IWorkItem
    {
        void Cancel();

        void Do();
    }
}
