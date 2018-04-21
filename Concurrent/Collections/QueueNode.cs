namespace Concurrent.Collections 
{
    internal class Node<T> where T: class
    {
        public T Value { get; }
        public volatile Node<T> Next;

        public Node(T item = null) 
        {
            Value = item;
            Next = null;
        }
    }
}
