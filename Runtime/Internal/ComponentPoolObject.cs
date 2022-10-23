using System;
using UnityEngine;

namespace Bert.Pool.Internal
{
    /// <summary>
    /// Component added to pooled instances to react to certain Unity event callbacks.
    /// </summary>
    [AddComponentMenu("")] // Remove component from the Add Component context menu.
    internal sealed class ComponentPoolObject : MonoBehaviour
    {
        private Action<ComponentPoolObject> _pooled = EmptyCallback;
        private Action<ComponentPoolObject> _destroyed = EmptyCallback;

        private static readonly Action<ComponentPoolObject> EmptyCallback = _ => { };

        /// <summary>
        /// Index of the object in its respective pool.
        /// </summary>
        public int Index { get; set; } = -1;

        private Transform _transform;

        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            _transform = transform;
        }

        private void OnDisable()
        {
            _pooled(this);
        }

        private void OnDestroy()
        {
            _destroyed(this);
        }

        public void SetCallbacks(Action<ComponentPoolObject> pooled, Action<ComponentPoolObject> destroyed)
        {
            _pooled = pooled ?? EmptyCallback;
            _destroyed = destroyed ?? EmptyCallback;
        }

        public void Activate(Vector3 pos, Quaternion rot, Transform parent)
        {
            _transform.SetParent(parent);
            _transform.SetPositionAndRotation(pos, rot);
            gameObject.SetActive(true);
        }
    }
}