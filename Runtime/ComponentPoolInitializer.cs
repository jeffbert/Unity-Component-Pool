using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bert.Pool
{
    /// <summary>
    /// Initializes one or many component instances in their respective <see cref="ComponentPool"/>.
    /// </summary>
    public sealed class ComponentPoolInitializer : MonoBehaviour
    {
        [System.Serializable]
        internal sealed class PoolInitializerElement
        {
            [SerializeField]
            internal Component _source;

            [SerializeField, Min(1)]
            internal int _quantity = 1;

            [SerializeField]
            internal bool _dontDestroy;

            public void Deconstruct(out Component source, out int quantity, out bool dontDestroy)
            {
                source = _source;
                quantity = _quantity;
                dontDestroy = _dontDestroy;
            }
        }

        [SerializeField]
        internal PoolInitializerElement[] _elements;

        private List<Component> _instances;

        private void Awake()
        {
            _instances = new List<Component>(_elements.Sum(element => element._quantity));

            foreach ((Component source, int quantity, bool dontDestroy) in _elements)
            {
                if (source == null)
                {
                    Debug.LogWarning($"{GetType().Name}: Couldn't create instances for null source component on \"{name}\".", this);
                    continue;
                }

                ComponentPool.CreateInstances(_instances, source, quantity, dontDestroy);
            }
        }

        private void Start()
        {
            foreach (var instance in _instances)
            {
                instance.gameObject.SetActive(false);
            }

            enabled = false;
        }
    }
}