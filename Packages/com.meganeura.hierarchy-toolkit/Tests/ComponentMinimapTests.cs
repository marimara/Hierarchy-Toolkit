using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class ComponentMinimapTests
    {
        private Scene scene;
        private GameObject target;
        private ComponentMinimapCache cache;

        [SetUp]
        public void SetUp()
        {
            scene = EditorSceneManager.NewPreviewScene();
            target = new GameObject("Minimap Test");
            SceneManager.MoveGameObjectToScene(target, scene);
            cache = new ComponentMinimapCache();
        }

        [TearDown]
        public void TearDown()
        {
            cache.Dispose();
            if (target != null) Undo.ClearUndo(target);
            EditorSceneManager.ClosePreviewScene(scene);
        }

        [Test]
        public void MissesAreDeferredAndRepeatedReadsReuseSnapshot()
        {
            target.AddComponent<Camera>();
            for (var i = 0; i < 100; ++i) Assert.That(cache.GetIcons(target), Is.Empty);
            Assert.That(cache.CollectionCount, Is.Zero);
            cache.Rebuild();
            var snapshot = cache.GetIcons(target);
            Assert.That(snapshot.Length, Is.EqualTo(1));
            for (var i = 0; i < 100; ++i) Assert.That(cache.GetIcons(target), Is.SameAs(snapshot));
            cache.Rebuild();
            Assert.That(cache.CollectionCount, Is.EqualTo(1));
        }

        [Test]
        public void ComponentsKeepOrderSkipTransformAndDeduplicateTypes()
        {
            target.AddComponent<BoxCollider>();
            target.AddComponent<AudioSource>();
            target.AddComponent<BoxCollider>();
            cache.GetIcons(target);
            cache.Rebuild();
            var result = cache.GetIcons(target);
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Type, Is.EqualTo(typeof(BoxCollider)));
            Assert.That(result[1].Type, Is.EqualTo(typeof(AudioSource)));
            Assert.That(result[0].Name, Is.EqualTo("BoxCollider"));
            Assert.That(result[0].Texture, Is.EqualTo(EditorGUIUtility.ObjectContent(target.GetComponent<BoxCollider>(), typeof(BoxCollider)).image));
        }

        [Test]
        public void MissingScriptSlotsAreSkippedWithoutDiscardingOtherComponents()
        {
            var camera = target.AddComponent<Camera>();
            var result = cache.Collect(new Component[] { target.transform, null, camera, null });
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Type, Is.EqualTo(typeof(Camera)));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ResolvedIconsAreSharedAcrossObjects()
        {
            var first = cache.Collect(new Component[] { target.AddComponent<BoxCollider>() });
            var second = cache.Collect(new Component[] { target.AddComponent<BoxCollider>() });
            Assert.That(second[0], Is.SameAs(first[0]));
        }

        [Test]
        public void InvalidationsCoalesceAndRebuildChangedComposition()
        {
            var camera = target.AddComponent<Camera>();
            cache.GetIcons(target);
            cache.Rebuild();
            UnityEngine.Object.DestroyImmediate(camera);
            target.AddComponent<AudioSource>();
            cache.Invalidate();
            cache.Invalidate();
            Assert.That(cache.GetIcons(target), Is.Empty);
            Assert.That(cache.CollectionCount, Is.EqualTo(1));
            cache.Rebuild();
            Assert.That(cache.GetIcons(target)[0].Type, Is.EqualTo(typeof(AudioSource)));
            Assert.That(cache.CollectionCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator EditorEventsRefreshAddedRemovedAndUndoneComponents()
        {
            cache.GetIcons(target);
            cache.Rebuild();
            Undo.IncrementCurrentGroup();
            var collider = Undo.AddComponent<BoxCollider>(target);
            yield return null;
            yield return null;
            cache.Rebuild();
            Assert.That(cache.GetIcons(target).Length, Is.EqualTo(1), "Component addition event");
            Undo.IncrementCurrentGroup();
            Undo.DestroyObjectImmediate(collider);
            yield return null;
            yield return null;
            cache.Rebuild();
            Assert.That(cache.GetIcons(target), Is.Empty);
            Undo.PerformUndo();
            cache.Rebuild();
            Assert.That(cache.GetIcons(target).Length, Is.EqualTo(1), "Undo component removal");
            Undo.PerformRedo();
            cache.Rebuild();
            Assert.That(cache.GetIcons(target), Is.Empty);
        }

        [Test]
        public void DestroyedHiddenAndDisposedTargetsFailSafely()
        {
            cache.GetIcons(target);
            target.hideFlags = HideFlags.HideInHierarchy;
            Assert.That(cache.GetIcons(target), Is.Empty);
            UnityEngine.Object.DestroyImmediate(target);
            cache.Invalidate();
            Assert.DoesNotThrow(cache.Rebuild);
            Assert.That(cache.GetIcons(target), Is.Empty);
            cache.Dispose();
            Assert.DoesNotThrow(cache.Rebuild);
        }

        [TestCase(300f, 3)]
        [TestCase(160f, 2)]
        [TestCase(144f, 1)]
        [TestCase(130f, 0)]
        public void MinimapShrinksAfterActivationReservation(float width, int expected)
        {
            var layout = new HierarchyRowLayout(new Rect(0, 0, width, 18), 100, width);
            Assert.That(layout.TryReserveRight(18f, out var activation), Is.True);
            var count = ComponentMinimapControl.Reserve(ref layout, 3, out var icons);
            Assert.That(count, Is.EqualTo(expected));
            if (count == 0) return;
            Assert.That(icons.xMax, Is.LessThan(activation.xMin));
            Assert.That(icons.xMin, Is.GreaterThan(100));
            Assert.That(activation.xMax, Is.LessThan(width));
        }

        [Test]
        public void TooltipsAndImagesUpdateAndHiddenSlotsReleaseOldContent()
        {
            var control = new ComponentMinimapControl();
            var icons = cache.Collect(new Component[] { target.AddComponent<Camera>(), target.AddComponent<AudioSource>() });
            var layout = new HierarchyRowLayout(new Rect(0, 0, 300, 18), 100, 280);
            control.Refresh(icons, ref layout);
            Assert.That(control.pickingMode, Is.EqualTo(PickingMode.Ignore));
            var image = (Image)control[0];
            Assert.That(image.tooltip, Is.EqualTo("Camera"));
            Assert.That(image.image, Is.SameAs(icons[0].Texture));
            Assert.That(image.focusable, Is.False);
            layout = new HierarchyRowLayout(new Rect(0, 0, 110, 18), 100, 110);
            control.Refresh(icons, ref layout);
            Assert.That(control.style.display.value, Is.EqualTo(DisplayStyle.None));
            control.Refresh(Array.Empty<ComponentMinimapCache.Icon>(), ref layout);
            Assert.That(image.image, Is.Null);
            Assert.That(image.tooltip, Is.Empty);
        }

        [Test]
        public void FeatureCanBeDisabledIndependently()
        {
            using var feature = new ComponentMinimapFeature();
            target.AddComponent<Camera>();
            feature.GetIcons(target);
            feature.Cache.Rebuild();
            Assert.That(feature.GetIcons(target).Length, Is.EqualTo(1));
            feature.Enabled = false;
            Assert.That(feature.GetIcons(target), Is.Empty);
            feature.Enabled = true;
            Assert.That(feature.GetIcons(target).Length, Is.EqualTo(1));
        }
    }
}
