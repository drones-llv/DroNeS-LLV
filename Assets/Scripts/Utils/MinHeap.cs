using System;

namespace Utils 
{
    public class MinHeap<T> : AbstractBinaryTree<T>
    {
        public MinHeap(Comparison<T> comparer) : base(comparer) { }

        #region Overrides
        protected override bool Compare(T entity1, T entity2)
        {
            return Comparer(entity1, entity2) <= 0;
        }
        #endregion
    }
}
