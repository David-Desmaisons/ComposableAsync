using System;

namespace EasyActor.Flow
{
    public class NullProgess<T> : IProgress<T>
    {
        public void Report(T value)
        {
        }
    }
}
