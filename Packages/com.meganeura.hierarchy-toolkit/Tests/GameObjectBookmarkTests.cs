using System;
using System.Collections;
using NUnit.Framework;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class GameObjectBookmarkTests
    {
        private string folder;
        private string key;
        private Scene scene;
        private Scene previousScene;
        private Object[] previousSelection;
        private GameObject first;
        private GameObject second;
        private GameObjectBookmarkStore store;

        [SetUp]
        public void SetUp()
        {
            previousSelection = Selection.objects;
            previousScene = SceneManager.GetActiveScene();
            var name = "BookmarkTest_" + Guid.NewGuid().ToString("N");
            const string root = "Packages/com.meganeura.hierarchy-toolkit/Tests";
            AssetDatabase.CreateFolder(root, name);
            folder = root + "/" + name;
            var path = folder + "/Scene.unity";
            System.IO.File.WriteAllText(path,
                "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!29 &1\nOcclusionCullingSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n");
            AssetDatabase.ImportAsset(path);
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            first = new GameObject("Parent");
            SceneManager.MoveGameObjectToScene(first, scene);
            second = new GameObject("Child");
            SceneManager.MoveGameObjectToScene(second, scene);
            second.transform.SetParent(first.transform);
            EditorSceneManager.SaveScene(scene);
            key = "HierarchyToolkit.Tests." + name;
            store = GameObjectBookmarkStore.Open(key);
        }

        [TearDown]
        public void TearDown()
        {
            if (store != null) { Undo.ClearUndo(store); Object.DestroyImmediate(store); }
            EditorPrefs.DeleteKey(key);
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            Selection.objects = previousSelection;
            if (folder != null) AssetDatabase.DeleteAsset(folder);
        }

        [Test]
        public void AddDeduplicatesAndPersistsWithoutChangingSceneOrSelection()
        {
            Selection.activeGameObject = second;
            var before = System.IO.File.ReadAllText(scene.path);
            store.Add(null, new[] { first, second, first });
            store.Add(first, new[] { first, second });
            Assert.That(store.Entries.Count, Is.EqualTo(2));
            Assert.That(scene.isDirty, Is.False);
            Assert.That(System.IO.File.ReadAllText(scene.path), Is.EqualTo(before));
            Assert.That(SceneMetadataStore.Find(scene), Is.Null);
            Assert.That(Selection.activeGameObject, Is.EqualTo(second));
            Undo.ClearUndo(store);
            Object.DestroyImmediate(store);
            store = GameObjectBookmarkStore.Open(key);
            Assert.That(store.Entries.Count, Is.EqualTo(2));
            Assert.That(GameObjectBookmarkStore.Resolve(store.Entries[1]), Is.EqualTo(second));
        }

        [Test]
        public void SelectionConventionAndRemovalSupportUndoRedo()
        {
            store.Add(first, new[] { second });
            Assert.That(store.Contains(first), Is.True);
            Assert.That(store.Contains(second), Is.False);
            store.Add(first, new[] { first, second });
            Assert.That(store.Entries.Count, Is.EqualTo(2));
            store.Remove(first, new[] { first, second });
            Assert.That(store.Entries.Count, Is.Zero);
            Undo.PerformUndo();
            Assert.That(store.Entries.Count, Is.EqualTo(2));
            Undo.PerformRedo();
            Assert.That(store.Entries.Count, Is.Zero);
        }

        [Test]
        public void RenameUsesCurrentNameAndDeletedBookmarkCanBeRemoved()
        {
            store.Add(null, new[] { second });
            var entry = store.Entries[0];
            second.name = "Renamed";
            Assert.That(GameObjectBookmarkStore.DisplayName(entry), Is.EqualTo("Renamed"));
            Object.DestroyImmediate(second);
            Assert.That(GameObjectBookmarkStore.Resolve(entry), Is.Null);
            Assert.That(GameObjectBookmarkReveal.Reveal(entry, null), Is.False);
            store.Remove(entry.objectId);
            Assert.That(store.Entries.Count, Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void UnloadedSceneIsNotOpenedAndStableIdResolvesAfterReload()
        {
            store.Add(null, new[] { second });
            var entry = store.Entries[0];
            var path = scene.path;
            EditorSceneManager.CloseScene(scene, true);
            var count = SceneManager.sceneCount;
            Assert.That(GameObjectBookmarkStore.Resolve(entry), Is.Null);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(count));
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            Assert.That(GameObjectBookmarkStore.Resolve(entry).name, Is.EqualTo("Child"));
        }

        [Test]
        public void PrefabAssetAndPreviewAreRejectedButSceneInstanceIsSupported()
        {
            var asset = PrefabUtility.SaveAsPrefabAsset(first, folder + "/Target.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
            EditorSceneManager.SaveScene(scene);
            var modificationsBefore = JsonUtility.ToJson(new Modifications { values = PrefabUtility.GetPropertyModifications(instance) });
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var target = new GameObject("Preview");
                SceneManager.MoveGameObjectToScene(target, preview);
                store.Add(null, new[] { asset, target, instance, null });
                Assert.That(store.Entries.Count, Is.EqualTo(1));
                Assert.That(GameObjectBookmarkStore.Resolve(store.Entries[0]), Is.EqualTo(instance));
                Assert.That(JsonUtility.ToJson(new Modifications { values = PrefabUtility.GetPropertyModifications(instance) }), Is.EqualTo(modificationsBefore));
                Assert.That(scene.isDirty, Is.False);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        [Serializable]
        private sealed class Modifications { public PropertyModification[] values; }

        [Test]
        public void CorruptPreferencesFailSafely()
        {
            Undo.ClearUndo(store);
            Object.DestroyImmediate(store);
            EditorPrefs.SetString(key, "not json");
            Assert.DoesNotThrow(() => store = GameObjectBookmarkStore.Open(key));
            Assert.That(store.Entries.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator RevealExpandsAncestorsAndSelectsCorrectObject()
        {
            // Use an isolated native Hierarchy so the user's expansion/search state is untouched.
            var window = ScriptableObject.CreateInstance<HierarchyWindow>();
            window.Show();
            try
            {
                yield return null;
                store.Add(null, new[] { second });
                var view = window.View;
                var handler = view.Source.GetOrCreateNodeTypeHandler<HierarchyGameObjectHandler>();
                var parent = handler.GetOrCreateNode(first);
                view.Collapse(parent);
                view.Update();
                Assert.That(view.IsExpanded(parent), Is.False);
                Assert.That(GameObjectBookmarkReveal.Reveal(store.Entries[0], window), Is.True);
                yield return null;
                Assert.That(view.IsExpanded(parent), Is.True);
                Assert.That(Selection.activeGameObject, Is.EqualTo(second));
                var child = handler.GetOrCreateNode(second);
                Assert.That(view.ViewModel.IndexOf(child), Is.GreaterThanOrEqualTo(0));
            }
            finally { window.Close(); }
        }
    }
}
