namespace Mapbox.Unity.MeshGeneration.Data
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class ObjectPool<T>
	{
		private Queue<T> _objects;
		private Func<T> _objectGenerator;

		public ObjectPool(Func<T> objectGenerator)
		{
			_objects = new Queue<T>();
			_objectGenerator = objectGenerator ?? throw new ArgumentNullException("objectGenerator");
		}

		public T GetObject()
		{
			return _objects.Count > 0 ? _objects.Dequeue() : _objectGenerator();
		}

		public void Put(T item)
		{
			_objects.Enqueue(item);
		}

		public void Clear()
		{
			_objects.Clear();
		}

		public IEnumerable<T> GetQueue()
		{
			return _objects;
		}
	}
}

