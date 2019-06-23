using System;
using System.Collections.Generic;
using System.Linq;

namespace Drones.Router
{
    public class PriorityQueue<TElement>
    {
        private readonly SortedDictionary<int, Queue<TElement>> _dictionary = new SortedDictionary<int, Queue<TElement>>();

        public int Count { get; private set; } = 0;

        public void Enqueue(TElement item, int key)
        {
            if (!_dictionary.TryGetValue(key, out Queue<TElement> queue))
            {
                queue = new Queue<TElement>();
                _dictionary.Add(key, queue);
            }
            queue.Enqueue(item);
            Count++;
        }

        public TElement Dequeue()
        {
            if (_dictionary.Count == 0)
                throw new Exception("No items to Dequeue:");
            var key = _dictionary.Keys.First();

            var queue = _dictionary[key];
            var output = queue.Dequeue();
            if (queue.Count == 0)
                _dictionary.Remove(key);

            Count--;
            return output;
        }

        public void Clear()
        {
            Count = 0;
            _dictionary.Clear();
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }

        public TElement Peek()
        {
            if (_dictionary.Count == 0)
                throw new Exception("No items to Dequeue:");
            var key = _dictionary.Keys.First();

            return _dictionary[key].Peek();
        }
    }
}
