namespace EasyActor.Examples
{
    internal class PingPongerSimple : IPingPonger
    {
        public int Count { get; private set; }
        public string Name { get; }

        internal IPingPonger Ponger { get; set; }

        public PingPongerSimple(string iName)
        {
            Name = iName;
        }

        public void Ping()
        {
            Count++;
            Ponger?.Ping();
        }
    }
}
