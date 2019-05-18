namespace ComposableAsync.Concurrent.Collections 
{
    internal class Node<T> where T: class
    {
        internal T Value { get; }
        internal volatile Node<T> Next;

        public Node(T item = null) 
        {
            Value = item;
            Next = null;
        }
    }
}
