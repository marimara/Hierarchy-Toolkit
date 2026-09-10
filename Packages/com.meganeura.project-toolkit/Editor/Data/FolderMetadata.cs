using System;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal enum FolderIconOverride
    {
        Unset,
        Default,
        Custom
    }

    [Serializable]
    internal sealed class FolderMetadataRecord
    {
        [SerializeField] private string folderGuid;
        [SerializeField] private bool hasColorOverride;
        [SerializeField] private Color colorOverride;
        [SerializeField] private FolderIconOverride iconOverride;
        [SerializeField] private string customIconGuid;

        internal FolderMetadataRecord(string folderGuid)
        {
            this.folderGuid = folderGuid;
        }

        internal string FolderGuid => folderGuid;

        internal bool HasColorOverride
        {
            get => hasColorOverride;
            set => hasColorOverride = value;
        }

        internal Color ColorOverride
        {
            get => colorOverride;
            set => colorOverride = value;
        }

        internal FolderIconOverride IconOverride
        {
            get => iconOverride;
            set => iconOverride = value;
        }

        internal string CustomIconGuid
        {
            get => customIconGuid;
            set => customIconGuid = value;
        }

        internal bool IsEmpty => !hasColorOverride && iconOverride == FolderIconOverride.Unset;
    }

    internal readonly struct FolderMetadataSnapshot
    {
        internal FolderMetadataSnapshot(FolderMetadataRecord record, string currentPath, bool isAvailable)
        {
            FolderGuid = record.FolderGuid;
            CurrentPath = currentPath;
            IsAvailable = isAvailable;
            HasColorOverride = record.HasColorOverride;
            ColorOverride = record.ColorOverride;
            IconOverride = record.IconOverride;
            CustomIconGuid = record.CustomIconGuid;
        }

        internal string FolderGuid { get; }

        internal string CurrentPath { get; }

        internal bool IsAvailable { get; }

        internal bool HasColorOverride { get; }

        internal Color ColorOverride { get; }

        internal FolderIconOverride IconOverride { get; }

        internal string CustomIconGuid { get; }
    }
}
