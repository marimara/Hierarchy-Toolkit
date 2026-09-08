using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit
{
    // Undo operates on this transient object; only EditorPrefs contains persistent data.
    internal sealed class GameObjectBookmarkStore : ScriptableObject
    {
        [Serializable]
        internal sealed class Entry
        {
            public string objectId;
            public string lastKnownName;
        }

        [Serializable]
        private sealed class Payload { public List<Entry> entries = new(); }

        [SerializeField] private List<Entry> entries = new();
        private string preferencesKey;
        internal IReadOnlyList<Entry> Entries => entries;
        internal event Action Changed;
        internal static string ProjectKey => "Meganeura.HierarchyToolkit.Bookmarks." + Hash128.Compute(Application.dataPath);

        internal static GameObjectBookmarkStore Open(string key)
        {
            var store = CreateInstance<GameObjectBookmarkStore>();
            store.hideFlags = HideFlags.HideAndDontSave;
            store.preferencesKey = key;
            try
            {
                var payload = JsonUtility.FromJson<Payload>(EditorPrefs.GetString(key, "{}"));
                var ids = new HashSet<string>();
                if (payload?.entries != null)
                    foreach (var entry in payload.entries)
                        if (entry != null && GlobalObjectId.TryParse(entry.objectId, out var id)
                            && id.identifierType == 2 && id.targetObjectId != 0 && ids.Add(entry.objectId))
                            store.entries.Add(entry);
            }
            catch (ArgumentException) { /* Invalid personal preferences behave as an empty list. */ }
            Undo.undoRedoPerformed += store.Save;
            return store;
        }

        internal bool Contains(GameObject target)
        {
            if (!ManualColorOperations.IsSupported(target)) return false;
            return IndexOf(GlobalObjectId.GetGlobalObjectIdSlow(target).ToString()) >= 0;
        }

        internal void Add(GameObject clicked, GameObject[] selection)
        {
            var targets = ManualColorOperations.ResolveTargets(clicked, selection);
            var additions = new List<Entry>();
            foreach (var target in targets)
            {
                var id = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
                if (IndexOf(id) < 0) additions.Add(new Entry { objectId = id, lastKnownName = target.name });
            }
            if (additions.Count == 0) return;
            RecordUndo("Add GameObject Bookmarks");
            entries.AddRange(additions);
            Commit();
        }

        internal void Remove(GameObject clicked, GameObject[] selection)
        {
            var targets = ManualColorOperations.ResolveTargets(clicked, selection);
            var ids = new HashSet<string>();
            foreach (var target in targets) ids.Add(GlobalObjectId.GetGlobalObjectIdSlow(target).ToString());
            if (!entries.Exists(entry => ids.Contains(entry.objectId))) return;
            RecordUndo("Remove GameObject Bookmarks");
            entries.RemoveAll(entry => ids.Contains(entry.objectId));
            Commit();
        }

        internal void Remove(string objectId)
        {
            var index = IndexOf(objectId);
            if (index < 0) return;
            RecordUndo("Remove GameObject Bookmark");
            entries.RemoveAt(index);
            Commit();
        }

        internal static GameObject Resolve(Entry entry)
        {
            if (entry == null || EditorApplication.isPlayingOrWillChangePlaymode
                || !GlobalObjectId.TryParse(entry.objectId, out var id)) return null;
            // Never ask Unity to resolve an ID belonging to an unloaded scene.
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || string.IsNullOrEmpty(scene.path)
                    || AssetDatabase.AssetPathToGUID(scene.path) != id.assetGUID.ToString()) continue;
                var target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                return target != null && target.scene == scene && ManualColorOperations.IsSupported(target) ? target : null;
            }
            return null;
        }

        internal static string DisplayName(Entry entry)
        {
            var target = Resolve(entry);
            return target != null ? target.name : (string.IsNullOrEmpty(entry.lastKnownName) ? "Missing GameObject" : entry.lastKnownName);
        }

        private void RecordUndo(string label)
        {
            Undo.IncrementCurrentGroup();
            Undo.RegisterCompleteObjectUndo(this, label);
        }

        private void Commit()
        {
            Save();
            Undo.IncrementCurrentGroup();
        }

        private int IndexOf(string id) => entries.FindIndex(entry => entry.objectId == id);
        private void Save()
        {
            EditorPrefs.SetString(preferencesKey, JsonUtility.ToJson(new Payload { entries = entries }));
            Changed?.Invoke();
        }

        private void OnDisable() => Undo.undoRedoPerformed -= Save;
    }
}


