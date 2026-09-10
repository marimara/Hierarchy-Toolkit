using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal sealed class ProjectFolderMetadataStore : ScriptableObject
    {
        [SerializeField] private List<FolderMetadataRecord> records = new List<FolderMetadataRecord>();

        internal event Action Changed;

        internal IReadOnlyList<FolderMetadataRecord> Records => records;

        internal FolderMetadataRecord GetOrCreate(string folderGuid)
        {
            FolderMetadataRecord record = Find(folderGuid);
            if (record != null)
            {
                return record;
            }

            record = new FolderMetadataRecord(folderGuid);
            records.Add(record);
            return record;
        }

        internal FolderMetadataRecord Find(string folderGuid)
        {
            for (int index = 0; index < records.Count; index++)
            {
                FolderMetadataRecord record = records[index];
                if (record != null && string.Equals(record.FolderGuid, folderGuid, StringComparison.Ordinal))
                {
                    return record;
                }
            }

            return null;
        }

        internal bool Remove(string folderGuid)
        {
            for (int index = records.Count - 1; index >= 0; index--)
            {
                FolderMetadataRecord record = records[index];
                if (record != null && string.Equals(record.FolderGuid, folderGuid, StringComparison.Ordinal))
                {
                    records.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        internal int RemoveAll(ISet<string> folderGuids)
        {
            int removed = 0;
            for (int index = records.Count - 1; index >= 0; index--)
            {
                FolderMetadataRecord record = records[index];
                if (record == null || folderGuids.Contains(record.FolderGuid))
                {
                    records.RemoveAt(index);
                    removed++;
                }
            }

            return removed;
        }

        internal void NotifyChanged()
        {
            Changed?.Invoke();
        }

        private void OnValidate()
        {
            Changed?.Invoke();
        }
    }
}
