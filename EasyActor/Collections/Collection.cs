using System.Collections.Concurrent;

namespace EasyActor.Collections
{
    public static class Collection
    {
        public static ConcurrentBag<T> Clear<T>(this ConcurrentBag<T> @this)
        {  
            T t= default(T);
            while (!@this.IsEmpty)
            {            
                @this.TryTake(out t);
            }

            return @this;
        }
    }
}
