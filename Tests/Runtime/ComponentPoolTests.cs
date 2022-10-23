using System.Collections;
using System.Collections.Generic;
using Bert.Pool.Internal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Bert.Pool.Tests
{
    internal sealed class ComponentPoolTests
    {
        private MockMonoBehaviour _source;
        
        [SetUp]
        public void Setup()
        {
            _source = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<MockMonoBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_source.gameObject);
        }
        
        [Test]
        public void Get_ReturnsInstance()
        {
            var instance = ComponentPool.Get(_source);
            UnityEngine.Assertions.Assert.IsNotNull(instance);
            UnityEngine.Assertions.Assert.AreNotEqual(_source, instance);
            
            Object.DestroyImmediate(instance.gameObject);
        }
        
        [Test]
        public void Get_ReturnsDifferentInstance_ForDifferentSource()
        {
            var instance = ComponentPool.Get(_source);
            instance.gameObject.SetActive(false);
            
            var otherSource = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<MockMonoBehaviour>();
            var otherInstance = ComponentPool.Get(otherSource);
            
            UnityEngine.Assertions.Assert.AreNotEqual(instance, otherInstance);
            
            Object.DestroyImmediate(instance.gameObject);
            Object.DestroyImmediate(otherSource.gameObject);
            Object.DestroyImmediate(otherInstance.gameObject);
        }
        
        [UnityTest]
        public IEnumerator DestroyInstances_DestroysAll_ComponentTypeInstances()
        {
            var otherSource = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<MockMonoBehaviour>();
            
            var instance1 = ComponentPool.Get(_source);
            var instance2 = ComponentPool.Get(otherSource);
            
            ComponentPool.DestroyInstances<MockMonoBehaviour>();

            yield return null;
            
            UnityEngine.Assertions.Assert.IsNull(instance1);
            UnityEngine.Assertions.Assert.IsNull(instance2);
            
            Object.DestroyImmediate(otherSource.gameObject);
        }

        [UnityTest]
        public IEnumerator DestroyInstances_DoesNotDestroy_Source()
        {
            _ = ComponentPool.Get(_source);
            ComponentPool.DestroyInstances<MockMonoBehaviour>();
            yield return null;
            UnityEngine.Assertions.Assert.IsNotNull(_source);
        }

        [UnityTest]
        public IEnumerator DestroySourceInstances_DestroysAll_SourceInstances()
        {
            var instance = ComponentPool.Get(_source);
            ComponentPool.DestroySourceInstances(_source);
            yield return null;
            UnityEngine.Assertions.Assert.IsNull(instance);
        }
        
        [UnityTest]
        public IEnumerator DestroySourceInstances_DoesNotDestroy_OtherSourceInstances()
        {
            var otherSource = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<MockMonoBehaviour>();
            var otherInstance = ComponentPool.Get(otherSource);
            
            ComponentPool.DestroySourceInstances(_source);
            
            yield return null;
            
            UnityEngine.Assertions.Assert.IsNotNull(otherInstance);
            
            Object.DestroyImmediate(otherSource.gameObject);
            Object.DestroyImmediate(otherInstance.gameObject);
        }

        [Test]
        public void GetMany_FillsArray_WithInstances()
        {
            const int instanceCount = 3;
            var instances = new MockMonoBehaviour[instanceCount];
            
            ComponentPool.GetMany(instances, _source, Vector3.zero, Quaternion.identity);

            foreach (var instance in instances)
            {
                UnityEngine.Assertions.Assert.IsNotNull(instance);
                Object.DestroyImmediate(instance.gameObject);
            }
        }

        [Test]
        public void GetMany_AddsInstances_ToList()
        {
            const int instanceCount = 3;
            var instances = new List<MockMonoBehaviour>(instanceCount);
            
            ComponentPool.GetMany(instances, instanceCount, _source, Vector3.zero, Quaternion.identity);

            Assert.That(instances.Count, Is.EqualTo(instanceCount));

            foreach (var instance in instances)
            {
                UnityEngine.Assertions.Assert.IsNotNull(instance);
                Object.DestroyImmediate(instance.gameObject);
            }
        }
        
        [Test]
        public void GetMany_AppendsInstances_ToExistingList()
        {
            const int instanceCount = 1;
            var instances = new List<MockMonoBehaviour>();
            ComponentPool.GetMany(instances, instanceCount, _source, Vector3.zero, Quaternion.identity);

            const int appendInstanceCount = 2;
            ComponentPool.GetMany(instances, appendInstanceCount, _source, Vector3.zero, Quaternion.identity);
            
            Assert.That(instances.Count, Is.EqualTo(instanceCount + appendInstanceCount));

            foreach (var instance in instances)
            {
                UnityEngine.Assertions.Assert.IsNotNull(instance);
                Object.DestroyImmediate(instance.gameObject);
            }
        }
    }
}
