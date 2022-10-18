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
        private const int ActiveInstanceIndex = -1;

        public Action<ComponentPoolObject> Pooled;
        public Action<ComponentPoolObject> Destroyed;
        
        /// <summary>
        /// Current index of the object in its pool when it's inactive / pooled.
        /// </summary>
        public int Index { get; set; } = ActiveInstanceIndex;

        private Transform _transform;

        private void Awake() => _transform = transform;

        private void OnDisable() => Pooled(this);

        private void OnDestroy() => Destroyed(this);

        public void Activate(Vector3 pos, Quaternion rot, Transform parent)
        {
            Index = ActiveInstanceIndex;
            _transform.SetParent(parent);
            _transform.SetPositionAndRotation(pos, rot);
            gameObject.SetActive(true);
        }
    }
}
