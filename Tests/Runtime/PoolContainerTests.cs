using System.Collections;
using Bert.Pool.Internal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Bert.Pool.Tests
{
    /// <summary>
    /// Base class for testing different types of component pool containers.
    /// </summary>
    /// <typeparam name="T">Pool container's component type.</typeparam>
    public abstract class PoolContainerTests<T>
        where T : Component
    {
        private PoolContainer<T> _container;
        
        protected T Source { get; private set; }
        
        /// <summary>
        /// Returns an instance from the <see cref="PoolContainer{T}"/>.
        /// </summary>
        /// <returns>Instance of the component.</returns>
        protected T GetInstance() => _container.GetInstance(Source, Vector3.zero, Quaternion.identity, null);
        
        protected virtual T InitializeSourceComponent(GameObject sourceGameObject)
        {
            return sourceGameObject.TryGetComponent(out T component) ? component : sourceGameObject.AddComponent<T>();
        }

        [SetUp]
        public void Setup()
        {
            _container = new PoolContainer<T>();

            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Source = InitializeSourceComponent(obj);
            
            UnityEngine.Assertions.Assert.IsNotNull(Source, $"Source component was null. {GetType().Name}.{nameof(InitializeSourceComponent)} must return a non-null component.");
        }

        [TearDown]
        public void TearDown()
        {
            if (Source != null)
            {
                Object.DestroyImmediate(Source.gameObject);
            }
        }

        [Test]
        public void CreateInstance_ReturnsInstance()
        {
            var instance = GetInstance();
            UnityEngine.Assertions.Assert.IsNotNull(instance);
            UnityEngine.Assertions.Assert.AreNotEqual(Source, instance);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance()
        {
            var instance = GetInstance();
            UnityEngine.Assertions.Assert.IsNotNull(instance);
            UnityEngine.Assertions.Assert.AreNotEqual(Source, instance);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance_AtCorrectPosition()
        {
            var position = new Vector3(5, -10, 10);
            var instance = _container.GetInstance(Source, position, Quaternion.identity, null);
            var instancePosition = instance.transform.position;

            Assert.That(position, Is.EqualTo(instancePosition).Using(Vector3EqualityComparer.Instance));

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance_WithCorrectRotation()
        {
            var rotation = Quaternion.Euler(90, -90, 15);
            var instance = _container.GetInstance(Source, Vector3.zero, rotation, null);
            var instanceRotation = instance.transform.rotation;

            Assert.That(rotation, Is.EqualTo(instanceRotation).Using(QuaternionEqualityComparer.Instance));

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance_WithCorrectParent()
        {
            var parent = Utils.CreatePrimitive(PrimitiveType.Sphere).transform;
            var instance = _container.GetInstance(Source, Vector3.zero, Quaternion.identity, parent);
            var instanceParent = instance.transform.parent;

            UnityEngine.Assertions.Assert.AreEqual(parent, instanceParent);

            Object.DestroyImmediate(instance.gameObject);
            Object.DestroyImmediate(parent.gameObject);
        }

        [Test]
        public void DeactivatingGameObject_PoolsInstance()
        {
            var instance = GetInstance();
            instance.gameObject.SetActive(false);

            UnityEngine.Assertions.Assert.AreEqual(instance, GetInstance());

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsNewInstance_WhenPoolEmpty()
        {
            var instance1 = GetInstance();
            Assert.That(_container.InstanceCount, Is.EqualTo(1));
            var instance2 = GetInstance();
            Assert.That(_container.InstanceCount, Is.EqualTo(2));
            UnityEngine.Assertions.Assert.AreNotEqual(instance1, instance2);

            Object.DestroyImmediate(instance1.gameObject);
            Object.DestroyImmediate(instance2.gameObject);
        }

        [UnityTest]
        public IEnumerator ObjectDestroy_RemovesInstance_FromPool()
        {
            var instance = GetInstance();
            Assert.That(_container.InstanceCount, Is.EqualTo(1));
            Object.Destroy(instance.gameObject);
            yield return null;
            Assert.That(_container.InstanceCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator DestroyInstances_ShouldDestroyInstances()
        {
            var instance1 = GetInstance();
            var instance2 = GetInstance();

            _container.DestroyInstances();

            yield return null;

            UnityEngine.Assertions.Assert.IsNull(instance1);
            UnityEngine.Assertions.Assert.IsNull(instance2);
            Assert.That(_container.InstanceCount, Is.EqualTo(0));
        }
    }
}