#if TEST_FRAMEWORK_EXISTS

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Bert.Pool.Tests
{
    internal sealed class ComponentPoolTests
    {
        private FakeMonoBehaviour _source;

        [SetUp]
        public void Setup()
        {
            _source = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<FakeMonoBehaviour>();
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

            var otherSource = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<FakeMonoBehaviour>();
            var otherInstance = ComponentPool.Get(otherSource);

            UnityEngine.Assertions.Assert.AreNotEqual(instance, otherInstance);

            Object.DestroyImmediate(instance.gameObject);
            Object.DestroyImmediate(otherSource.gameObject);
            Object.DestroyImmediate(otherInstance.gameObject);
        }

        [UnityTest]
        public IEnumerator DestroyInstances_DestroysAll_ComponentTypeInstances()
        {
            var otherSource = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<FakeMonoBehaviour>();

            var instance1 = ComponentPool.Get(_source);
            var instance2 = ComponentPool.Get(otherSource);

            ComponentPool.DestroyInstances<FakeMonoBehaviour>();

            yield return null;

            UnityEngine.Assertions.Assert.IsNull(instance1);
            UnityEngine.Assertions.Assert.IsNull(instance2);

            Object.DestroyImmediate(otherSource.gameObject);
        }

        [UnityTest]
        public IEnumerator DestroyInstances_DoesNotDestroy_Source()
        {
            _ = ComponentPool.Get(_source);
            ComponentPool.DestroyInstances<FakeMonoBehaviour>();
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
            var otherSource = Utils.CreatePrimitive(PrimitiveType.Sphere).AddComponent<FakeMonoBehaviour>();
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
            var instances = new FakeMonoBehaviour[instanceCount];

            ComponentPool.GetMany(instances, _source, Vector3.zero, Quaternion.identity);

            foreach (var instance in instances)
            {
                UnityEngine.Assertions.Assert.IsNotNull(instance);
                Object.DestroyImmediate(instance.gameObject);
            }
        }

        [Test]
        public void GetMany_AddsInstances_ToHashSet()
        {
            const int instanceCount = 3;
            var instances = new HashSet<FakeMonoBehaviour>(instanceCount);

            ComponentPool.GetMany(instances, _source, instanceCount, Vector3.zero, Quaternion.identity);

            Assert.That(instances.Count, Is.EqualTo(instanceCount));

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
            var instances = new List<FakeMonoBehaviour>(instanceCount);

            ComponentPool.GetMany(instances, _source, instanceCount, Vector3.zero, Quaternion.identity);

            Assert.That(instances.Count, Is.EqualTo(instanceCount));

            foreach (var instance in instances)
            {
                UnityEngine.Assertions.Assert.IsNotNull(instance);
                Object.DestroyImmediate(instance.gameObject);
            }
        }

        [Test]
        public void GetMany_AppendsInstances_ToExistingHashSet()
        {
            const int instanceCount = 1;
            var instances = new HashSet<FakeMonoBehaviour>();
            ComponentPool.GetMany(instances, _source, instanceCount, Vector3.zero, Quaternion.identity);

            const int appendInstanceCount = 2;
            ComponentPool.GetMany(instances, _source, appendInstanceCount, Vector3.zero, Quaternion.identity);

            Assert.That(instances.Count, Is.EqualTo(instanceCount + appendInstanceCount));

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
            var instances = new List<FakeMonoBehaviour>();
            ComponentPool.GetMany(instances, _source, instanceCount, Vector3.zero, Quaternion.identity);

            const int appendInstanceCount = 2;
            ComponentPool.GetMany(instances, _source, appendInstanceCount, Vector3.zero, Quaternion.identity);

            Assert.That(instances.Count, Is.EqualTo(instanceCount + appendInstanceCount));

            foreach (var instance in instances)
            {
                UnityEngine.Assertions.Assert.IsNotNull(instance);
                Object.DestroyImmediate(instance.gameObject);
            }
        }

        [Test]
        public void Get_WithInactiveParent_ReturnsDifferentInstances()
        {
            var pooled1 = ComponentPool.Get(_source);
            var pooled2 = ComponentPool.Get(_source);
            pooled1.gameObject.SetActive(false);
            pooled2.gameObject.SetActive(false);

            var inactiveParent = CreateInactiveParent();

            var instance1 = ComponentPool.Get(_source, inactiveParent.transform);
            var instance2 = ComponentPool.Get(_source, inactiveParent.transform);

            Assert.That(instance1, Is.Not.SameAs(instance2), "An instance retrieved under an inactive parent must not stay in the pool.");

            DestroyGameObjects(pooled1, pooled2, inactiveParent.transform);
        }

        [Test]
        public void ActivatingParent_OfInstanceCreatedUnderInactiveParent_DoesNotThrow()
        {
            var inactiveParent = CreateInactiveParent();
            _ = ComponentPool.Get(_source, inactiveParent.transform);

            // The instance's OnEnable fires for the first time here, even though the pool already considers it active.
            inactiveParent.SetActive(true);

            DestroyGameObjects(inactiveParent.transform);
        }

        [Test]
        public void ActivatingParent_OfInstanceCreatedUnderInactiveParent_DoesNotUnPoolOtherInstances()
        {
            var inactiveParent = CreateInactiveParent();
            var hiddenInstance = ComponentPool.Get(_source, inactiveParent.transform);

            var pooledInstance = ComponentPool.Get(_source);
            pooledInstance.gameObject.SetActive(false);

            inactiveParent.SetActive(true);

            var instance = ComponentPool.Get(_source);

            Assert.That(instance, Is.SameAs(pooledInstance), "The pooled instance should still be available.");

            DestroyGameObjects(hiddenInstance, pooledInstance, instance, inactiveParent.transform);
        }

        [Test]
        public void CreateInstances_WhilePoolHasPooledInstances_DoesNotPoolCreatedInstances()
        {
            var pooledInstance = ComponentPool.Get(_source);
            pooledInstance.gameObject.SetActive(false);

            var createdInstances = new List<Component>();
            ComponentPool.CreateInstances(createdInstances, _source, 2, false);

            var instance = ComponentPool.Get(_source);

            Assert.That(createdInstances.Count, Is.EqualTo(1));
            Assert.That(instance, Is.SameAs(pooledInstance), "Created instances are active, so the pooled instance should be returned.");

            DestroyGameObjects(pooledInstance, createdInstances[0], instance);
        }

        [Test]
        public void Get_WithBaseType_SharesPool_WithSourceType()
        {
            var instance = ComponentPool.Get(_source);
            instance.gameObject.SetActive(false);

            MonoBehaviour baseTypeInstance = ComponentPool.Get<MonoBehaviour>(_source);

            Assert.That(baseTypeInstance, Is.SameAs(instance));

            DestroyGameObjects(instance, baseTypeInstance);
        }

        [Test]
        public void CreateInstances_ArePooled_ForGetWithBaseType()
        {
            var createdInstances = new List<Component>();
            ComponentPool.CreateInstances(createdInstances, _source, 1, false);
            createdInstances[0].gameObject.SetActive(false);

            MonoBehaviour instance = ComponentPool.Get<MonoBehaviour>(_source);

            Assert.That(instance, Is.SameAs(createdInstances[0]), "Instances created by a ComponentPoolInitializer should be reused.");

            DestroyGameObjects(createdInstances[0], instance);
        }

        [Test]
        public void Get_ReusedInstance_KeepsLocalScale_WhenPreviousParentWasScaled()
        {
            var scaledParent = new GameObject("Scaled Parent").transform;
            scaledParent.localScale = Vector3.one * 2f;

            var instance = ComponentPool.Get(_source, scaledParent);
            instance.gameObject.SetActive(false);

            var reusedInstance = ComponentPool.Get(_source);

            Assert.That(reusedInstance, Is.SameAs(instance));
            Assert.That(reusedInstance.transform.localScale, Is.EqualTo(_source.transform.localScale).Using(Vector3EqualityComparer.Instance));

            DestroyGameObjects(instance, scaledParent);
        }

        [Test]
        public void DestroySourceInstances_DoesNotThrow_WhenInstanceWasDestroyedBeforeBeingActivated()
        {
            var inactiveParent = CreateInactiveParent();
            _ = ComponentPool.Get(_source, inactiveParent.transform);

            // Objects that were never active don't receive OnDestroy, so the pool isn't notified.
            Object.DestroyImmediate(inactiveParent);

            Assert.DoesNotThrow(() => ComponentPool.DestroySourceInstances(_source));
        }

        [UnityTest]
        public IEnumerator DestroyInstances_OfBaseType_DestroysDerivedTypeInstances()
        {
            var instance = ComponentPool.Get(_source);

            ComponentPool.DestroyInstances<MonoBehaviour>();

            yield return null;

            UnityEngine.Assertions.Assert.IsNull(instance);
        }

        [Test]
        public void ResetStatics_ClearsPools()
        {
            var instance = ComponentPool.Get(_source);
            instance.gameObject.SetActive(false);

            ComponentPool.ResetStatics();

            var newInstance = ComponentPool.Get(_source);

            Assert.That(newInstance, Is.Not.SameAs(instance));

            DestroyGameObjects(instance, newInstance);
        }

        private static GameObject CreateInactiveParent()
        {
            var parent = new GameObject("Inactive Parent");
            parent.SetActive(false);
            return parent;
        }

        private static void DestroyGameObjects(params Component[] components)
        {
            foreach (var component in components)
            {
                if (component != null)
                {
                    Object.DestroyImmediate(component.gameObject);
                }
            }
        }
    }
}

#endif
