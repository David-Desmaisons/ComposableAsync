using System.Collections.Concurrent;

namespace EasyActor.Collections
{
    public static class CollectionExtension
    {
        public static ConcurrentBag<T> Clear<T>(this ConcurrentBag<T> @this)
        {
            while (!@this.IsEmpty)
            {
                T t;
                @this.TryTake(out t);
            }

            return @this;
        }
    }
}
