using System;
using System.Collections.Generic;

namespace Meganeura.ProjectToolkit
{
    internal sealed class ProjectFeatureCoordinator : IDisposable
    {
        private readonly List<IProjectFeature> features = new List<IProjectFeature>();
        private readonly IProjectWindowIntegration integration;
        private readonly ProjectItemResolver itemResolver;
        private bool isDisposed;

        internal ProjectFeatureCoordinator(IProjectWindowIntegration integration, ProjectItemResolver itemResolver)
        {
            this.integration = integration ?? throw new ArgumentNullException(nameof(integration));
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));

            integration.ItemGUI += OnProjectItemGUI;
            integration.ProjectChanged += OnProjectChanged;
        }

        internal bool Register(IProjectFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            ThrowIfDisposed();

            for (int index = 0; index < features.Count; index++)
            {
                if (ReferenceEquals(features[index], feature))
                {
                    return false;
                }
            }

            features.Add(feature);
            feature.Initialize();
            return true;
        }

        internal bool Unregister(IProjectFeature feature)
        {
            if (feature == null || isDisposed)
            {
                return false;
            }

            for (int index = 0; index < features.Count; index++)
            {
                if (!ReferenceEquals(features[index], feature))
                {
                    continue;
                }

                features.RemoveAt(index);
                feature.Dispose();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            integration.ItemGUI -= OnProjectItemGUI;
            integration.ProjectChanged -= OnProjectChanged;
            integration.Dispose();

            for (int index = features.Count - 1; index >= 0; index--)
            {
                features[index].Dispose();
            }

            features.Clear();
            itemResolver.Invalidate();
        }

        private void OnProjectItemGUI(string assetGuid, UnityEngine.Rect rect)
        {
            ProjectItemContext item = default;
            bool itemResolved = false;

            for (int index = 0; index < features.Count; index++)
            {
                IProjectFeature feature = features[index];
                if (!feature.IsEnabled)
                {
                    continue;
                }

                if (!itemResolved)
                {
                    item = itemResolver.Resolve(assetGuid, rect);
                    itemResolved = true;
                }

                feature.OnProjectItemGUI(item);
            }
        }

        private void OnProjectChanged()
        {
            itemResolver.Invalidate();
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(ProjectFeatureCoordinator));
            }
        }
    }
}
