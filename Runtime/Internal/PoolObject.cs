using System;
using UnityEngine;

namespace Bert.Pool.Internal
{
    /// <summary>
    /// Component added to pooled instances to react to certain Unity event callbacks.
    /// </summary>
    [AddComponentMenu("")] // Remove component from the Add Component context menu.
    internal sealed class PoolObject : MonoBehaviour
    {
        private static readonly Action<PoolObject> EmptyCallback = _ => { };

        /// <summary>
        /// Index of the object in its respective pool.
        /// </summary>
        public int Index { get; set; } = -1;

        private Action<PoolObject> _pooled = EmptyCallback;
        private Action<PoolObject> _destroyed = EmptyCallback;
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

        public void SetCallbacks(Action<PoolObject> pooled, Action<PoolObject> destroyed)
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