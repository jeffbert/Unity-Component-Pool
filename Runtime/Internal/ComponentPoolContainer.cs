using System.Collections.Generic;
using UnityEngine;

namespace Bert.Pool.Internal
{
    /// <summary>
    /// Pool of <see cref="ComponentPoolInstance"/> instances.
    /// </summary>
    /// <typeparam name="T">Type of component.</typeparam>
    internal sealed class ComponentPoolContainer<T>
        where T : Component
    {
        private const int DefaultCapacity = 16;

        private readonly List<(ComponentPoolInstance poolInstance, T component)> _pool = new List<(ComponentPoolInstance, T)>(DefaultCapacity);

        /// <summary>
        /// Current number of existing instances.
        /// </summary>
        public int InstanceCount { get; private set; }

        /// <summary>
        /// Ensure the pool's capacity is at a certain value to unnecessary list allocations when adding many instances to the pool. 
        /// </summary>
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

            (ComponentPoolInstance poolInstance, T component) = _pool[index];
            _pool.RemoveAt(index);
            poolInstance.Activate(position, rotation, parent);

            return component;
        }

        /// <summary>
        /// Creates an instance of the <see cref="source"/> component.
        /// </summary>
        public T CreateInstance(UnityEngine.Object source, Vector3 position, Quaternion rotation, Transform parent)
        {
            T instance = (T) UnityEngine.Object.Instantiate(source, position, rotation, parent);
            instance.gameObject.SetActive(true);

            var poolInstance = instance.gameObject.AddComponent<ComponentPoolInstance>();
            poolInstance.Pooled = obj => OnObjectPooled(obj, instance);
            poolInstance.Destroyed = OnObjectDestroyed;
            
            ++InstanceCount;

            return instance;
        }

        private void OnObjectPooled(ComponentPoolInstance obj, T component)
        {
            obj.Index = _pool.Count;
            _pool.Add((obj, component));
        }

        private void OnObjectDestroyed(ComponentPoolInstance obj)
        {
            --InstanceCount;
            
            // Un-pooled items that are destroyed don't need to be removed from the pool.
            int removeIndex = obj.Index;
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