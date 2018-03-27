namespace EasyActor.Examples
{
    internal class PingPongerSimple : IPingPonger
    {
        public int Count { get; private set; }
        public string Name { get; }

        internal IPingPonger Ponger { get; set; }

        public PingPongerSimple(string name)
        {
            Name = name;
        }

        public void Ping()
        {
            Count++;
            Ponger?.Ping();
        }
    }
}
