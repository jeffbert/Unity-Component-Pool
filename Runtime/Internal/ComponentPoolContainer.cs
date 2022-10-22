using System.Collections.Generic;
using UnityEngine;

namespace Bert.Pool.Internal
{
    /// <summary>
    /// Pool of <see cref="ComponentPoolObject"/> instances.
    /// </summary>
    /// <typeparam name="T">Type of component.</typeparam>
    internal sealed class ComponentPoolContainer<T>
        where T : Component
    {
        private const int DefaultCapacity = 16;

        private readonly List<(ComponentPoolObject poolInstance, T component)> _pool = new List<(ComponentPoolObject, T)>(DefaultCapacity);

        /// <summary>
        /// Current number of existing instances.
        /// </summary>
        public int InstanceCount { get; private set; }

        /// <summary>
        /// Ensure the pool's capacity is at least the specified <paramref name="capacity"/> to
        /// avoid unnecessary list allocations when adding many instances to the pool. 
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        public void EnsureCapacity(int capacity)
        {
            if (_pool.Capacity < capacity)
            {
                _pool.Capacity = capacity;
            }
        }

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public T GetInstance(T source, Vector3 position, Quaternion rotation, Transform parent)
        {
            int index = _pool.Count - 1;
            if (index == -1)
            {
                // Empty pool.
                return CreateInstance(source, position, rotation, parent);
            }

            (ComponentPoolObject poolObject, T instance) = _pool[index];
            _pool.RemoveAt(index);
            poolObject.Activate(position, rotation, parent);

            return instance;
        }

        /// <summary>
        /// Creates an instance of the <see cref="source"/> component.
        /// </summary>
        public T CreateInstance(Object source, Vector3 position, Quaternion rotation, Transform parent)
        {
            T instance = (T) Object.Instantiate(source, position, rotation, parent);
            instance.gameObject.SetActive(true);

            var poolObject = instance.gameObject.AddComponent<ComponentPoolObject>();
            poolObject.Pooled = obj => OnObjectPooled(obj, instance);
            poolObject.Destroyed = OnObjectDestroyed;
            
            ++InstanceCount;

            return instance;
        }

        /// <summary>
        /// Destroys all instances in the pool.
        /// </summary>
        public void DestroyInstances()
        {
            foreach ((ComponentPoolObject poolInstance, _) in _pool)
            {
                poolInstance.DestroyInstance();
            }
            
            _pool.Clear();
            _pool.Capacity = DefaultCapacity;
        }

        private void OnObjectPooled(ComponentPoolObject poolObject, T instance)
        {
            poolObject.Index = _pool.Count;
            _pool.Add((poolObject, instance));
        }

        private void OnObjectDestroyed(ComponentPoolObject poolObject)
        {
            --InstanceCount;
            
            // Un-pooled/active items that are destroyed don't need to be removed from the pool.
            int removeIndex = poolObject.Index;
            if (removeIndex < 0)
                return;

            // Replace the destroyed instance with the last one in the list.
            int lastIndex = _pool.Count - 1;
            _pool[removeIndex] = _pool[lastIndex];
            _pool[removeIndex].poolInstance.Index = removeIndex;
            _pool.RemoveAt(lastIndex);
        }
    }
}