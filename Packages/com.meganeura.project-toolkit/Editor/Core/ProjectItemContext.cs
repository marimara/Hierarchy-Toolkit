using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal enum ProjectItemKind
    {
        Unknown,
        Asset,
        Folder
    }

    internal readonly struct ProjectItemContext
    {
        internal ProjectItemContext(string assetGuid, string assetPath, ProjectItemKind kind, Rect rect)
        {
            AssetGuid = assetGuid;
            AssetPath = assetPath;
            Kind = kind;
            Rect = rect;
        }

        internal string AssetGuid { get; }

        internal string AssetPath { get; }

        internal ProjectItemKind Kind { get; }

        internal bool IsFolder => Kind == ProjectItemKind.Folder;

        internal Rect Rect { get; }
    }
}
