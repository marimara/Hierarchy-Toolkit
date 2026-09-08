using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class SceneSelectorTests
    {
        private const string SceneYaml = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!29 &1\nOcclusionCullingSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n";
        private string folder;
        private string key;
        private SceneFavoriteStore favorites;
        private SceneCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            var name = "SceneSelectorTest_" + Guid.NewGuid().ToString("N");
            const string root = "Packages/com.meganeura.hierarchy-toolkit/Tests";
            AssetDatabase.CreateFolder(root, name);
            folder = root + "/" + name;
            AssetDatabase.CreateFolder(folder, "First");
            AssetDatabase.CreateFolder(folder, "Second");
            CreateScene(folder + "/First/Duplicate.unity");
            CreateScene(folder + "/Second/Duplicate.unity");
            CreateScene(folder + "/Ordinary.unity");
            AssetDatabase.Refresh();
            key = "HierarchyToolkit.Tests." + name;
            favorites = SceneFavoriteStore.Open(key);
            catalog = new SceneCatalog();
        }

        [TearDown]
        public void TearDown()
        {
            catalog?.Dispose();
            if (favorites != null) { Undo.ClearUndo(favorites); Object.DestroyImmediate(favorites); }
            EditorPrefs.DeleteKey(key);
            if (folder != null) AssetDatabase.DeleteAsset(folder);
        }

        [Test]
        public void DiscoveryCachesScenesAndDisambiguatesDuplicateNames()
        {
            var first = FindByPath(folder + "/First/Duplicate.unity");
            var second = FindByPath(folder + "/Second/Duplicate.unity");
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(first.DisplayName, Does.Contain("First"));
            Assert.That(second.DisplayName, Does.Contain("Second"));
            Assert.That(FindByPath(folder + "/Ordinary.unity").DisplayName, Is.EqualTo("Ordinary"));
        }

        [Test]
        public void RefreshTracksMovedAndDeletedSceneAssetsByGuid()
        {
            var original = FindByPath(folder + "/Ordinary.unity");
            var guid = original.Guid;
            var movedPath = folder + "/First/Moved.unity";
            Assert.That(AssetDatabase.MoveAsset(original.Path, movedPath), Is.Empty);
            catalog.Refresh();
            Assert.That(FindByGuid(guid).Path, Is.EqualTo(movedPath));
            Assert.That(AssetDatabase.DeleteAsset(movedPath), Is.True);
            catalog.Refresh();
            Assert.That(FindByGuid(guid), Is.Null);
        }

        [Test]
        public void FavoritesDeduplicatePersistToggleAndLeadFilteredResults()
        {
            var ordinary = FindByPath(folder + "/Ordinary.unity");
            favorites.SetFavorite(ordinary.Guid, true);
            favorites.SetFavorite(ordinary.Guid, true);
            Assert.That(favorites.Guids.Count, Is.EqualTo(1));
            var visible = new List<SceneCatalog.Entry>();
            SceneSelectorModel.Build(catalog.Entries, favorites, string.Empty, visible, out var favoriteCount);
            Assert.That(favoriteCount, Is.EqualTo(1));
            Assert.That(visible[0].Guid, Is.EqualTo(ordinary.Guid));
            SceneSelectorModel.Build(catalog.Entries, favorites, "Ordinary", visible, out favoriteCount);
            Assert.That(visible.Count, Is.EqualTo(1));
            Undo.ClearUndo(favorites);
            Object.DestroyImmediate(favorites);
            favorites = SceneFavoriteStore.Open(key);
            Assert.That(favorites.Contains(ordinary.Guid), Is.True);
            favorites.Toggle(ordinary.Guid);
            Assert.That(favorites.Contains(ordinary.Guid), Is.False);
        }

        [Test]
        public void CancelledSaveSafeguardDoesNotOpenSceneAndInvalidAssetFailsSafely()
        {
            var entry = FindByPath(folder + "/Ordinary.unity");
            var opened = false;
            Assert.That(SceneSelectorNavigator.TryOpen(entry, (guid, path) => true, () => false, path => opened = true), Is.False);
            Assert.That(opened, Is.False);
            Assert.That(SceneSelectorNavigator.TryOpen(entry, (guid, path) => false, () => true, path => opened = true), Is.False);
            Assert.That(opened, Is.False);
            Assert.That(SceneSelectorNavigator.TryOpen(entry, (guid, path) => true, () => true, path => opened = true), Is.True);
            Assert.That(opened, Is.True);
        }

        private static void CreateScene(string path)
        {
            System.IO.File.WriteAllText(path, SceneYaml);
            AssetDatabase.ImportAsset(path);
        }

        private SceneCatalog.Entry FindByPath(string path)
        {
            for (var i = 0; i < catalog.Entries.Count; ++i)
                if (catalog.Entries[i].Path == path) return catalog.Entries[i];
            return null;
        }

        private SceneCatalog.Entry FindByGuid(string guid)
        {
            for (var i = 0; i < catalog.Entries.Count; ++i)
                if (catalog.Entries[i].Guid == guid) return catalog.Entries[i];
            return null;
        }
    }
}
