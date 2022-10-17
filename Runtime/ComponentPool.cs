using System;
using System.Collections.Generic;
using System.Reflection;
using Bert.Pool.Internal;
using UnityEngine;

namespace Bert.Pool
{
    using InitInstanceDelegate = Action<List<Component>, Component, int, Transform>;

    /// <summary>
    /// Utility class for getting pooled <see cref="Component"/> instances.
    /// </summary>
    public static class ComponentPool
    {
        private static readonly Dictionary<Type, InitInstanceDelegate> InitDelegateCache = new Dictionary<Type, InitInstanceDelegate>();

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty. 
        /// </summary>
        public static T Get<T>(T source, Transform parent = null)
            where T : Component =>
            Pool<T>.Get(source, Vector3.zero, Quaternion.identity, parent);

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public static T Get<T>(T source, Vector3 position, Transform parent = null)
            where T : Component =>
            Pool<T>.Get(source, position, Quaternion.identity, parent);

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public static T Get<T>(T source, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component =>
            Pool<T>.Get(source, position, rotation, parent);

        /// <summary>
        /// Replaces all elements of the array with instances of the <paramref name="source"/>.
        /// </summary>
        public static void GetMany<T>(T[] instances, T source, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component =>
            Pool<T>.GetMany(instances, source, position, rotation, parent);

        /// <summary>
        /// Adds instances of the <paramref name="source"/> to the <see cref="List{T}"/>.
        /// </summary>
        /// <remarks>Does not clear the <see cref="List{T}"/> before adding elements.</remarks>
        public static void GetMany<T>(List<T> instances, int quantity, T source, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component =>
            Pool<T>.GetMany(instances, quantity, source, position, rotation, parent);

        /// <summary>
        /// Creates instances of the <see cref="source"/> component, up to the requested <see cref="quantity"/>. The number of new instances
        /// created will be equal to the difference between the existing instance count and the requested <see cref="quantity"/>.
        /// </summary>
        /// <remarks>Does not clear the <paramref name="createdInstances"/> list before adding created instances.</remarks>
        public static void CreateInstances(List<Component> createdInstances, Component source, int quantity, Transform parent)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var type = source.GetType();

            if (!InitDelegateCache.TryGetValue(type, out InitInstanceDelegate action))
            {
                MethodInfo methodInfo = typeof(Pool<>)
                    .MakeGenericType(type)
                    .GetMethod(nameof(Pool<Component>.CreateInstances), BindingFlags.Public | BindingFlags.Static);

                if (methodInfo == null)
                {
                    Debug.LogError($"{nameof(ComponentPool)}.{nameof(CreateInstances)}: Method info not found for \"{type.Name}\" type for {source.name}.", source);
                    return;
                }

                action = (InitInstanceDelegate) methodInfo.CreateDelegate(typeof(InitInstanceDelegate));
                InitDelegateCache[type] = action;
            }

            action.Invoke(createdInstances, source, quantity, parent);
        }
    }

    internal static class Pool<T>
        where T : Component
    {
        // Each source object is mapped to its own distinct pool (different sources of the same type shouldn't use the same pool).
        private static readonly Dictionary<UnityEngine.Object, ComponentPoolContainer<T>> Pools = new Dictionary<UnityEngine.Object, ComponentPoolContainer<T>>();

        public static T Get(T source, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return GetPool(source).GetInstance(source, position, rotation, parent);
        }

        public static void GetMany(T[] instances, T source, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (instances == null)
                throw new ArgumentNullException(nameof(instances));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ComponentPoolContainer<T> componentPoolContainer = GetPool(source);
            componentPoolContainer.EnsureCapacity(instances.Length);
            
            for (int i = instances.Length - 1; i >= 0; --i)
            {
                instances[i] = componentPoolContainer.GetInstance(source, position, rotation, parent);
            }
        }

        public static void GetMany(List<T> instances, int quantity, T source, Vector3 position, Quaternion rotation,
            Transform parent = null)
        {
            if (instances == null)
                throw new ArgumentNullException(nameof(instances));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ComponentPoolContainer<T> componentPoolContainer = GetPool(source);
            componentPoolContainer.EnsureCapacity(quantity);
            
            for (int i = 0; i < quantity; ++i)
            {
                instances.Add(componentPoolContainer.GetInstance(source, position, rotation, parent));
            }
        }

        public static void CreateInstances(List<Component> createdInstances, Component source, int quantity, Transform parent)
        {
            if (createdInstances == null)
                throw new ArgumentNullException(nameof(createdInstances));

            ComponentPoolContainer<T> componentPoolContainer = GetPool(source);
            componentPoolContainer.EnsureCapacity(quantity);
            
            int requiredQuantity = quantity - componentPoolContainer.InstanceCount;
            for (int i = 0; i < requiredQuantity; i++)
            {
                createdInstances.Add(componentPoolContainer.CreateInstance(source, Vector3.zero, Quaternion.identity, parent));
            }
        }

        private static ComponentPoolContainer<T> GetPool(UnityEngine.Object source)
        {
            if (!Pools.TryGetValue(source, out ComponentPoolContainer<T> instancePool))
            {
                instancePool = new ComponentPoolContainer<T>();
                Pools[source] = instancePool;
            }

            return instancePool;
        }
    }
}