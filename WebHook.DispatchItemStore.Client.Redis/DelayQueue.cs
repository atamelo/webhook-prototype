namespace WebHook.DispatchItemStore.Client.Redis
{
    public class DelayQueue<T>
    {
        private readonly PriorityQueue<T, DateTime> _queue;

        public DelayQueue(IReadOnlyCollection<T> items)
        {
            _queue = new();
            foreach (T item in items) {
                Enqueue(item, TimeSpan.Zero);
            }
        }

        public void Enqueue(T item, TimeSpan delay)
        {
            _queue.Enqueue(item, DateTime.Now.Add(delay));
        }

        public bool Any()
        {
            if (_queue.TryPeek(out T element, out DateTime priority)) {
                if (priority < DateTime.Now) {
                    return true;
                }
            }
            return false;
        }

        public T Dequeue() => _queue.Dequeue();
    }
}
