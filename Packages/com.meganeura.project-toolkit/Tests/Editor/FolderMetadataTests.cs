using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit.Tests
{
    internal sealed class FolderMetadataTests
    {
        private const string TestRootPrefix = "Assets/__MeganeuraProjectToolkitSpec002Tests_";
        private readonly List<FolderMetadataRepository> repositories = new List<FolderMetadataRepository>();
        private string testRoot;

        [SetUp]
        public void SetUp()
        {
            testRoot = TestRootPrefix + Guid.NewGuid().ToString("N");
            string guid = AssetDatabase.CreateFolder("Assets", testRoot.Substring("Assets/".Length));
            Assert.That(guid, Is.Not.Empty, "The isolated Spec 002 test folder could not be created.");
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = repositories.Count - 1; index >= 0; index--)
            {
                repositories[index].Dispose();
            }

            repositories.Clear();

            Assert.That(testRoot, Does.StartWith(TestRootPrefix));
            Assert.That(testRoot.Substring(TestRootPrefix.Length), Does.Not.Contain("/"));
            if (AssetDatabase.IsValidFolder(testRoot))
            {
                Assert.That(AssetDatabase.DeleteAsset(testRoot), Is.True);
            }
        }

        [Test]
        public void RecordsPersistAfterStoreIsSavedAndReloaded()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string folderGuid = CreateFolder("PersistentFolder");
            string iconGuid = CreateIconAsset("PersistentIcon.asset");

            Assert.That(repository.TrySetColor(folderGuid, new Color(0.2f, 0.4f, 0.6f, 0.8f)), Is.True);
            Assert.That(repository.TrySetIcon(folderGuid, FolderIconOverride.Custom, iconGuid), Is.True);
            string storePath = AssetDatabase.GetAssetPath(store);
            repository.Dispose();
            Resources.UnloadAsset(store);

            ProjectFolderMetadataStore reloaded =
                AssetDatabase.LoadAssetAtPath<ProjectFolderMetadataStore>(storePath);
            FolderMetadataRepository reloadedRepository = CreateRepository(reloaded);

            Assert.That(reloadedRepository.TryGet(folderGuid, out FolderMetadataSnapshot metadata), Is.True);
            Assert.That(metadata.HasColorOverride, Is.True);
            Assert.That(metadata.ColorOverride, Is.EqualTo(new Color(0.2f, 0.4f, 0.6f, 0.8f)));
            Assert.That(metadata.IconOverride, Is.EqualTo(FolderIconOverride.Custom));
            Assert.That(metadata.CustomIconGuid, Is.EqualTo(iconGuid));
        }

        [Test]
        public void FolderGuidIsThePersistentIdentity()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string folderGuid = CreateFolder("GuidIdentity");

            repository.TrySetColor(folderGuid, Color.red);

            Assert.That(store.Records.Count, Is.EqualTo(1));
            Assert.That(store.Records[0].FolderGuid, Is.EqualTo(folderGuid));
            Assert.That(store.Records[0].FolderGuid, Is.Not.EqualTo(AssetDatabase.GUIDToAssetPath(folderGuid)));
        }

        [Test]
        public void RenamePreservesMetadataAndUpdatesCachedPath()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string originalPath = testRoot + "/BeforeRename";
            string folderGuid = CreateFolder("BeforeRename");
            repository.TrySetColor(folderGuid, Color.green);

            Assert.That(AssetDatabase.RenameAsset(originalPath, "AfterRename"), Is.Empty);
            repository.RefreshForProjectChange();

            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot metadata), Is.True);
            Assert.That(metadata.CurrentPath, Is.EqualTo(testRoot + "/AfterRename"));
            Assert.That(metadata.ColorOverride, Is.EqualTo(Color.green));
        }

        [Test]
        public void MovePreservesMetadataAndUpdatesCachedPath()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string destinationGuid = AssetDatabase.CreateFolder(testRoot, "Destination");
            Assert.That(destinationGuid, Is.Not.Empty);
            string originalPath = testRoot + "/BeforeMove";
            string folderGuid = CreateFolder("BeforeMove");
            repository.TrySetColor(folderGuid, Color.blue);

            string movedPath = testRoot + "/Destination/BeforeMove";
            Assert.That(AssetDatabase.MoveAsset(originalPath, movedPath), Is.Empty);
            repository.RefreshForProjectChange();

            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot metadata), Is.True);
            Assert.That(metadata.CurrentPath, Is.EqualTo(movedPath));
            Assert.That(metadata.ColorOverride, Is.EqualTo(Color.blue));
        }

        [Test]
        public void TransparentColorIsDifferentFromNoColorOverride()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string folderGuid = CreateFolder("TransparentColor");

            Assert.That(repository.TryGet(folderGuid, out _), Is.False);
            Assert.That(repository.TrySetColor(folderGuid, new Color(0f, 0f, 0f, 0f)), Is.True);
            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot metadata), Is.True);
            Assert.That(metadata.HasColorOverride, Is.True);
            Assert.That(metadata.ColorOverride.a, Is.Zero);

            Assert.That(repository.TryClearColor(folderGuid), Is.True);
            Assert.That(repository.TryGet(folderGuid, out _), Is.False);
        }

        [Test]
        public void CustomIconStoresPortableAssetGuidAndDefaultIsExplicit()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string customFolderGuid = CreateFolder("CustomIconFolder");
            string defaultFolderGuid = CreateFolder("DefaultIconFolder");
            string iconGuid = CreateIconAsset("Icon.asset");
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuid));

            Assert.That(repository.TrySetIcon(customFolderGuid, FolderIconOverride.Custom, iconGuid), Is.True);
            Assert.That(repository.TrySetIcon(defaultFolderGuid, FolderIconOverride.Default), Is.True);

            Assert.That(repository.TryGet(customFolderGuid, out FolderMetadataSnapshot custom), Is.True);
            Assert.That(custom.CustomIconGuid, Is.EqualTo(iconGuid));
            Assert.That(custom.CustomIconGuid, Has.Length.EqualTo(32));
            Assert.That(AssetDatabase.GUIDToAssetPath(custom.CustomIconGuid), Is.EqualTo(AssetDatabase.GetAssetPath(icon)));
            Assert.That(repository.TryGet(defaultFolderGuid, out FolderMetadataSnapshot @default), Is.True);
            Assert.That(@default.IconOverride, Is.EqualTo(FolderIconOverride.Default));
            Assert.That(@default.CustomIconGuid, Is.Empty);
        }

        [Test]
        public void RemovedFolderIsIgnoredUntilExplicitOrphanCleanup()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string folderGuid = CreateFolder("RemovedFolder");
            repository.TrySetColor(folderGuid, Color.yellow);
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);

            Assert.That(AssetDatabase.DeleteAsset(folderPath), Is.True);
            repository.RefreshForProjectChange();

            Assert.DoesNotThrow(() => repository.TryGet(folderGuid, out _));
            Assert.That(repository.TryGet(folderGuid, out _), Is.False);
            Assert.That(store.Records.Count, Is.EqualTo(1), "A read/project refresh must not silently remove the orphan.");
            Assert.That(repository.FindOrphanedRecords(), Is.EquivalentTo(new[] { folderGuid }));

            Assert.That(repository.ClearOrphanedRecords(), Is.EqualTo(1));
            Assert.That(store.Records, Is.Empty);
            Assert.That(repository.FindOrphanedRecords(), Is.Empty);
        }

        [Test]
        public void StoreProjectAndUndoEventsExplicitlyInvalidateReadModel()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string folderGuid = CreateFolder("Invalidation");
            int initialVersion = repository.CacheVersion;

            FolderMetadataRecord record = store.GetOrCreate(folderGuid);
            record.HasColorOverride = true;
            record.ColorOverride = Color.magenta;
            store.NotifyChanged();
            int storeVersion = repository.CacheVersion;
            repository.RefreshForProjectChange();

            Assert.That(storeVersion, Is.GreaterThan(initialVersion));
            Assert.That(repository.CacheVersion, Is.GreaterThan(storeVersion));
            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot metadata), Is.True);
            Assert.That(metadata.ColorOverride, Is.EqualTo(Color.magenta));
        }

        [Test]
        public void ConfigurationChangesSupportUndoAndRedo()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string folderGuid = CreateFolder("UndoRedo");
            int undoGroup = Undo.GetCurrentGroup();

            Assert.That(repository.TrySetColor(folderGuid, Color.cyan), Is.True);
            Undo.FlushUndoRecordObjects();
            Undo.CollapseUndoOperations(undoGroup);
            Undo.PerformUndo();
            repository.RefreshForProjectChange();

            Assert.That(repository.TryGet(folderGuid, out _), Is.False);

            Undo.PerformRedo();
            repository.RefreshForProjectChange();

            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot metadata), Is.True);
            Assert.That(metadata.ColorOverride, Is.EqualTo(Color.cyan));
        }

        [Test]
        public void CachedQueriesDoNotResolvePathsOrWriteStore()
        {
            ProjectFolderMetadataStore store = ScriptableObject.CreateInstance<ProjectFolderMetadataStore>();
            int resolveCount = 0;
            int writeCount = 0;
            FolderMetadataRepository repository = new FolderMetadataRepository(
                store,
                guid =>
                {
                    resolveCount++;
                    return guid == "folder-guid" ? "Assets/CachedFolder" : "Assets/Icon.asset";
                },
                path => path == "Assets/CachedFolder",
                (target, action) => { },
                target => writeCount++);
            repositories.Add(repository);

            Assert.That(repository.TrySetColor("folder-guid", Color.white), Is.True);
            resolveCount = 0;
            writeCount = 0;

            Assert.That(repository.TryGet("folder-guid", out _), Is.True);
            Assert.That(repository.TryGet("folder-guid", out _), Is.True);

            Assert.That(resolveCount, Is.Zero);
            Assert.That(writeCount, Is.Zero);
            UnityEngine.Object.DestroyImmediate(store);
        }

        [Test]
        public void PackageFolderMutationFailsSafelyWithoutCreatingARecord()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string packageGuid = AssetDatabase.AssetPathToGUID("Packages/com.meganeura.project-toolkit/Specs");

            Assert.DoesNotThrow(() => repository.TrySetColor(packageGuid, Color.red));
            Assert.That(repository.TrySetColor(packageGuid, Color.red), Is.False);
            Assert.That(store.Records, Is.Empty);
        }

        private ProjectFolderMetadataStore CreateStore()
        {
            ProjectFolderMetadataStore store = ScriptableObject.CreateInstance<ProjectFolderMetadataStore>();
            AssetDatabase.CreateAsset(store, testRoot + "/FolderMetadataStore.asset");
            AssetDatabase.SaveAssetIfDirty(store);
            return store;
        }

        private FolderMetadataRepository CreateRepository(ProjectFolderMetadataStore store)
        {
            FolderMetadataRepository repository = new FolderMetadataRepository(store);
            repositories.Add(repository);
            return repository;
        }

        private string CreateFolder(string name)
        {
            string guid = AssetDatabase.CreateFolder(testRoot, name);
            Assert.That(guid, Is.Not.Empty);
            return guid;
        }

        private string CreateIconAsset(string fileName)
        {
            string path = testRoot + "/" + fileName;
            Texture2D texture = new Texture2D(1, 1);
            AssetDatabase.CreateAsset(texture, path);
            AssetDatabase.SaveAssetIfDirty(texture);
            string guid = AssetDatabase.AssetPathToGUID(path);
            Assert.That(guid, Is.Not.Empty);
            return guid;
        }
    }
}
