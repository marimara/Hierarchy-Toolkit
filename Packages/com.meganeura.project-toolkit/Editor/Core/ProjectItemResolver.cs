using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal sealed class ProjectItemResolver
    {
        private readonly Dictionary<string, ResolvedItem> cache = new Dictionary<string, ResolvedItem>();
        private readonly Func<string, string> guidToAssetPath;
        private readonly Func<string, bool> isValidFolder;

        internal ProjectItemResolver()
            : this(AssetDatabase.GUIDToAssetPath, AssetDatabase.IsValidFolder)
        {
        }

        internal ProjectItemResolver(Func<string, string> guidToAssetPath, Func<string, bool> isValidFolder)
        {
            this.guidToAssetPath = guidToAssetPath ?? throw new ArgumentNullException(nameof(guidToAssetPath));
            this.isValidFolder = isValidFolder ?? throw new ArgumentNullException(nameof(isValidFolder));
        }

        internal ProjectItemContext Resolve(string assetGuid, Rect rect)
        {
            if (string.IsNullOrEmpty(assetGuid))
            {
                return new ProjectItemContext(assetGuid, string.Empty, ProjectItemKind.Unknown, rect);
            }

            if (!cache.TryGetValue(assetGuid, out ResolvedItem item))
            {
                string path = guidToAssetPath(assetGuid);
                ProjectItemKind kind = string.IsNullOrEmpty(path)
                    ? ProjectItemKind.Unknown
                    : isValidFolder(path) ? ProjectItemKind.Folder : ProjectItemKind.Asset;

                item = new ResolvedItem(path, kind);
                cache.Add(assetGuid, item);
            }

            return new ProjectItemContext(assetGuid, item.AssetPath, item.Kind, rect);
        }

        internal void Invalidate()
        {
            cache.Clear();
        }

        private readonly struct ResolvedItem
        {
            internal ResolvedItem(string assetPath, ProjectItemKind kind)
            {
                AssetPath = assetPath;
                Kind = kind;
            }

            internal string AssetPath { get; }

            internal ProjectItemKind Kind { get; }
        }
    }
}
