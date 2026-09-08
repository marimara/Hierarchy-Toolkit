using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class SceneCatalog : IDisposable
    {
        internal sealed class Entry
        {
            internal readonly string Guid;
            internal readonly string Path;
            internal readonly string Name;
            internal string DisplayName;

            internal Entry(string guid, string path)
            {
                Guid = guid;
                Path = path;
                Name = System.IO.Path.GetFileNameWithoutExtension(path);
                DisplayName = Name;
            }
        }

        private readonly List<Entry> entries = new();
        internal IReadOnlyList<Entry> Entries => entries;
        internal event Action Changed;

        internal SceneCatalog()
        {
            Refresh();
            EditorApplication.projectChanged += Refresh;
        }

        internal void Refresh()
        {
            var refreshed = new List<Entry>();
            var guids = AssetDatabase.FindAssets("t:Scene");
            for (var i = 0; i < guids.Length; ++i)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path) || AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
                refreshed.Add(new Entry(guids[i], path));
            }
            refreshed.Sort(Compare);
            ApplyDuplicateContext(refreshed);
            entries.Clear();
            entries.AddRange(refreshed);
            Changed?.Invoke();
        }

        private static int Compare(Entry left, Entry right)
        {
            var byName = string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            return byName != 0 ? byName : string.Compare(left.Path, right.Path, StringComparison.OrdinalIgnoreCase);
        }

        private static void ApplyDuplicateContext(List<Entry> scenes)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < scenes.Count; ++i)
            {
                var name = scenes[i].Name;
                counts.TryGetValue(name, out var count);
                counts[name] = count + 1;
            }
            for (var i = 0; i < scenes.Count; ++i)
            {
                var entry = scenes[i];
                if (counts[entry.Name] < 2) continue;
                var folder = Path.GetDirectoryName(entry.Path)?.Replace('\\', '/');
                entry.DisplayName = entry.Name + " — " + (string.IsNullOrEmpty(folder) ? entry.Path : folder);
            }
        }

        public void Dispose() => EditorApplication.projectChanged -= Refresh;
    }
}
