using System;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class ActivationToggleTests
    {
        private string folder;
        private Scene scene;
        private Scene previousScene;
        private Object[] previousSelection;
        private GameObject first;
        private GameObject second;
        private ActivationToggleFeature feature;

        [SetUp]
        public void SetUp()
        {
            previousSelection = Selection.objects;
            previousScene = SceneManager.GetActiveScene();
            const string root = "Packages/com.meganeura.hierarchy-toolkit/Tests";
            var name = "ActivationTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder(root, name);
            folder = root + "/" + name;
            var path = folder + "/Scene.unity";
            System.IO.File.WriteAllText(path,
                "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!29 &1\nOcclusionCullingSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n");
            AssetDatabase.ImportAsset(path);
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            first = Create("First");
            second = Create("Second");
            feature = new ActivationToggleFeature();
        }

        private GameObject Create(string name)
        {
            var target = new GameObject(name);
            SceneManager.MoveGameObjectToScene(target, scene);
            return target;
        }

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var transform in root.GetComponentsInChildren<Transform>(true)) Undo.ClearUndo(transform.gameObject);
                EditorSceneManager.CloseScene(scene, true);
            }
            if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            Selection.objects = previousSelection;
            if (folder != null) AssetDatabase.DeleteAsset(folder);
        }

        [Test]
        public void UnselectedClickOnlyChangesClickedObjectAndPreservesSelection()
        {
            Selection.objects = new Object[] { second };
            feature.Toggle(first);
            Assert.That(first.activeSelf, Is.False);
            Assert.That(second.activeSelf, Is.True);
            Assert.That(Selection.objects, Is.EqualTo(new Object[] { second }));
            Assert.That(SceneMetadataStore.Find(scene), Is.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MixedSelectionUsesClickedStateAndOneUndoRedo(bool initial)
        {
            first.SetActive(initial);
            second.SetActive(!initial);
            var third = Create("Third");
            third.SetActive(initial);
            Selection.objects = new Object[] { first, second, third };
            var selection = Selection.objects;
            feature.Toggle(first);
            Assert.That(first.activeSelf, Is.EqualTo(!initial));
            Assert.That(second.activeSelf, Is.EqualTo(!initial));
            Assert.That(third.activeSelf, Is.EqualTo(!initial));
            Undo.PerformUndo();
            Assert.That(first.activeSelf, Is.EqualTo(initial));
            Assert.That(second.activeSelf, Is.EqualTo(!initial));
            Assert.That(third.activeSelf, Is.EqualTo(initial));
            Undo.PerformRedo();
            Assert.That(first.activeSelf, Is.EqualTo(!initial));
            Assert.That(second.activeSelf, Is.EqualTo(!initial));
            Assert.That(third.activeSelf, Is.EqualTo(!initial));
            Assert.That(Selection.objects, Is.EqualTo(selection));
        }

        [Test]
        public void ChildDisplaysAndTogglesOwnStateUnderInactiveParent()
        {
            second.transform.SetParent(first.transform);
            first.SetActive(false);
            Assert.That(second.activeInHierarchy, Is.False);
            Assert.That(feature.TryGetState(second, out var active), Is.True);
            Assert.That(active, Is.True);
            feature.Toggle(second);
            Assert.That(second.activeSelf, Is.False);
            feature.Toggle(second);
            Assert.That(second.activeSelf, Is.True);
            Assert.That(second.activeInHierarchy, Is.False);
        }

        [Test]
        public void DisabledFeatureDoesNotMutateOrDisplayAndNotifiesOnlyOnChange()
        {
            var changes = 0;
            feature.Changed += () => ++changes;
            feature.Enabled = false;
            feature.Enabled = false;
            feature.Toggle(first);
            Assert.That(first.activeSelf, Is.True);
            Assert.That(feature.TryGetState(first, out _), Is.False);
            Assert.That(changes, Is.EqualTo(1));
            feature.Enabled = true;
            Assert.That(feature.TryGetState(first, out _), Is.True);
        }

        [Test]
        public void UnsupportedTargetsAreSkippedInSelection()
        {
            second.hideFlags = HideFlags.NotEditable;
            Selection.objects = new Object[] { first, second };
            feature.Toggle(first);
            Assert.That(first.activeSelf, Is.False);
            Assert.That(second.activeSelf, Is.True);
            Assert.That(feature.TryGetState(second, out _), Is.False);
            Assert.That(feature.TryGetState(null, out _), Is.False);
            feature.Toggle(null);
        }

        [Test]
        public void PrefabInstanceOverrideSurvivesSaveAndUndoWithoutChangingAsset()
        {
            var path = folder + "/Target.prefab";
            var asset = PrefabUtility.SaveAsPrefabAsset(first, path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
            Selection.objects = new Object[] { instance, asset };
            feature.Toggle(instance);
            Assert.That(instance.activeSelf, Is.False);
            Assert.That(asset.activeSelf, Is.True);
            Assert.That(PrefabUtility.GetPrefabInstanceStatus(instance), Is.EqualTo(PrefabInstanceStatus.Connected));
            Assert.That(feature.TryGetState(asset, out _), Is.False);
            var modifications = PrefabUtility.GetPropertyModifications(instance);
            Assert.That(Array.Exists(modifications, modification => modification.propertyPath == "m_IsActive" && modification.value == "0"), Is.True);
            Undo.PerformUndo();
            Assert.That(instance.activeSelf, Is.True);
            Undo.PerformRedo();
            Assert.That(instance.activeSelf, Is.False);
            Assert.That(EditorSceneManager.SaveScene(scene), Is.True);
            var scenePath = scene.path;
            Undo.ClearUndo(instance);
            EditorSceneManager.CloseScene(scene, true);
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            var found = false;
            foreach (var root in scene.GetRootGameObjects())
                if (PrefabUtility.IsPartOfPrefabInstance(root))
                {
                    found = true;
                    Assert.That(root.activeSelf, Is.False);
                    Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(root).activeSelf, Is.True);
                }
            Assert.That(found, Is.True);
        }

        [Test]
        public void PreviewSceneIsNotEligible()
        {
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var target = new GameObject("Preview");
                SceneManager.MoveGameObjectToScene(target, preview);
                Assert.That(feature.TryGetState(target, out _), Is.False);
                feature.Toggle(target);
                Assert.That(target.activeSelf, Is.True);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        [Test]
        public void RightReservationsPreserveLabelAndNativeControls()
        {
            var layout = new HierarchyRowLayout(new Rect(0, 0, 300, 18), 100, 280);
            Assert.That(layout.TryReserveRight(18, out var activation), Is.True);
            Assert.That(layout.TryReserveRight(40, out var other), Is.True);
            Assert.That(activation.xMax, Is.LessThan(280));
            Assert.That(other.xMax, Is.LessThan(activation.xMin));
            Assert.That(other.xMin, Is.GreaterThan(100));
            Assert.That(layout.TryReserveRight(200, out _), Is.False);
        }

        [TestCase(100f, 90f)]
        [TestCase(40f, 100f)]
        [TestCase(300f, 285f)]
        public void NarrowOrLongLabelRowsOmitControls(float width, float labelEnd)
        {
            var layout = new HierarchyRowLayout(new Rect(0, 0, width, 18), labelEnd, width);
            Assert.That(layout.TryReserveRight(18, out _), Is.False);
        }
    }
}
