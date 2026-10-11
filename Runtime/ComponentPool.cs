using System;
using System.Collections.Generic;
using Bert.Pool.Internal;
#if UNITY_7000_0_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bert.Pool
{
#if UNITY_6000_5_OR_NEWER
    using SourceId = EntityId;
#else
    using SourceId = Int32;
#endif

    /// <summary>
    /// Utility class for getting pooled <see cref="Component"/> instances.
    /// </summary>
    public static partial class ComponentPool
    {
        // Each source object is mapped to its own distinct pool (different sources of the same type shouldn't use the same pool).
        // Pools are keyed by source only, so a source's instances are shared regardless of the type argument used to get them.
#if UNITY_7000_0_OR_NEWER
        [NoAutoStaticsCleanup] // Cleared in ResetStatics.
#endif
        private static readonly Dictionary<SourceId, PoolContainer> s_containers = new();

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public static T Get<T>(T source, Transform parent = null)
            where T : Component =>
            Get(source, Vector3.zero, Quaternion.identity, parent);

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public static T Get<T>(T source, Vector3 position, Transform parent = null)
            where T : Component =>
            Get(source, position, Quaternion.identity, parent);

        /// <summary>
        /// Get an instance of the <paramref name="source"/>. A new instance will be created if the pool is empty.
        /// </summary>
        public static T Get<T>(T source, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Couldn't get instance for null source component.");
            }

            return GetContainer(source).GetInstance(source, position, rotation, parent);
        }

        /// <summary>
        /// Replaces all elements of the array with instances of the <paramref name="source"/>.
        /// </summary>
        public static void GetMany<T>(T[] instances, T source, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            if (instances == null)
            {
                throw new ArgumentNullException(nameof(instances));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Couldn't get instances for null source component.");
            }

            PoolContainer container = GetContainer(source);
            container.Reserve(instances.Length);

            for (int i = instances.Length - 1; i >= 0; --i)
            {
                instances[i] = container.GetInstance(source, position, rotation, parent);
            }
        }

        /// <summary>
        /// Adds instances of the <paramref name="source"/> to the specified collection.
        /// </summary>
        /// <remarks>Does not clear the <paramref name="instances"/> collection before adding elements.</remarks>
        public static void GetMany<T>(ICollection<T> instances, T source, int quantity, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            if (instances == null)
            {
                throw new ArgumentNullException(nameof(instances));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Couldn't get instances for null source component.");
            }

            PoolContainer container = GetContainer(source);
            container.Reserve(quantity);

            for (int i = 0; i < quantity; ++i)
            {
                instances.Add(container.GetInstance(source, position, rotation, parent));
            }
        }

        /// <summary>
        /// Creates instances of the <paramref name="source"/> component, up to the requested <paramref name="quantity"/>. The number of new instances
        /// created will be equal to the difference between the existing instance count and the requested <paramref name="quantity"/>.
        /// </summary>
        /// <remarks>Does not clear the <paramref name="createdInstances"/> collection before adding created instances.</remarks>
        public static void CreateInstances(ICollection<Component> createdInstances, Component source, int quantity, bool dontDestroy)
        {
            if (createdInstances == null)
            {
                throw new ArgumentNullException(nameof(createdInstances));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Couldn't create instances for null source component.");
            }

            PoolContainer container = GetContainer(source);
            container.EnsureCapacity(quantity);

            for (int i = container.InstanceCount; i < quantity; ++i)
            {
                Component instance = container.CreateInstance(source, Vector3.zero, Quaternion.identity, null);

                if (dontDestroy)
                {
                    Object.DontDestroyOnLoad(instance);
                }

                createdInstances.Add(instance);
            }
        }

        /// <summary>
        /// Destroys all <typeparamref name="T"/> component instances that were created by the <see cref="ComponentPool"/>.
        /// </summary>
        /// <remarks>Instances of sources whose type derives from <typeparamref name="T"/> are destroyed as well.</remarks>
        /// <typeparam name="T">Type of component that will have its instances destroyed.</typeparam>
        public static void DestroyInstances<T>()
            where T : Component
        {
            var destroyedSourceIds = new List<SourceId>();

            foreach (KeyValuePair<SourceId, PoolContainer> pair in s_containers)
            {
                if (typeof(T).IsAssignableFrom(pair.Value.SourceType))
                {
                    pair.Value.DestroyInstances();
                    destroyedSourceIds.Add(pair.Key);
                }
            }

            foreach (SourceId sourceId in destroyedSourceIds)
            {
                s_containers.Remove(sourceId);
            }
        }

        /// <summary>
        /// Destroys all instances of the specified <paramref name="source"/> component that were created by the <see cref="ComponentPool"/>.
        /// </summary>
        /// <typeparam name="T">Type of the source component.</typeparam>
        /// <param name="source">Source component that will have its instances destroyed.</param>
        public static void DestroySourceInstances<T>(T source)
            where T : Component
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Couldn't destroy instances for null source component.");
            }

            SourceId sourceId = GetSourceId(source);
            if (!s_containers.TryGetValue(sourceId, out PoolContainer container))
            {
                return;
            }

            container.DestroyInstances();
            s_containers.Remove(sourceId);
        }

        // Statics survive entering Play mode when the domain isn't reloaded (always the case in Unity 7, optional before), so the
        // pools of the previous Play mode session, whose instances were destroyed on exit, are cleared here.
#if UNITY_7000_0_OR_NEWER
        [OnEnteringPlayMode]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        internal static void ResetStatics() => s_containers.Clear();

        private static PoolContainer GetContainer(Component source)
        {
            SourceId sourceId = GetSourceId(source);
            if (!s_containers.TryGetValue(sourceId, out PoolContainer container))
            {
                container = new PoolContainer(source.GetType());
                s_containers.Add(sourceId, container);
            }

            return container;
        }

#if UNITY_6000_5_OR_NEWER
        private static SourceId GetSourceId(Component source) => source.GetEntityId();
#else
        private static SourceId GetSourceId(Component source) => source.GetInstanceID();
#endif
    }
}
