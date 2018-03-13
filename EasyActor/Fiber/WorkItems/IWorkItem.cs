namespace EasyActor.Fiber.WorkItems
{
    internal interface IWorkItem
    {
        void Cancel();

        void Do();
    }
}
