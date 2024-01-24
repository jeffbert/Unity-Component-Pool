using System.Collections;
using Bert.Pool.Internal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Bert.Pool.Tests
{
    internal class PoolContainerTests
    {
        private PoolContainer<MockMonoBehaviour> _container;
        private MockMonoBehaviour _source;
        
        private MockMonoBehaviour GetInstance() => _container.GetInstance(_source, Vector3.zero, Quaternion.identity, null);
        
        [SetUp]
        public void Setup()
        {
            _container = new PoolContainer<MockMonoBehaviour>();
            _source =  new GameObject(nameof(MockMonoBehaviour)).AddComponent<MockMonoBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_source.gameObject);
        }

        [Test]
        public void CreateInstance_ReturnsInstance()
        {
            var instance = GetInstance();
            UnityEngine.Assertions.Assert.IsNotNull(instance);
            UnityEngine.Assertions.Assert.AreNotEqual(_source, instance);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance()
        {
            var instance = GetInstance();
            UnityEngine.Assertions.Assert.IsNotNull(instance);
            UnityEngine.Assertions.Assert.AreNotEqual(_source, instance);

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance_AtCorrectPosition()
        {
            var position = new Vector3(5, -10, 10);
            var instance = _container.GetInstance(_source, position, Quaternion.identity, null);
            var instancePosition = instance.transform.position;

            Assert.That(position, Is.EqualTo(instancePosition).Using(Vector3EqualityComparer.Instance));

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance_WithCorrectRotation()
        {
            var rotation = Quaternion.Euler(90, -90, 15);
            var instance = _container.GetInstance(_source, Vector3.zero, rotation, null);
            var instanceRotation = instance.transform.rotation;

            Assert.That(rotation, Is.EqualTo(instanceRotation).Using(QuaternionEqualityComparer.Instance));

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void GetInstance_ReturnsInstance_WithCorrectParent()
        {
            var parent = Utils.CreatePrimitive(PrimitiveType.Sphere).transform;
            var instance = _container.GetInstance(_source, Vector3.zero, Quaternion.identity, parent);
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
        
        [UnityTest]
        public IEnumerator ActivatingGameObject_ShouldUnPoolInstance()
        {
            var instance = GetInstance();
            instance.gameObject.SetActive(false);

            yield return null;

            instance.gameObject.SetActive(true);
            
            // New instance should be created since the previous instance was un-pooled.
            var newInstance = GetInstance();
            UnityEngine.Assertions.Assert.AreNotEqual(instance, newInstance);
            
            Object.DestroyImmediate(instance.gameObject);
            Object.DestroyImmediate(newInstance.gameObject);
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