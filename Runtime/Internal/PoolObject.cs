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
        /// Index of the object in its respective pool.
        /// </summary>
        public int Index { get; set; } = -1;

        private Transform _transform;
        private IPoolContainer _container = DummyContainer.Instance;

        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            _transform = transform;
        }

        private void OnDisable()
        {
            _container.Pool(this);
        }

        private void OnDestroy()
        {
            _container.Destroy(this);
        }

        public void SetContainer(IPoolContainer container)
        {
            _container = container ?? DummyContainer.Instance;
        }

        public void Activate(Vector3 pos, Quaternion rot, Transform parent)
        {
            _transform.SetParent(parent);
            _transform.SetPositionAndRotation(pos, rot);
            gameObject.SetActive(true);
        }
        
        private sealed class DummyContainer : IPoolContainer
        {
            public static readonly DummyContainer Instance = new DummyContainer();

            private DummyContainer() { }
            
            public void Pool(PoolObject poolObject) { }
            public void Destroy(PoolObject poolObject) { }
        }
    }
}