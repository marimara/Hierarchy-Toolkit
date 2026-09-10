using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit.Tests
{
    internal sealed class ManualFolderColorTests
    {
        private const string TestRootPrefix = "Assets/__MeganeuraProjectToolkitSpec003Tests_";
        private readonly List<FolderMetadataRepository> repositories = new List<FolderMetadataRepository>();
        private readonly List<ManualFolderColorFeature> features = new List<ManualFolderColorFeature>();
        private string testRoot;

        [SetUp]
        public void SetUp()
        {
            testRoot = TestRootPrefix + Guid.NewGuid().ToString("N");
            string guid = AssetDatabase.CreateFolder("Assets", testRoot.Substring("Assets/".Length));
            Assert.That(guid, Is.Not.Empty, "The isolated Spec 003 test folder could not be created.");
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = features.Count - 1; index >= 0; index--)
            {
                features[index].Dispose();
            }

            features.Clear();
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
        public void SetAndClearColorPreservesOtherFolderMetadata()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string folderGuid = CreateFolder("ColoredFolder");
            string iconGuid = CreateTextureAsset("Icon.asset");
            Color color = new Color(0.18f, 0.42f, 0.76f, 0.55f);

            Assert.That(repository.TrySetIcon(folderGuid, FolderIconOverride.Custom, iconGuid), Is.True);
            Assert.That(repository.TrySetColor(folderGuid, color), Is.True);
            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot colored), Is.True);
            Assert.That(colored.HasColorOverride, Is.True);
            Assert.That(colored.ColorOverride, Is.EqualTo(color));

            Assert.That(repository.TryClearColor(folderGuid), Is.True);
            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot cleared), Is.True);
            Assert.That(cleared.HasColorOverride, Is.False);
            Assert.That(cleared.IconOverride, Is.EqualTo(FolderIconOverride.Custom));
            Assert.That(cleared.CustomIconGuid, Is.EqualTo(iconGuid));
        }

        [Test]
        public void FullyTransparentColorRemainsAValidDrawableOverride()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            string folderGuid = CreateFolder("TransparentFolder");
            Color transparent = new Color(0.7f, 0.2f, 0.4f, 0f);
            ManualFolderColorFeature feature = CreateFeature(repository);

            Assert.That(repository.TrySetColor(folderGuid, transparent), Is.True);
            ManualFolderColorDrawDecision decision = feature.GetDrawDecision(
                new ProjectItemContext(folderGuid, testRoot + "/TransparentFolder", ProjectItemKind.Folder, new Rect(0f, 0f, 120f, 18f)),
                18f);

            Assert.That(decision.ShouldDraw, Is.True);
            Assert.That(decision.Color, Is.EqualTo(transparent));
            Assert.That(decision.Color.a, Is.Zero);
        }

        [Test]
        public void EmptyGuidAssetAndPackageFolderAreRejected()
        {
            ProjectFolderMetadataStore store = CreateStore();
            FolderMetadataRepository repository = CreateRepository(store);
            string assetGuid = CreateTextureAsset("OrdinaryAsset.asset");
            string packageGuid = AssetDatabase.AssetPathToGUID("Packages/com.meganeura.project-toolkit/Specs");

            Assert.That(repository.TrySetColor(string.Empty, Color.red), Is.False);
            Assert.That(repository.TrySetColor(assetGuid, Color.red), Is.False);
            Assert.That(repository.TrySetColor(packageGuid, Color.red), Is.False);
            Assert.That(store.Records, Is.Empty);
        }

        [Test]
        public void NonEditableAssetFolderIsRejected()
        {
            ProjectFolderMetadataStore store = ScriptableObject.CreateInstance<ProjectFolderMetadataStore>();
            FolderMetadataRepository repository = new FolderMetadataRepository(
                store,
                guid => guid == "locked" ? "Assets/Locked" : string.Empty,
                path => path == "Assets/Locked",
                (target, action) => { },
                target => { },
                path => false);
            repositories.Add(repository);

            Assert.That(repository.TrySetColor("locked", Color.red), Is.False);
            Assert.That(store.Records, Is.Empty);
            UnityEngine.Object.DestroyImmediate(store);
        }

        [Test]
        public void ClickedSelectedFolderColorsAllEligibleFoldersInMixedSelection()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            ManualFolderColorFeature feature = CreateFeature(repository);
            string firstFolder = CreateFolder("First");
            string secondFolder = CreateFolder("Second");
            string asset = CreateTextureAsset("Mixed.asset");
            string[] selection = { firstFolder, asset, secondFolder };
            string[] originalSelection = (string[])selection.Clone();

            Assert.That(feature.ApplyColor(firstFolder, selection, Color.cyan), Is.True);

            Assert.That(repository.TryGet(firstFolder, out FolderMetadataSnapshot first), Is.True);
            Assert.That(repository.TryGet(secondFolder, out FolderMetadataSnapshot second), Is.True);
            Assert.That(repository.TryGet(asset, out _), Is.False);
            Assert.That(first.ColorOverride, Is.EqualTo(Color.cyan));
            Assert.That(second.ColorOverride, Is.EqualTo(Color.cyan));
            Assert.That(selection, Is.EqualTo(originalSelection), "Target resolution must not mutate selection.");
        }

        [Test]
        public void SelectedFolderOnlySelectionColorsEveryFolder()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            ManualFolderColorFeature feature = CreateFeature(repository);
            string firstFolder = CreateFolder("OnlyFirst");
            string secondFolder = CreateFolder("OnlySecond");

            Assert.That(feature.ApplyColor(firstFolder, new[] { firstFolder, secondFolder }, Color.red), Is.True);

            Assert.That(repository.TryGet(firstFolder, out FolderMetadataSnapshot first), Is.True);
            Assert.That(repository.TryGet(secondFolder, out FolderMetadataSnapshot second), Is.True);
            Assert.That(first.ColorOverride, Is.EqualTo(Color.red));
            Assert.That(second.ColorOverride, Is.EqualTo(Color.red));
        }

        [Test]
        public void ClickedUnselectedFolderTargetsOnlyClickedFolder()
        {
            string clickedFolder = CreateFolder("Clicked");
            string selectedFolder = CreateFolder("Selected");

            IReadOnlyList<string> targets = ManualFolderColorTargets.Resolve(
                clickedFolder,
                new[] { selectedFolder },
                guid => guid == clickedFolder || guid == selectedFolder);

            Assert.That(targets, Is.EqualTo(new[] { clickedFolder }));
        }

        [Test]
        public void MultiFolderSetIsOneRepositoryChangeAndOneUndoRecord()
        {
            ProjectFolderMetadataStore store = ScriptableObject.CreateInstance<ProjectFolderMetadataStore>();
            int undoRecords = 0;
            int persists = 0;
            HashSet<string> folders = new HashSet<string> { "first", "second" };
            FolderMetadataRepository repository = new FolderMetadataRepository(
                store,
                guid => folders.Contains(guid) ? "Assets/" + guid : string.Empty,
                path => path == "Assets/first" || path == "Assets/second",
                (target, action) => undoRecords++,
                target => persists++);
            repositories.Add(repository);

            Assert.That(repository.TrySetColors(new[] { "first", "second", "first" }, Color.cyan), Is.True);

            Assert.That(undoRecords, Is.EqualTo(1));
            Assert.That(persists, Is.EqualTo(1));
            Assert.That(store.Records.Count, Is.EqualTo(2));
            UnityEngine.Object.DestroyImmediate(store);
        }

        [Test]
        public void MultiFolderOperationUndoesAndRedoesAsOneLogicalStep()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            string firstFolder = CreateFolder("UndoFirst");
            string secondFolder = CreateFolder("UndoSecond");
            int undoGroup = Undo.GetCurrentGroup();

            Assert.That(repository.TrySetColors(new[] { firstFolder, secondFolder }, Color.yellow), Is.True);
            Undo.FlushUndoRecordObjects();
            Undo.CollapseUndoOperations(undoGroup);
            Undo.PerformUndo();

            Assert.That(repository.TryGet(firstFolder, out _), Is.False);
            Assert.That(repository.TryGet(secondFolder, out _), Is.False);

            Undo.PerformRedo();

            Assert.That(repository.TryGet(firstFolder, out FolderMetadataSnapshot first), Is.True);
            Assert.That(repository.TryGet(secondFolder, out FolderMetadataSnapshot second), Is.True);
            Assert.That(first.ColorOverride, Is.EqualTo(Color.yellow));
            Assert.That(second.ColorOverride, Is.EqualTo(Color.yellow));
        }

        [Test]
        public void ApplyingColorInvalidatesCacheAndRequestsImmediateRepaint()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            string folderGuid = CreateFolder("Immediate");
            int initialVersion = repository.CacheVersion;
            int repaintCount = 0;
            ManualFolderColorFeature feature = CreateFeature(repository, () => repaintCount++);

            Assert.That(feature.ApplyColor(folderGuid, new[] { folderGuid }, Color.green), Is.True);

            Assert.That(repository.CacheVersion, Is.GreaterThan(initialVersion));
            Assert.That(repaintCount, Is.EqualTo(1));
            Assert.That(repository.TryGet(folderGuid, out FolderMetadataSnapshot metadata), Is.True);
            Assert.That(metadata.ColorOverride, Is.EqualTo(Color.green));
        }

        [Test]
        public void DrawDecisionSupportsListAndGridAndSkipsUnsetOrNonFolderItems()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            string coloredGuid = CreateFolder("Drawn");
            string unsetGuid = CreateFolder("Unset");
            repository.TrySetColor(coloredGuid, Color.magenta);
            ManualFolderColorFeature feature = CreateFeature(repository);

            ManualFolderColorDrawDecision list = feature.GetDrawDecision(
                new ProjectItemContext(coloredGuid, testRoot + "/Drawn", ProjectItemKind.Folder, new Rect(0f, 0f, 140f, 18f)),
                18f);
            ManualFolderColorDrawDecision grid = feature.GetDrawDecision(
                new ProjectItemContext(coloredGuid, testRoot + "/Drawn", ProjectItemKind.Folder, new Rect(0f, 0f, 84f, 102f)),
                18f);
            ManualFolderColorDrawDecision unset = feature.GetDrawDecision(
                new ProjectItemContext(unsetGuid, testRoot + "/Unset", ProjectItemKind.Folder, new Rect(0f, 0f, 140f, 18f)),
                18f);
            ManualFolderColorDrawDecision asset = feature.GetDrawDecision(
                new ProjectItemContext(coloredGuid, testRoot + "/file.asset", ProjectItemKind.Asset, new Rect(0f, 0f, 140f, 18f)),
                18f);

            Assert.That(list.Presentation, Is.EqualTo(ProjectItemPresentation.ListOrTree));
            Assert.That(grid.Presentation, Is.EqualTo(ProjectItemPresentation.Grid));
            Assert.That(unset.ShouldDraw, Is.False);
            Assert.That(asset.ShouldDraw, Is.False);
            Assert.That(ManualFolderColorLayout.Classify(new Rect(0f, 0f, 20f, 28f), 18f), Is.EqualTo(ProjectItemPresentation.Unsupported));
        }

        [Test]
        public void ListLayoutUsesNativeIconSizeAndCentersItVertically()
        {
            Rect itemRect = new Rect(11f, 7f, 180f, 18f);

            Assert.That(ManualFolderColorLayout.TryGetIconRect(itemRect, 18f, out ProjectItemPresentation presentation, out Rect iconRect), Is.True);

            Assert.That(presentation, Is.EqualTo(ProjectItemPresentation.ListOrTree));
            Assert.That(iconRect.width, Is.EqualTo(16f));
            Assert.That(iconRect.height, Is.EqualTo(16f));
            Assert.That(iconRect.xMin, Is.EqualTo(itemRect.xMin));
            Assert.That(iconRect.center.y, Is.EqualTo(itemRect.center.y));
        }

        [TestCase(32f)]
        [TestCase(64f)]
        [TestCase(96f)]
        public void GridLayoutUsesNativeSquareAndPreservesLabelRemainder(float iconSize)
        {
            const float labelRemainder = 14f;
            Rect itemRect = new Rect(13f, 17f, iconSize, iconSize + labelRemainder);

            Assert.That(ManualFolderColorLayout.TryGetIconRect(itemRect, 18f, out ProjectItemPresentation presentation, out Rect iconRect), Is.True);

            Assert.That(presentation, Is.EqualTo(ProjectItemPresentation.Grid));
            Assert.That(iconRect.size, Is.EqualTo(new Vector2(iconSize, iconSize)));
            Assert.That(iconRect.center.x, Is.EqualTo(itemRect.center.x));
            Assert.That(iconRect.yMin - itemRect.yMin, Is.EqualTo(labelRemainder * 0.5f));
            Assert.That(itemRect.yMax - iconRect.yMax, Is.EqualTo(labelRemainder * 0.5f));
        }

        [TestCase(20f, 28f)]
        [TestCase(31f, 45f)]
        [TestCase(64f, 70f)]
        [TestCase(64f, 90f)]
        public void AmbiguousGeometryReturnsSafeFallback(float width, float height)
        {
            Assert.That(ManualFolderColorLayout.TryGetIconRect(
                new Rect(0f, 0f, width, height),
                18f,
                out ProjectItemPresentation presentation,
                out _), Is.False);
            Assert.That(presentation, Is.EqualTo(ProjectItemPresentation.Unsupported));
        }

        [Test]
        public void FolderTextureIsResolvedOnlyOnceAcrossRepeatedInitialization()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            int resolutions = 0;
            Texture2D texture = new Texture2D(1, 1);
            ManualFolderColorFeature feature = new ManualFolderColorFeature(
                repository,
                () => { },
                callback => { },
                callback => { },
                () =>
                {
                    resolutions++;
                    return texture;
                });
            features.Add(feature);

            feature.Initialize();
            feature.Initialize();

            Assert.That(resolutions, Is.EqualTo(1));
            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void RepeatedDrawDecisionsUseCachedSnapshotWithoutPathLookupOrWrite()
        {
            ProjectFolderMetadataStore store = ScriptableObject.CreateInstance<ProjectFolderMetadataStore>();
            int pathLookups = 0;
            int writes = 0;
            FolderMetadataRepository repository = new FolderMetadataRepository(
                store,
                guid =>
                {
                    pathLookups++;
                    return guid == "folder" ? "Assets/folder" : string.Empty;
                },
                path => path == "Assets/folder",
                (target, action) => { },
                target => writes++);
            repositories.Add(repository);
            repository.TrySetColor("folder", Color.blue);
            pathLookups = 0;
            writes = 0;
            ManualFolderColorFeature feature = CreateFeature(repository);
            ProjectItemContext item = new ProjectItemContext("folder", "Assets/folder", ProjectItemKind.Folder, new Rect(0f, 0f, 120f, 18f));

            Assert.That(feature.GetDrawDecision(item, 18f).ShouldDraw, Is.True);
            Assert.That(feature.GetDrawDecision(item, 18f).ShouldDraw, Is.True);

            Assert.That(pathLookups, Is.Zero);
            Assert.That(writes, Is.Zero);
            UnityEngine.Object.DestroyImmediate(store);
        }

        [Test]
        public void InitializeAndDisposeDoNotDuplicateUndoCallbacks()
        {
            FolderMetadataRepository repository = CreateRepository(CreateStore());
            int subscriptions = 0;
            int unsubscriptions = 0;
            ManualFolderColorFeature feature = new ManualFolderColorFeature(
                repository,
                () => { },
                callback => subscriptions++,
                callback => unsubscriptions++);
            features.Add(feature);

            feature.Initialize();
            feature.Initialize();
            feature.Dispose();
            feature.Dispose();

            Assert.That(subscriptions, Is.EqualTo(1));
            Assert.That(unsubscriptions, Is.EqualTo(1));
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

        private ManualFolderColorFeature CreateFeature(FolderMetadataRepository repository, Action repaint = null)
        {
            ManualFolderColorFeature feature = new ManualFolderColorFeature(
                repository,
                repaint ?? (() => { }),
                callback => { },
                callback => { });
            features.Add(feature);
            return feature;
        }

        private string CreateFolder(string name)
        {
            string guid = AssetDatabase.CreateFolder(testRoot, name);
            Assert.That(guid, Is.Not.Empty);
            return guid;
        }

        private string CreateTextureAsset(string fileName)
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
