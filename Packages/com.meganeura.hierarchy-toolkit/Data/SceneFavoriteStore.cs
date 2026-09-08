using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    // Undo operates on this transient object; only EditorPrefs contains personal data.
    internal sealed class SceneFavoriteStore : ScriptableObject
    {
        [Serializable]
        private sealed class Payload { public List<string> guids = new(); }

        [SerializeField] private List<string> guids = new();
        private readonly HashSet<string> lookup = new(StringComparer.Ordinal);
        private string preferencesKey;
        internal event Action Changed;
        internal static string ProjectKey => "Meganeura.HierarchyToolkit.SceneFavorites." + Hash128.Compute(Application.dataPath);
        internal IReadOnlyList<string> Guids => guids;

        internal static SceneFavoriteStore Open(string key)
        {
            var store = CreateInstance<SceneFavoriteStore>();
            store.hideFlags = HideFlags.HideAndDontSave;
            store.preferencesKey = key;
            try
            {
                var payload = JsonUtility.FromJson<Payload>(EditorPrefs.GetString(key, "{}"));
                if (payload?.guids != null)
                    for (var i = 0; i < payload.guids.Count; ++i)
                    {
                        var guid = payload.guids[i];
                        if (!string.IsNullOrEmpty(guid) && store.lookup.Add(guid)) store.guids.Add(guid);
                    }
            }
            catch (ArgumentException) { /* Invalid personal preferences behave as an empty list. */ }
            Undo.undoRedoPerformed += store.RestoreAndSave;
            return store;
        }

        internal bool Contains(string guid) => !string.IsNullOrEmpty(guid) && lookup.Contains(guid);

        internal void SetFavorite(string guid, bool favorite)
        {
            if (string.IsNullOrEmpty(guid) || favorite == lookup.Contains(guid)) return;
            Undo.IncrementCurrentGroup();
            Undo.RegisterCompleteObjectUndo(this, favorite ? "Favorite Scene" : "Unfavorite Scene");
            if (favorite)
            {
                lookup.Add(guid);
                guids.Add(guid);
            }
            else
            {
                lookup.Remove(guid);
                guids.Remove(guid);
            }
            Save();
            Undo.IncrementCurrentGroup();
        }

        internal void Toggle(string guid) => SetFavorite(guid, !Contains(guid));

        private void RestoreAndSave()
        {
            lookup.Clear();
            for (var i = 0; i < guids.Count;)
            {
                if (string.IsNullOrEmpty(guids[i]) || !lookup.Add(guids[i])) guids.RemoveAt(i);
                else ++i;
            }
            Save();
        }

        private void Save()
        {
            EditorPrefs.SetString(preferencesKey, JsonUtility.ToJson(new Payload { guids = guids }));
            Changed?.Invoke();
        }

        private void OnDisable() => Undo.undoRedoPerformed -= RestoreAndSave;
    }
}
