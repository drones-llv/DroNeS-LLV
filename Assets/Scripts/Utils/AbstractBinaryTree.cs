using System;

namespace Utils
{
    public abstract class AbstractBinaryTree<T>
    {
        private T[] _queue;
        protected Comparison<T> Comparer;
        private int Size { get; set; }
        public int Count => Size;

        protected AbstractBinaryTree(Comparison<T> comparer)
        {
            Clear();
            Comparer = comparer;
        }

        protected abstract bool Compare(T entity1, T entity2);

        public virtual void Add(T element)
        {
            if (Size == _queue.Length - 1) { DoubleSize(); }
            var pos = ++Size;
            if (Comparer != null)
            {
                while (pos > 1 && Compare(element, _queue[pos / 2]))
                {
                    _queue[pos] = _queue[pos / 2];
                    pos /= 2;
                }
            }

            _queue[pos] = element;

        }

        public virtual T Remove()
        {
            if (IsEmpty()) { throw new NullReferenceException(); }
            T head;
            if (Comparer != null)
            {
                head = _queue[1];
                var n = 1;
                for (; 2 * n < Size && !Compare(_queue[Size], _queue[ChangeNode(n)]); n = ChangeNode(n))
                {
                    _queue[n] = _queue[ChangeNode(n)];
                }
                _queue[n] = _queue[Size];
            }
            else
            {
                head = _queue[Size];
            }

            _queue[Size--] = default;
            return head;
        }

        public void Clear()
        {
            Size = 0;
            _queue = new T[2];
        }

        public bool IsEmpty()
        {
            return Size == 0;
        }

        public void ReSort(Comparison<T> comparer = null)
        {
            if (comparer != null)
            {
                Comparer = comparer;
            }

            if (Comparer == null) return;
            for (var i = Size; i >= 1; i--)
            {
                Heapify(i);
            }
        }

        private static void Swap(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        private void Heapify(int i)
        {
            while (true)
            {
                var extreme = i; // Initialize largest as root
                var l = 2 * i; // left = 2*i + 1
                var r = 2 * i + 1; // right = 2*i + 2

                // If left child is larger than root
                if (l <= Size && Compare(_queue[l], _queue[extreme]))
                {
                    extreme = l;
                }

                // If right child is larger than largest so far
                if (r <= Size && Compare(_queue[r], _queue[extreme]))
                {
                    extreme = r;
                }

                // If largest is not root
                if (extreme == i) return;
                Swap(ref _queue[i], ref _queue[extreme]);
                // Recursively heapify the affected sub-tree
                i = extreme;
            }
        }

        public T Peek() 
        {
            if (IsEmpty()) throw new NullReferenceException(); return _queue[1];
        }

        private void DoubleSize()
        {
            var temp = new T[_queue.Length * 2];

            for (var i = 0; i <= Size; i++)
            {
                temp[i] = _queue[i];
            }

            _queue = temp;
        }

        private int ChangeNode(int n)
        {
            var posChild = 2 * n;
            if (!Compare(_queue[posChild], _queue[posChild + 1])) { posChild++; }
            return posChild;
        }
    }
}
