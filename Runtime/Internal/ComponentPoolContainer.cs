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
        private const int DefaultCapacity = 8;

        private readonly List<(ComponentPoolObject poolInstance, T component)> _instances = new List<(ComponentPoolObject, T)>(DefaultCapacity);
        private int _poolCount;

        /// <summary>
        /// Total number of instances created by the pool.
        /// </summary>
        public int InstanceCount => _instances.Count;

        /// <summary>
        /// Ensure the pool's capacity is at least the specified <paramref name="capacity"/> to
        /// avoid unnecessary list allocations when adding many instances to the pool. 
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        public void EnsureCapacity(int capacity)
        {
            if (_instances.Capacity < capacity)
            {
                _instances.Capacity = capacity;
            }
        }

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public T GetInstance(T source, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (_poolCount == 0)
                return CreateInstance(source, position, rotation, parent);

            int index = _instances.Count - _poolCount;
            --_poolCount;
            
            (ComponentPoolObject poolObject, T instance) = _instances[index];
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
            poolObject.SetCallbacks(OnObjectPooled, OnObjectDestroyed);
            poolObject.Index = _instances.Count;
            
            _instances.Add((poolObject, instance));

            return instance;
        }

        /// <summary>
        /// Destroys all instances in the pool.
        /// </summary>
        public void DestroyInstances()
        {
            foreach ((ComponentPoolObject poolInstance, T component) in _instances)
            {
                poolInstance.SetCallbacks(null, null);
                Object.Destroy(component.gameObject);
            }

            _instances.Clear();
            _poolCount = 0;
        }

        private void OnObjectPooled(ComponentPoolObject poolObject)
        {
            ++_poolCount;
            int targetIndex = _instances.Count - _poolCount;
            int activeIndex = poolObject.Index;
            
            // Swap pooled object with last active instance in the list.
            (_instances[activeIndex], _instances[targetIndex]) = (_instances[targetIndex], _instances[activeIndex]);
            
            _instances[activeIndex].poolInstance.Index = activeIndex;
            _instances[targetIndex].poolInstance.Index = targetIndex;
        }

        private void OnObjectDestroyed(ComponentPoolObject poolObject)
        {
            // Instances are always pooled before they're destroyed.
            --_poolCount;
            
            int destroyedIndex = poolObject.Index;
            int lastIndex = _instances.Count - 1;

            // Replace the destroyed instance with the last one in the list.
            _instances[destroyedIndex] = _instances[lastIndex];
            _instances[destroyedIndex].poolInstance.Index = destroyedIndex;
            _instances.RemoveAt(lastIndex);
        }
    }
}