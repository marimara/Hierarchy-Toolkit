using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit.Tests
{
    internal sealed class FoundationTests
    {
        [Test]
        public void MultipleFeaturesReceiveOneInvocationPerProjectItem()
        {
            FakeIntegration integration = new FakeIntegration();
            ProjectFeatureCoordinator coordinator = CreateCoordinator(integration);
            TrackingFeature first = new TrackingFeature(true);
            TrackingFeature second = new TrackingFeature(true);

            coordinator.Register(first);
            coordinator.Register(second);
            integration.RaiseItemGUI("folder-guid", new Rect(1f, 2f, 3f, 4f));

            Assert.That(first.InitializeCount, Is.EqualTo(1));
            Assert.That(second.InitializeCount, Is.EqualTo(1));
            Assert.That(first.DrawCount, Is.EqualTo(1));
            Assert.That(second.DrawCount, Is.EqualTo(1));
            Assert.That(first.LastItem.IsFolder, Is.True);
            Assert.That(second.LastItem.IsFolder, Is.True);

            coordinator.Dispose();
        }

        [Test]
        public void RegisteringSameFeatureTwiceDoesNotDuplicateLifecycleOrDrawing()
        {
            FakeIntegration integration = new FakeIntegration();
            ProjectFeatureCoordinator coordinator = CreateCoordinator(integration);
            TrackingFeature feature = new TrackingFeature(true);

            Assert.That(coordinator.Register(feature), Is.True);
            Assert.That(coordinator.Register(feature), Is.False);
            integration.RaiseItemGUI("asset-guid", Rect.zero);

            Assert.That(feature.InitializeCount, Is.EqualTo(1));
            Assert.That(feature.DrawCount, Is.EqualTo(1));

            coordinator.Dispose();
        }

        [Test]
        public void DisposeRemovesCallbacksAndDisposesRegisteredFeaturesOnce()
        {
            FakeIntegration integration = new FakeIntegration();
            ProjectFeatureCoordinator coordinator = CreateCoordinator(integration);
            TrackingFeature feature = new TrackingFeature(true);
            coordinator.Register(feature);

            coordinator.Dispose();
            coordinator.Dispose();
            integration.RaiseItemGUI("asset-guid", Rect.zero);
            integration.RaiseProjectChanged();

            Assert.That(integration.ItemSubscriberCount, Is.Zero);
            Assert.That(integration.ProjectChangedSubscriberCount, Is.Zero);
            Assert.That(integration.IsDisposed, Is.True);
            Assert.That(feature.DisposeCount, Is.EqualTo(1));
            Assert.That(feature.DrawCount, Is.Zero);
        }

        [Test]
        public void DisabledFeaturesDoNotResolveOrDrawProjectItems()
        {
            int lookupCount = 0;
            FakeIntegration integration = new FakeIntegration();
            ProjectItemResolver resolver = new ProjectItemResolver(
                guid =>
                {
                    lookupCount++;
                    return "Assets/Folder";
                },
                path => true);
            ProjectFeatureCoordinator coordinator = new ProjectFeatureCoordinator(integration, resolver);
            TrackingFeature feature = new TrackingFeature(false);
            coordinator.Register(feature);

            integration.RaiseItemGUI("folder-guid", Rect.zero);

            Assert.That(feature.DrawCount, Is.Zero);
            Assert.That(lookupCount, Is.Zero);

            coordinator.Dispose();
        }

        [Test]
        public void ResolverClassifiesFoldersAssetsAndUnknownItems()
        {
            ProjectItemResolver resolver = new ProjectItemResolver(
                guid => guid == "folder-guid"
                    ? "Assets/Folder"
                    : guid == "asset-guid" ? "Assets/Folder/file.txt" : string.Empty,
                path => path == "Assets/Folder");

            ProjectItemContext folder = resolver.Resolve("folder-guid", Rect.zero);
            ProjectItemContext asset = resolver.Resolve("asset-guid", Rect.zero);
            ProjectItemContext unknown = resolver.Resolve("missing-guid", Rect.zero);

            Assert.That(folder.Kind, Is.EqualTo(ProjectItemKind.Folder));
            Assert.That(folder.IsFolder, Is.True);
            Assert.That(asset.Kind, Is.EqualTo(ProjectItemKind.Asset));
            Assert.That(asset.IsFolder, Is.False);
            Assert.That(unknown.Kind, Is.EqualTo(ProjectItemKind.Unknown));
        }

        [Test]
        public void DefaultResolverClassifiesRealPackageFolderAndAsset()
        {
            const string folderPath = "Packages/com.meganeura.project-toolkit/Specs";
            const string assetPath = folderPath + "/README.md";
            string folderGuid = AssetDatabase.AssetPathToGUID(folderPath);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            ProjectItemResolver resolver = new ProjectItemResolver();

            ProjectItemContext folder = resolver.Resolve(folderGuid, Rect.zero);
            ProjectItemContext asset = resolver.Resolve(assetGuid, Rect.zero);

            Assert.That(folder.AssetPath, Is.EqualTo(folderPath));
            Assert.That(folder.Kind, Is.EqualTo(ProjectItemKind.Folder));
            Assert.That(asset.AssetPath, Is.EqualTo(assetPath));
            Assert.That(asset.Kind, Is.EqualTo(ProjectItemKind.Asset));
        }

        [Test]
        public void ProjectChangeInvalidatesTransientGuidLookupCache()
        {
            int lookupCount = 0;
            FakeIntegration integration = new FakeIntegration();
            ProjectItemResolver resolver = new ProjectItemResolver(
                guid =>
                {
                    lookupCount++;
                    return "Assets/Folder";
                },
                path => true);
            ProjectFeatureCoordinator coordinator = new ProjectFeatureCoordinator(integration, resolver);
            coordinator.Register(new TrackingFeature(true));

            integration.RaiseItemGUI("folder-guid", Rect.zero);
            integration.RaiseItemGUI("folder-guid", Rect.zero);
            integration.RaiseProjectChanged();
            integration.RaiseItemGUI("folder-guid", Rect.zero);

            Assert.That(lookupCount, Is.EqualTo(2));

            coordinator.Dispose();
        }

        private static ProjectFeatureCoordinator CreateCoordinator(FakeIntegration integration)
        {
            ProjectItemResolver resolver = new ProjectItemResolver(
                guid => guid == "folder-guid" ? "Assets/Folder" : "Assets/file.txt",
                path => path == "Assets/Folder");
            return new ProjectFeatureCoordinator(integration, resolver);
        }

        private sealed class TrackingFeature : IProjectFeature
        {
            internal TrackingFeature(bool isEnabled)
            {
                IsEnabled = isEnabled;
            }

            public bool IsEnabled { get; }

            internal int InitializeCount { get; private set; }

            internal int DrawCount { get; private set; }

            internal int DisposeCount { get; private set; }

            internal ProjectItemContext LastItem { get; private set; }

            public void Initialize()
            {
                InitializeCount++;
            }

            public void OnProjectItemGUI(ProjectItemContext item)
            {
                LastItem = item;
                DrawCount++;
            }

            public void Dispose()
            {
                DisposeCount++;
            }
        }

        private sealed class FakeIntegration : IProjectWindowIntegration
        {
            private Action<string, Rect> itemGUI;
            private Action projectChanged;

            public event Action<string, Rect> ItemGUI
            {
                add => itemGUI += value;
                remove => itemGUI -= value;
            }

            public event Action ProjectChanged
            {
                add => projectChanged += value;
                remove => projectChanged -= value;
            }

            internal int ItemSubscriberCount => itemGUI?.GetInvocationList().Length ?? 0;

            internal int ProjectChangedSubscriberCount => projectChanged?.GetInvocationList().Length ?? 0;

            internal bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }

            internal void RaiseItemGUI(string guid, Rect rect)
            {
                itemGUI?.Invoke(guid, rect);
            }

            internal void RaiseProjectChanged()
            {
                projectChanged?.Invoke();
            }
        }
    }
}
