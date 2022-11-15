using UnityEngine;

namespace Bert.Pool.Internal
{
    /// <summary>
    /// Component added to pooled instances to react to certain Unity event callbacks.
    /// </summary>
    [AddComponentMenu("")] // Remove component from the Add Component context menu.
    internal sealed class PoolObject : MonoBehaviour
    {
        /// <summary>
        /// Index of the object in its respective <see cref="PoolContainer{T}"/>.
        /// </summary>
        public int Index { get; set; } = -1;

        private Transform _transform;
        private IPoolObjectManager _manager;

        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            _transform = transform;
        }

        private void OnDisable()
        {
            _manager?.Pool(this);
        }

        private void OnDestroy()
        {
            _manager?.Destroy(this);
        }

        public void SetManager(IPoolObjectManager manager)
        {
            _manager = manager;
        }

        public void Activate(Vector3 pos, Quaternion rot, Transform parent)
        {
            _transform.SetParent(parent);
            _transform.SetPositionAndRotation(pos, rot);
            gameObject.SetActive(true);
        }
    }
}