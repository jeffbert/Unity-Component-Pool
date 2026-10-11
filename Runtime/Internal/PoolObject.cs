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
        /// Index of the object in its respective <see cref="PoolContainer"/>.
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Pooled instance of the source component.
        /// </summary>
        public Component Instance { get; private set; }

        private GameObject _gameObject;
        private Transform _transform;
        private PoolContainer _container;

        // Not done in Awake, which doesn't run until the instance is active in the hierarchy (e.g. it could be created under an inactive parent).
        public void Initialize(PoolContainer container, Component instance)
        {
            hideFlags = HideFlags.HideAndDontSave;
            _gameObject = instance.gameObject;
            _transform = instance.transform;
            _container = container;
            Instance = instance;
        }

        /// <summary>
        /// Stop notifying the container of this instance's state changes.
        /// </summary>
        public void Detach() => _container = null;

        private void OnEnable()
        {
            // Alert the container when the object is enabled in case it's enabled without going through the component pool API.
            // Un-pooling will be ignored until the container is set.
            _container?.UnPool(this);
        }

        private void OnDisable() => _container?.Pool(this);

        private void OnDestroy() => _container?.Remove(this);

        public void Activate(Vector3 position, Quaternion rotation, Transform parent)
        {
            // The world position and rotation are set right after, so they don't need to be preserved when changing parents.
            // It's cheaper, and it keeps the instance's local scale instead of carrying over its previous parent's scale.
            _transform.SetParent(parent, false);
            _transform.SetPositionAndRotation(position, rotation);
            _gameObject.SetActive(true);
        }
    }
}
