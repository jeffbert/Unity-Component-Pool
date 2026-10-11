using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bert.Pool.Internal
{
    /// <summary>
    /// Pool of a source component's instances.
    /// </summary>
    internal sealed class PoolContainer
    {
        private const int DefaultCapacity = 8;

        // Active instances are kept at the front of the list, while pooled instances are at the back (separated by _pooledCount).
        // Every operation checks which side an instance is on, so callbacks received in an unexpected order can't corrupt the pool.
        private readonly List<PoolObject> _instances = new(DefaultCapacity);
        private int _pooledCount;

#if UNITY_EDITOR
        private readonly string _instanceNameSuffix;
#endif

        public PoolContainer(Type sourceType)
        {
            SourceType = sourceType;

#if UNITY_EDITOR
            _instanceNameSuffix = $" (Pool: {sourceType.Name})";
#endif
        }

        /// <summary>
        /// Runtime type of the source component.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Total number of instances created by the pool.
        /// </summary>
        public int InstanceCount => _instances.Count;

        /// <summary>
        /// Number of instances available in the pool.
        /// </summary>
        public int PooledCount => _pooledCount;

        private int FirstPooledIndex => _instances.Count - _pooledCount;

        /// <summary>
        /// Ensure the pool's capacity is at least the specified <paramref name="capacity"/> to
        /// avoid unnecessary list allocations when adding many instances to the pool.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        public void EnsureCapacity(int capacity)
        {
            if (_instances.Capacity < capacity)
            {
                _instances.Capacity = Math.Max(capacity, _instances.Capacity * 2);
            }
        }

        /// <summary>
        /// Ensure <paramref name="quantity"/> instances can be retrieved without growing the pool more than once.
        /// </summary>
        public void Reserve(int quantity) => EnsureCapacity(_instances.Count + Math.Max(0, quantity - _pooledCount));

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public T GetInstance<T>(T source, Vector3 position, Quaternion rotation, Transform parent)
            where T : Component =>
            // Instances are clones of the source, so they're always of type T.
            (T)GetOrCreateInstance(source, position, rotation, parent);

        private Component GetOrCreateInstance(Component source, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (_pooledCount == 0)
            {
                return CreateInstance(source, position, rotation, parent);
            }

            // The first pooled instance sits right after the active ones, so un-pooling it only moves the boundary.
            // It's un-pooled before being activated: OnEnable doesn't fire under an inactive parent (the instance would be
            // handed out again), and OnEnable callbacks of other components could retrieve an instance first.
            PoolObject poolObject = _instances[FirstPooledIndex];
            --_pooledCount;
            poolObject.Activate(position, rotation, parent);

            return poolObject.Instance;
        }

        /// <summary>
        /// Creates an active instance of the <paramref name="source"/> component.
        /// </summary>
        public Component CreateInstance(Component source, Vector3 position, Quaternion rotation, Transform parent)
        {
            Component instance = Object.Instantiate(source, position, rotation, parent);
            GameObject instanceObject = instance.gameObject;
            instanceObject.SetActive(true);

#if UNITY_EDITOR
            instanceObject.name = GetInstanceName(instanceObject.name);
#endif

            var poolObject = instanceObject.AddComponent<PoolObject>();
            poolObject.Initialize(this, instance);

            // Move the new instance from the end of the list to the end of the active instances.
            int index = _instances.Count;
            poolObject.Index = index;
            _instances.Add(poolObject);
            Swap(index, index - _pooledCount);

            return instance;
        }

        /// <summary>
        /// Destroys all instances in the pool.
        /// </summary>
        public void DestroyInstances()
        {
            foreach (PoolObject poolObject in _instances)
            {
                // Instances destroyed without ever being active (e.g. created under an inactive parent) don't receive
                // OnDestroy, so they're never removed from the list.
                if (poolObject == null)
                {
                    continue;
                }

                poolObject.Detach();
                Object.Destroy(poolObject.gameObject);
            }

            _instances.Clear();
            _pooledCount = 0;
        }

        /// <summary>
        /// Remove the instance from the pool. Does nothing if the instance is already active.
        /// </summary>
        public void UnPool(PoolObject poolObject)
        {
            int firstPooledIndex = FirstPooledIndex;
            if (poolObject.Index < firstPooledIndex)
            {
                return;
            }

            Swap(poolObject.Index, firstPooledIndex);
            --_pooledCount;
        }

        /// <summary>
        /// Add the instance back to the pool. Does nothing if the instance is already pooled.
        /// </summary>
        public void Pool(PoolObject poolObject)
        {
            int lastActiveIndex = FirstPooledIndex - 1;
            if (poolObject.Index > lastActiveIndex)
            {
                return;
            }

            Swap(poolObject.Index, lastActiveIndex);
            ++_pooledCount;
        }

        /// <summary>
        /// Remove a destroyed instance from the container.
        /// </summary>
        public void Remove(PoolObject poolObject)
        {
            int index = poolObject.Index;
            int firstPooledIndex = FirstPooledIndex;

            if (index < firstPooledIndex)
            {
                // Move the active instance to the end of the active instances, so the last instance can take its place.
                int lastActiveIndex = firstPooledIndex - 1;
                Swap(index, lastActiveIndex);
                index = lastActiveIndex;
            }
            else
            {
                --_pooledCount;
            }

            // Replace the removed instance with the last one in the list.
            int lastIndex = _instances.Count - 1;
            PoolObject lastPoolObject = _instances[lastIndex];
            _instances[index] = lastPoolObject;
            lastPoolObject.Index = index;
            _instances.RemoveAt(lastIndex);

            poolObject.Index = -1;
        }

        private void Swap(int x, int y)
        {
            if (x == y)
            {
                return;
            }

            PoolObject poolObjectX = _instances[x];
            PoolObject poolObjectY = _instances[y];

            _instances[x] = poolObjectY;
            _instances[y] = poolObjectX;

            poolObjectY.Index = x;
            poolObjectX.Index = y;
        }

#if UNITY_EDITOR
        private string GetInstanceName(string name)
        {
            const string CloneSuffix = "(Clone)";

            if (name.EndsWith(CloneSuffix, StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - CloneSuffix.Length);
            }

            return name + _instanceNameSuffix;
        }
#endif
    }
}
