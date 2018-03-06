using System.Collections.Concurrent;

namespace EasyActor.Collections
{
    public static class CollectionExtension
    {
        public static ConcurrentBag<T> Clear<T>(this ConcurrentBag<T> @this)
        {  
            var t= default(T);
            while (!@this.IsEmpty)
            {            
                @this.TryTake(out t);
            }

            return @this;
        }
    }
}
