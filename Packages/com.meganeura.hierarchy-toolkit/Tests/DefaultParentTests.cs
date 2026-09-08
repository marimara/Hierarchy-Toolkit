using System;
using System.Collections;
using NUnit.Framework;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class DefaultParentTests
    {
        private string folder;
        private string storePath;
        private Scene scene;
        private Scene previousScene;
        private Object[] previousSelection;
        private GameObject first;
        private GameObject second;
        private SceneMetadataStore store;

        [SetUp]
        public void SetUp()
        {
            previousScene = SceneManager.GetActiveScene();
            previousSelection = Selection.objects;
            const string root = "Packages/com.meganeura.hierarchy-toolkit/Tests";
            var name = "DefaultParentTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder(root, name);
            folder = root + "/" + name;
            var scenePath = folder + "/Scene.unity";
            System.IO.File.WriteAllText(scenePath,
                "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!29 &1\nOcclusionCullingSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n");
            AssetDatabase.ImportAsset(scenePath);
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            first = Create("First Default Parent");
            second = Create("Second Default Parent");
            EditorSceneManager.SetActiveScene(scene);
            Assert.That(EditorSceneManager.SaveScene(scene, scenePath), Is.True);
            storePath = SceneMetadataStore.GetStorePath(scene);
            store = SceneMetadataStore.OpenOrCreate(scene);
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
            if (store != null)
            {
                store.ClearDefaultParent();
                Undo.ClearUndo(store);
            }
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects()) Undo.ClearUndo(root);
                EditorSceneManager.CloseScene(scene, true);
            }
            if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            Selection.objects = previousSelection;
            if (!string.IsNullOrEmpty(storePath)) AssetDatabase.DeleteAsset(storePath);
            if (!string.IsNullOrEmpty(folder)) AssetDatabase.DeleteAsset(folder);
        }

        [Test]
        public void MetadataSetsReplacesClearsAndSupportsUndoRedo()
        {
            Assert.That(store.TryGetDefaultParent(scene, out _), Is.False);
            Undo.IncrementCurrentGroup();
            store.SetDefaultParent(first);
            Assert.That(store.TryGetDefaultParent(scene, out var target), Is.True);
            Assert.That(target, Is.EqualTo(first));
            Undo.IncrementCurrentGroup();
            store.SetDefaultParent(second);
            Assert.That(store.TryGetDefaultParent(scene, out target), Is.True);
            Assert.That(target, Is.EqualTo(second));
            Undo.IncrementCurrentGroup();
            Undo.PerformUndo();
            Assert.That(store.TryGetDefaultParent(scene, out target), Is.True);
            Assert.That(target, Is.EqualTo(first));
            Undo.PerformRedo();
            Assert.That(store.TryGetDefaultParent(scene, out target), Is.True);
            Assert.That(target, Is.EqualTo(second));
            store.ClearDefaultParent();
            Assert.That(store.TryGetDefaultParent(scene, out _), Is.False);
        }

        [Test]
        public void DeletedDefaultParentIsIgnoredSafely()
        {
            store.SetDefaultParent(first);
            Object.DestroyImmediate(first);
            Assert.DoesNotThrow(() => store.TryGetDefaultParent(scene, out _));
            Assert.That(store.TryGetDefaultParent(scene, out _), Is.False);
            var added = Create("Added After Delete");
            Assert.That(DefaultParentCreationHandler.TryParentNewObject(added), Is.False);
            Assert.That(added.transform.parent, Is.Null);
        }

        [Test]
        public void DefaultParentIsRestrictedToItsOwningScene()
        {
            store.SetDefaultParent(first);
            var otherPath = folder + "/Other.unity";
            Assert.That(AssetDatabase.CopyAsset(scene.path, otherPath), Is.True);
            var otherScene = EditorSceneManager.OpenScene(otherPath, OpenSceneMode.Additive);
            try
            {
                var added = new GameObject("Other Scene Object");
                SceneManager.MoveGameObjectToScene(added, otherScene);
                Assert.That(DefaultParentCreationHandler.TryParentNewObject(added), Is.False);
                Assert.That(added.transform.parent, Is.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(otherScene, true);
            }
        }

        [Test]
        public void AutomaticParentingPreservesWorldTransformAndIsUndoable()
        {
            first.transform.SetPositionAndRotation(new Vector3(4f, 2f, -3f), Quaternion.Euler(0f, 35f, 0f));
            first.transform.localScale = Vector3.one * 2f;
            store.SetDefaultParent(first);
            var added = Create("New Root");
            var position = new Vector3(8f, 3f, 6f);
            var rotation = Quaternion.Euler(12f, 25f, 7f);
            added.transform.SetPositionAndRotation(position, rotation);
            var scale = added.transform.lossyScale;

            Assert.That(DefaultParentCreationHandler.TryParentNewObject(added), Is.True);
            Assert.That(added.transform.parent, Is.EqualTo(first.transform));
            Assert.That(Vector3.Distance(added.transform.position, position), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(added.transform.rotation, rotation), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(added.transform.lossyScale, scale), Is.LessThan(0.0001f));
            Undo.PerformUndo();
            Assert.That(added.transform.parent, Is.Null);
            Assert.That(Vector3.Distance(added.transform.position, position), Is.LessThan(0.0001f));
            Undo.PerformRedo();
            Assert.That(added.transform.parent, Is.EqualTo(first.transform));
        }

        [Test]
        public void ExplicitParentAndExistingObjectsRemainUnchanged()
        {
            var existing = Create("Existing Root");
            var explicitParent = Create("Explicit Parent");
            var added = Create("Explicitly Parented New Object");
            added.transform.SetParent(explicitParent.transform);

            store.SetDefaultParent(first);
            Assert.That(DefaultParentCreationHandler.TryParentNewObject(added), Is.False);
            Assert.That(added.transform.parent, Is.EqualTo(explicitParent.transform));
            store.SetDefaultParent(second);
            Assert.That(existing.transform.parent, Is.Null);
            Assert.That(added.transform.parent, Is.EqualTo(explicitParent.transform));
        }

        [Test]
        public void PrefabInstanceRootCanBeParentedWithoutBreakingConnection()
        {
            store.SetDefaultParent(first);
            var prefabPath = folder + "/Prefab.prefab";
            var source = PrefabUtility.SaveAsPrefabAsset(second, prefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, scene);
            Assert.That(DefaultParentCreationHandler.TryParentNewObject(instance), Is.True);
            Assert.That(instance.transform.parent, Is.EqualTo(first.transform));
            Assert.That(PrefabUtility.GetPrefabInstanceStatus(instance), Is.EqualTo(PrefabInstanceStatus.Connected));
        }

        [Test]
        public void CacheTracksReplacementAndClearWithoutRowObjectLookup()
        {
            using var cache = new DefaultParentCache();
            store.SetDefaultParent(first);
            cache.Rebuild();
            Assert.That(cache.Contains(first.GetEntityId()), Is.True);
            Assert.That(cache.Contains(second.GetEntityId()), Is.False);
            store.SetDefaultParent(second);
            cache.Rebuild();
            Assert.That(cache.Contains(first.GetEntityId()), Is.False);
            Assert.That(cache.Contains(second.GetEntityId()), Is.True);
            store.ClearDefaultParent();
            cache.Rebuild();
            Assert.That(cache.Contains(second.GetEntityId()), Is.False);
        }

        [UnityTest]
        public IEnumerator HoverShortcutTargetsThePhysicalRowAndTogglesWithoutSelectionFallback()
        {
            var window = ScriptableObject.CreateInstance<HierarchyWindow>();
            window.Show();
            var shortcuts = new HierarchyShortcuts();
            try
            {
                yield return null;
                var view = window.View;
                var handler = view.Source.GetOrCreateNodeTypeHandler<HierarchyGameObjectHandler>();
                var node = handler.GetOrCreateNode(first);
                view.Update();
                yield return null;
                HierarchyViewItem item = null;
                window.rootVisualElement.Query<HierarchyViewItem>().ForEach(candidate =>
                {
                    if (candidate.Node == node) item = candidate;
                });
                Assert.That(item, Is.Not.Null);
                Selection.activeGameObject = second;
                shortcuts.SetHovered(window, item.worldBound.center);
                Assert.That(shortcuts.TryToggleDefaultParentHovered(), Is.True);
                Assert.That(store.IsDefaultParent(first), Is.True);
                Assert.That(store.IsDefaultParent(second), Is.False);
                Assert.That(shortcuts.TryToggleDefaultParentHovered(), Is.True);
                Assert.That(store.TryGetDefaultParent(scene, out _), Is.False);
                Assert.That(Selection.activeGameObject, Is.EqualTo(second));
            }
            finally
            {
                shortcuts.Dispose();
                window.Close();
            }
        }
    }
}
