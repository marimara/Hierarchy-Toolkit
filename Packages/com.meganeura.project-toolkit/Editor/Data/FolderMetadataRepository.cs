using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal sealed class FolderMetadataRepository : IDisposable
    {
        private readonly Dictionary<string, FolderMetadataSnapshot> readModel =
            new Dictionary<string, FolderMetadataSnapshot>(StringComparer.Ordinal);
        private readonly ProjectFolderMetadataStore store;
        private readonly Func<string, string> guidToAssetPath;
        private readonly Func<string, bool> isValidFolder;
        private readonly Action<ProjectFolderMetadataStore, string> recordUndo;
        private readonly Action<ProjectFolderMetadataStore> persist;
        private bool isDisposed;

        internal FolderMetadataRepository(ProjectFolderMetadataStore store)
            : this(
                store,
                AssetDatabase.GUIDToAssetPath,
                AssetDatabase.IsValidFolder,
                (target, actionName) => Undo.RecordObject(target, actionName),
                target =>
                {
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssetIfDirty(target);
                })
        {
        }

        internal FolderMetadataRepository(
            ProjectFolderMetadataStore store,
            Func<string, string> guidToAssetPath,
            Func<string, bool> isValidFolder,
            Action<ProjectFolderMetadataStore, string> recordUndo,
            Action<ProjectFolderMetadataStore> persist)
        {
            this.store = store != null ? store : throw new ArgumentNullException(nameof(store));
            this.guidToAssetPath = guidToAssetPath ?? throw new ArgumentNullException(nameof(guidToAssetPath));
            this.isValidFolder = isValidFolder ?? throw new ArgumentNullException(nameof(isValidFolder));
            this.recordUndo = recordUndo ?? throw new ArgumentNullException(nameof(recordUndo));
            this.persist = persist ?? throw new ArgumentNullException(nameof(persist));

            store.Changed += OnStoreChanged;
            EditorApplication.projectChanged += OnProjectChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
            RebuildReadModel();
        }

        internal int CacheVersion { get; private set; }

        internal bool TryGet(string folderGuid, out FolderMetadataSnapshot metadata)
        {
            if (!string.IsNullOrEmpty(folderGuid)
                && readModel.TryGetValue(folderGuid, out metadata)
                && metadata.IsAvailable)
            {
                return true;
            }

            metadata = default;
            return false;
        }

        internal bool TrySetColor(string folderGuid, Color color)
        {
            if (!CanEditFolder(folderGuid))
            {
                return false;
            }

            FolderMetadataRecord existing = store.Find(folderGuid);
            if (existing != null && existing.HasColorOverride && existing.ColorOverride == color)
            {
                return true;
            }

            BeginChange("Set Project Toolkit Folder Color");
            FolderMetadataRecord record = store.GetOrCreate(folderGuid);
            record.HasColorOverride = true;
            record.ColorOverride = color;
            CompleteChange();
            return true;
        }

        internal bool TryClearColor(string folderGuid)
        {
            FolderMetadataRecord record = store.Find(folderGuid);
            if (record == null || !record.HasColorOverride || !CanEditFolder(folderGuid))
            {
                return false;
            }

            BeginChange("Clear Project Toolkit Folder Color");
            record.HasColorOverride = false;
            RemoveIfEmpty(record);
            CompleteChange();
            return true;
        }

        internal bool TrySetIcon(string folderGuid, FolderIconOverride iconOverride, string customIconGuid = null)
        {
            if (!CanEditFolder(folderGuid)
                || iconOverride == FolderIconOverride.Unset
                || (iconOverride == FolderIconOverride.Custom && !IsPortableAssetGuid(customIconGuid)))
            {
                return false;
            }

            string storedIconGuid = iconOverride == FolderIconOverride.Custom ? customIconGuid : string.Empty;
            FolderMetadataRecord existing = store.Find(folderGuid);
            if (existing != null
                && existing.IconOverride == iconOverride
                && string.Equals(existing.CustomIconGuid, storedIconGuid, StringComparison.Ordinal))
            {
                return true;
            }

            BeginChange("Set Project Toolkit Folder Icon");
            FolderMetadataRecord record = store.GetOrCreate(folderGuid);
            record.IconOverride = iconOverride;
            record.CustomIconGuid = storedIconGuid;
            CompleteChange();
            return true;
        }

        internal bool TryClearIcon(string folderGuid)
        {
            FolderMetadataRecord record = store.Find(folderGuid);
            if (record == null || record.IconOverride == FolderIconOverride.Unset || !CanEditFolder(folderGuid))
            {
                return false;
            }

            BeginChange("Clear Project Toolkit Folder Icon");
            record.IconOverride = FolderIconOverride.Unset;
            record.CustomIconGuid = string.Empty;
            RemoveIfEmpty(record);
            CompleteChange();
            return true;
        }

        internal IReadOnlyList<string> FindOrphanedRecords()
        {
            List<string> orphanedGuids = new List<string>();
            IReadOnlyList<FolderMetadataRecord> records = store.Records;
            for (int index = 0; index < records.Count; index++)
            {
                FolderMetadataRecord record = records[index];
                if (record == null
                    || !readModel.TryGetValue(record.FolderGuid, out FolderMetadataSnapshot metadata)
                    || !metadata.IsAvailable)
                {
                    orphanedGuids.Add(record?.FolderGuid ?? string.Empty);
                }
            }

            return orphanedGuids;
        }

        internal int ClearOrphanedRecords()
        {
            IReadOnlyList<string> orphaned = FindOrphanedRecords();
            if (orphaned.Count == 0)
            {
                return 0;
            }

            HashSet<string> orphanedGuids = new HashSet<string>(orphaned, StringComparer.Ordinal);
            BeginChange("Clean Project Toolkit Orphaned Folder Metadata");
            int removed = store.RemoveAll(orphanedGuids);
            CompleteChange();
            return removed;
        }

        internal void RefreshForProjectChange()
        {
            RebuildReadModel();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            store.Changed -= OnStoreChanged;
            EditorApplication.projectChanged -= OnProjectChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            readModel.Clear();
        }

        private bool CanEditFolder(string folderGuid)
        {
            if (string.IsNullOrEmpty(folderGuid))
            {
                return false;
            }

            string path = guidToAssetPath(folderGuid);
            return !string.IsNullOrEmpty(path)
                && (string.Equals(path, "Assets", StringComparison.Ordinal)
                    || path.StartsWith("Assets/", StringComparison.Ordinal))
                && isValidFolder(path);
        }

        private bool IsPortableAssetGuid(string assetGuid)
        {
            return !string.IsNullOrEmpty(assetGuid)
                && !string.IsNullOrEmpty(guidToAssetPath(assetGuid));
        }

        private void BeginChange(string actionName)
        {
            recordUndo(store, actionName);
        }

        private void CompleteChange()
        {
            persist(store);
            store.NotifyChanged();
        }

        private void RemoveIfEmpty(FolderMetadataRecord record)
        {
            if (record.IsEmpty)
            {
                store.Remove(record.FolderGuid);
            }
        }

        private void OnStoreChanged()
        {
            RebuildReadModel();
        }

        private void OnProjectChanged()
        {
            RebuildReadModel();
        }

        private void OnUndoRedo()
        {
            RebuildReadModel();
        }

        private void RebuildReadModel()
        {
            readModel.Clear();
            IReadOnlyList<FolderMetadataRecord> records = store.Records;
            for (int index = 0; index < records.Count; index++)
            {
                FolderMetadataRecord record = records[index];
                if (record == null || string.IsNullOrEmpty(record.FolderGuid))
                {
                    continue;
                }

                string path = guidToAssetPath(record.FolderGuid);
                bool isAvailable = !string.IsNullOrEmpty(path) && isValidFolder(path);
                readModel[record.FolderGuid] = new FolderMetadataSnapshot(record, path, isAvailable);
            }

            CacheVersion++;
        }
    }
}
