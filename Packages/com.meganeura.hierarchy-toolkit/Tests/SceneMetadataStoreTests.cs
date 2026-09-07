using System;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed partial class SceneMetadataStoreTests
    {
        private string folder;
        private string scenePath;
        private string storePath;
        private Scene scene;
        private Scene originalActiveScene;
        private GameObject target;
        private SceneMetadataStore store;

        [SetUp]
        public void SetUp()
        {
            const string root = "Packages/com.meganeura.hierarchy-toolkit/Tests";
            var name = "MetadataTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder(root, name);
            folder = root + "/" + name;
            scenePath = folder + "/Scene.unity";

            originalActiveScene = SceneManager.GetActiveScene();
            // Opening a fixture additively also works while the runner owns an untitled scene.
            System.IO.File.WriteAllText(scenePath,
                "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!29 &1\nOcclusionCullingSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n");
            AssetDatabase.ImportAsset(scenePath);
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            target = new GameObject("Metadata Target");
            SceneManager.MoveGameObjectToScene(target, scene);
            Assert.That(EditorSceneManager.SaveScene(scene, scenePath), Is.True);
            storePath = SceneMetadataStore.GetStorePath(scene);
            store = SceneMetadataStore.OpenOrCreate(scene);
        }

        [TearDown]
        public void TearDown()
        {
            if (store != null) Undo.ClearUndo(store);
            if (originalActiveScene.IsValid() && originalActiveScene.isLoaded) SceneManager.SetActiveScene(originalActiveScene);
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            if (!string.IsNullOrEmpty(storePath)) AssetDatabase.DeleteAsset(storePath);
            if (!string.IsNullOrEmpty(folder)) AssetDatabase.DeleteAsset(folder);
        }

        [Test]
        public void CreateAndRetrieveUsesStableIdAndDoesNotDuplicate()
        {
            Assert.That(store.TryGetMetadata(target, out _), Is.False);
            Assert.That(store.Count, Is.Zero);
            var metadata = store.GetOrCreateMetadata(target);
            Assert.That(metadata.ObjectId, Is.EqualTo(GlobalObjectId.GetGlobalObjectIdSlow(target).ToString()));
            Assert.That(metadata.IsEmpty, Is.True);
            Assert.That(store.GetOrCreateMetadata(target), Is.SameAs(metadata));
            Assert.That(store.TryGetMetadata(target, out var found), Is.True);
            Assert.That(found, Is.SameAs(metadata));
            Assert.That(store.Count, Is.EqualTo(1));
        }

        [Test]
        public void SetAndClearColorRemovesEmptyEntry()
        {
            store.SetColor(target, Color.clear);
            Assert.That(store.TryGetMetadata(target, out var metadata), Is.True);
            Assert.That(metadata.HasColorOverride, Is.True);
            Assert.That(metadata.CustomColor, Is.EqualTo(Color.clear));
            store.ClearColor(target);
            Assert.That(store.TryGetMetadata(target, out _), Is.False);
            store.ClearColor(target);
            Assert.That(store.Count, Is.Zero);
        }

        [Test]
        public void SetAndClearIconPreservesOtherOverride()
        {
            store.SetColor(target, Color.red);
            store.SetIcon(target, "GameObject Icon");
            Assert.That(store.TryGetMetadata(target, out var metadata), Is.True);
            Assert.That(metadata.HasIconOverride, Is.True);
            Assert.That(metadata.CustomIconReference, Is.EqualTo("builtin:GameObject Icon"));
            store.ClearIcon(target);
            Assert.That(store.TryGetMetadata(target, out metadata), Is.True);
            Assert.That(metadata.HasIconOverride, Is.False);
            Assert.That(metadata.HasColorOverride, Is.True);
            store.SetIcon(target, "Other Icon");
            store.ClearColor(target);
            Assert.That(store.TryGetMetadata(target, out metadata), Is.True);
            Assert.That(metadata.HasIconOverride, Is.True);
            Assert.That(metadata.HasColorOverride, Is.False);
            store.ClearIcon(target);
            Assert.That(store.Count, Is.Zero);
        }

        [Test]
        public void RemoveAndCleanupOnlyRemoveIntendedEntries()
        {
            store.GetOrCreateMetadata(target);
            var other = new GameObject("Other");
            SceneManager.MoveGameObjectToScene(other, scene);
            store.SetIcon(other, "GameObject Icon");
            Assert.That(store.CleanupEmptyMetadata(), Is.EqualTo(1));
            Assert.That(store.TryGetMetadata(other, out _), Is.True);
            Assert.That(store.CleanupEmptyMetadata(), Is.Zero);
            Assert.That(store.RemoveMetadata(other), Is.True);
            Assert.That(store.RemoveMetadata(other), Is.False);
            Assert.That(store.Count, Is.Zero);
        }

        [Test]
        public void RejectsNullUnsavedAndOtherSceneObjects()
        {
            Assert.That(store.TryGetMetadata(null, out _), Is.False);
            Assert.Throws<ArgumentException>(() => store.SetColor(null, Color.red));
            Assert.Throws<ArgumentException>(() => store.SetIcon(target, " "));
            var temporary = EditorSceneManager.NewPreviewScene();
            try
            {
                var other = new GameObject("Unsaved");
                SceneManager.MoveGameObjectToScene(other, temporary);
                Assert.That(store.TryGetMetadata(other, out _), Is.False);
                Assert.Throws<ArgumentException>(() => store.GetOrCreateMetadata(other));
                Assert.Throws<ArgumentException>(() => SceneMetadataStore.OpenOrCreate(temporary));
            }
            finally { EditorSceneManager.ClosePreviewScene(temporary); }
            AssetDatabase.CopyAsset(scenePath, folder + "/Other.unity");
            temporary = EditorSceneManager.OpenScene(folder + "/Other.unity", OpenSceneMode.Additive);
            try
            {
                Assert.That(store.TryGetMetadata(temporary.GetRootGameObjects()[0], out _), Is.False);
                Assert.That(SceneMetadataStore.GetStorePath(temporary), Is.Not.EqualTo(storePath));
                Assert.That(SceneMetadataStore.Find(temporary), Is.Null);
            }
            finally { EditorSceneManager.CloseScene(temporary, true); }
        }

        [Test]
        public void PrefabAssetRejectedAndInstancesHaveIndependentMetadata()
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(target, folder + "/Prefab.prefab");
            Assert.That(store.TryGetMetadata(prefab, out _), Is.False);
            Assert.Throws<ArgumentException>(() => store.SetColor(prefab, Color.red));
            var first = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var second = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            EditorSceneManager.SaveScene(scene);
            store.SetColor(first, Color.red);
            store.SetColor(second, Color.blue);
            Assert.That(store.TryGetMetadata(first, out var a), Is.True);
            Assert.That(store.TryGetMetadata(second, out var b), Is.True);
            Assert.That(a.ObjectId, Is.Not.EqualTo(b.ObjectId));
            Assert.That(a.CustomColor, Is.EqualTo(Color.red));
            Assert.That(b.CustomColor, Is.EqualTo(Color.blue));
        }

        [Test]
        public void AssetReloadAndSceneReopenPreserveMetadata()
        {
            store.SetColor(target, Color.cyan);
            store.SetIcon(target, "GameObject Icon");
            var id = store.GetOrCreateMetadata(target).ObjectId;
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);
            ReloadStore();
            Assert.That(store.Count, Is.EqualTo(1)); // Closed scenes are not treated as deleted objects.
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            target = scene.GetRootGameObjects()[0];
            Assert.That(store.TryGetMetadata(target, out var metadata), Is.True);
            Assert.That(metadata.ObjectId, Is.EqualTo(id));
            Assert.That(metadata.CustomColor, Is.EqualTo(Color.cyan));
            Assert.That(metadata.CustomIconReference, Is.EqualTo("builtin:GameObject Icon"));
            Assert.That(metadata.HasColorOverride && metadata.HasIconOverride, Is.True);
        }

        [Test]
        public void UndoRedoAndUndoPersistenceRestoreEntries()
        {
            Undo.IncrementCurrentGroup();
            store.SetColor(target, Color.red);
            Undo.IncrementCurrentGroup();
            store.ClearColor(target);
            Undo.IncrementCurrentGroup();
            Undo.PerformUndo();
            Assert.That(store.TryGetMetadata(target, out var metadata), Is.True);
            Assert.That(metadata.CustomColor, Is.EqualTo(Color.red));
            Undo.PerformRedo();
            Assert.That(store.Count, Is.Zero);
            Undo.PerformUndo();
            ReloadStore();
            Assert.That(store.TryGetMetadata(target, out metadata), Is.True);
            Assert.That(metadata.CustomColor, Is.EqualTo(Color.red));
        }

        [Test]
        public void CanonicalResolutionSurvivesSceneRenameAndStoreReload()
        {
            Assert.That(SceneMetadataStore.OpenOrCreate(scene), Is.SameAs(store));
            Assert.That(SceneMetadataStore.Find(scene), Is.SameAs(store));
            Assert.That(storePath, Does.EndWith("/" + store.SceneGuid + ".asset"));
            var movedPath = folder + "/Renamed.unity";
            Assert.That(AssetDatabase.MoveAsset(scenePath, movedPath), Is.Empty);
            Assert.That(SceneMetadataStore.GetStorePath(scene), Is.EqualTo(storePath));
            ReloadStore();
            Assert.That(SceneMetadataStore.OpenOrCreate(scene), Is.SameAs(store));
        }

        [Test]
        public void ColorSnapshotInvalidatesOnMutationAndClearPreservesIcon()
        {
            using var cache = new ManualColorCache();
            var id = target.GetEntityId();
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out _), Is.False);
            store.SetColor(target, Color.red);
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out var color), Is.True);
            Assert.That(color, Is.EqualTo(Color.red));
            store.SetColor(target, Color.blue);
            Assert.That(cache.TryGetColor(id, out _), Is.False);
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out color), Is.True);
            Assert.That(color, Is.EqualTo(Color.blue));
            store.SetIcon(target, "GameObject Icon");
            store.ClearColor(target);
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out _), Is.False);
            Assert.That(store.TryGetMetadata(target, out var metadata), Is.True);
            Assert.That(metadata.HasIconOverride, Is.True);
        }

        [Test]
        public void ColorSnapshotInvalidatesOnSceneSaveCloseAndUndo()
        {
            using var cache = new ManualColorCache();
            var id = target.GetEntityId();
            Undo.IncrementCurrentGroup();
            store.SetColor(target, Color.green);
            Undo.IncrementCurrentGroup();
            cache.Rebuild();
            EditorSceneManager.SaveScene(scene);
            Assert.That(cache.TryGetColor(id, out _), Is.False);
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out _), Is.True);
            Undo.PerformUndo();
            Assert.That(cache.TryGetColor(id, out _), Is.False);
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out _), Is.False);
            Undo.PerformRedo();
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out _), Is.True);
            EditorSceneManager.CloseScene(scene, true);
            Assert.That(cache.TryGetColor(id, out _), Is.False);
            cache.Rebuild();
            Assert.That(cache.TryGetColor(id, out _), Is.False);
        }

        [Test]
        public void ColorActionsAssignChangeClearAndInitializePicker()
        {
            var initial = ManualColorOperations.InitialColor(target);
            Assert.That(initial.a, Is.GreaterThan(0));
            var color = new Color(0.2f, 0.4f, 0.6f, 0.3f);
            ManualColorOperations.Apply(new[] { target }, color);
            Assert.That(ManualColorOperations.InitialColor(target), Is.EqualTo(color));
            ManualColorOperations.Apply(new[] { target }, Color.clear);
            Assert.That(ManualColorOperations.InitialColor(target), Is.EqualTo(Color.clear));
            ManualColorOperations.Apply(new[] { target }, null);
            Assert.That(store.TryGetMetadata(target, out _), Is.False);
            Assert.That(ManualColorOperations.InitialColor(target), Is.EqualTo(initial));
            Undo.PerformUndo();
            Assert.That(ManualColorOperations.InitialColor(target), Is.EqualTo(Color.clear));
            Undo.PerformRedo();
            Assert.That(store.TryGetMetadata(target, out _), Is.False);
        }

        [Test]
        public void ColorActionSelectionUsesClickedObjectAndDeduplicates()
        {
            var other = new GameObject("Other selected object");
            SceneManager.MoveGameObjectToScene(other, scene);
            EditorSceneManager.SaveScene(scene);
            Assert.That(ManualColorOperations.ResolveTargets(target, new[] { target, other, target }),
                Is.EqualTo(new[] { target, other }));
            var targets = ManualColorOperations.ResolveTargets(target, new[] { other });
            Assert.That(targets, Is.EqualTo(new[] { target }));
            ManualColorOperations.Apply(targets, Color.red);
            Assert.That(store.TryGetMetadata(other, out _), Is.False);
        }

        [Test]
        public void ColorActionsSkipUnsupportedObjectsAndRecheckDestroyedTargets()
        {
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var unsupported = new GameObject("Preview object");
                SceneManager.MoveGameObjectToScene(unsupported, preview);
                var prefab = PrefabUtility.SaveAsPrefabAsset(target, folder + "/ActionPrefab.prefab");
                var selection = new[] { unsupported, prefab, null, target };
                Assert.That(ManualColorOperations.ResolveTargets(target, selection), Is.EqualTo(new[] { target }));
                Assert.That(ManualColorOperations.ResolveTargets(unsupported, new[] { target }), Is.Empty);
                Assert.DoesNotThrow(() => ManualColorOperations.Apply(selection, Color.green));
                UnityEngine.Object.DestroyImmediate(unsupported);
                Assert.DoesNotThrow(() => ManualColorOperations.Apply(selection, null));
                Assert.That(store.Count, Is.Zero);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        [Test]
        public void ColorActionsAcrossScenesUndoAndRedoAsOneOperation()
        {
            var otherPath = folder + "/ActionOther.unity";
            Assert.That(AssetDatabase.CopyAsset(scenePath, otherPath), Is.True);
            var otherScene = EditorSceneManager.OpenScene(otherPath, OpenSceneMode.Additive);
            var otherStorePath = SceneMetadataStore.GetStorePath(otherScene);
            SceneMetadataStore otherStore = null;
            try
            {
                var second = new GameObject("Second in same scene");
                SceneManager.MoveGameObjectToScene(second, scene);
                EditorSceneManager.SaveScene(scene);
                var other = otherScene.GetRootGameObjects()[0];
                var targets = new[] { target, second, other };
                ManualColorOperations.Apply(targets, Color.cyan);
                otherStore = SceneMetadataStore.Find(otherScene);
                Assert.That(otherStore, Is.Not.Null);
                Assert.That(store.Count, Is.EqualTo(2));
                Assert.That(otherStore.TryGetMetadata(other, out var entry), Is.True);
                Assert.That(entry.CustomColor, Is.EqualTo(Color.cyan));
                Undo.PerformUndo();
                Assert.That(store.Count, Is.Zero);
                Assert.That(otherStore.Count, Is.Zero);
                Undo.PerformRedo();
                Assert.That(store.Count, Is.EqualTo(2));
                Assert.That(otherStore.Count, Is.EqualTo(1));
                ManualColorOperations.Apply(targets, null);
                Assert.That(store.Count + otherStore.Count, Is.Zero);
                Undo.PerformUndo();
                Assert.That(store.Count, Is.EqualTo(2));
                Assert.That(otherStore.Count, Is.EqualTo(1));
                Undo.PerformRedo();
                Assert.That(store.Count + otherStore.Count, Is.Zero);
            }
            finally
            {
                if (otherStore != null) Undo.ClearUndo(otherStore);
                EditorSceneManager.CloseScene(otherScene, true);
                AssetDatabase.DeleteAsset(otherStorePath);
            }
        }

        [Test]
        public void RetainedColorRowRefreshesAfterCacheRebuildAndRestoresOnClear()
        {
            using var cache = new ManualColorCache();
            using var binding = new ManualColorHierarchyBinding(cache);
            var row = new UnityEngine.UIElements.VisualElement();
            row.style.backgroundColor = Color.gray;
            binding.BindRow(row, target.GetEntityId());
            store.SetColor(target, Color.red);
            cache.Rebuild();
            Assert.That(row.style.backgroundColor.value, Is.EqualTo(new Color(1f, 0f, 0f, 0.18f)));
            store.ClearColor(target);
            cache.Rebuild();
            Assert.That(row.style.backgroundColor.value, Is.EqualTo(Color.gray));
        }

        [Test]
        public void RetainedColorRowPreservesSelectionAndClearsRecycledRows()
        {
            var selection = Selection.objects;
            using var cache = new ManualColorCache();
            using var binding = new ManualColorHierarchyBinding(cache);
            var row = new UnityEngine.UIElements.VisualElement();
            var background = row.style.backgroundColor;
            try
            {
                store.SetColor(target, Color.red);
                cache.Rebuild();
                Selection.activeGameObject = target;
                binding.BindRow(row, target.GetEntityId());
                Assert.That(row.style.backgroundColor, Is.EqualTo(background));
                Selection.objects = Array.Empty<UnityEngine.Object>();
                cache.Rebuild();
                Assert.That(row.style.backgroundColor.value.a, Is.EqualTo(0.18f));
                binding.UnbindRow(row);
                Assert.That(row.style.backgroundColor, Is.EqualTo(background));
                binding.BindRow(row, default);
                Assert.That(row.style.backgroundColor, Is.EqualTo(background));
                cache.Rebuild();
                Assert.That(row.style.backgroundColor, Is.EqualTo(background));
            }
            finally { Selection.objects = selection; }
        }

        private void ReloadStore()
        {
            Undo.ClearUndo(store);
            Resources.UnloadAsset(store);
            store = AssetDatabase.LoadAssetAtPath<SceneMetadataStore>(storePath);
            Assert.That(store, Is.Not.Null);
        }
    }
}



