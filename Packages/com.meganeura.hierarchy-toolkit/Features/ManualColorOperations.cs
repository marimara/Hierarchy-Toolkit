using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit
{
    internal static class ManualColorOperations
    {
        internal static bool IsSupported(GameObject target)
        {
            if (target == null || EditorApplication.isPlayingOrWillChangePlaymode
                || EditorUtility.IsPersistent(target) || !target.scene.IsValid()
                || !target.scene.isLoaded || string.IsNullOrEmpty(target.scene.path)
                || EditorSceneManager.IsPreviewScene(target.scene)
                || PrefabStageUtility.GetPrefabStage(target) != null) return false;
            var guid = AssetDatabase.AssetPathToGUID(target.scene.path);
            if (string.IsNullOrEmpty(guid)
                || AssetDatabase.LoadAssetAtPath<SceneAsset>(target.scene.path) == null) return false;
            var id = GlobalObjectId.GetGlobalObjectIdSlow(target);
            return id.identifierType == 2 && id.targetObjectId != 0 && id.assetGUID.ToString() == guid;
        }

        internal static GameObject[] ResolveTargets(GameObject clicked, GameObject[] selection)
        {
            var candidates = clicked == null ? selection : new[] { clicked };
            if (clicked != null && selection != null)
                foreach (var target in selection)
                    if (target == clicked) { candidates = selection; break; }
            var result = new List<GameObject>();
            if (candidates != null)
                foreach (var target in candidates)
                    if (!result.Contains(target) && IsSupported(target)) result.Add(target);
            return result.ToArray();
        }

        internal static Color InitialColor(GameObject target)
        {
            if (IsSupported(target))
            {
                var store = SceneMetadataStore.Find(target.scene);
                if (store != null && store.TryGetMetadata(target, out var entry) && entry.HasColorOverride)
                    return entry.CustomColor;
            }
            return new Color(0.35f, 0.65f, 1f, 1f);
        }

        internal static void Apply(GameObject[] targets, Color? color)
        {
            ApplyMetadata(targets, color.HasValue, color.HasValue ? "Set Hierarchy Color" : "Clear Hierarchy Color",
                (store, target) => { if (color.HasValue) store.SetColor(target, color.Value); else store.ClearColor(target); });
        }

        internal static GameObject[] ResolveRecursiveTargets(GameObject root)
        {
            if (!IsSupported(root)) return System.Array.Empty<GameObject>();
            var scene = root.scene;
            var result = new List<GameObject>();
            var pending = new Stack<Transform>();
            pending.Push(root.transform);
            while (pending.Count > 0)
            {
                var transform = pending.Pop();
                var target = transform.gameObject;
                if (target.scene != scene) continue;
                if (IsSupported(target)) result.Add(target);
                for (var i = transform.childCount - 1; i >= 0; --i)
                    pending.Push(transform.GetChild(i));
            }
            return result.ToArray();
        }

        internal static void ApplyMetadata(GameObject[] targets, bool create, string label,
            System.Action<SceneMetadataStore, GameObject> mutation)
        {
            var eligible = ResolveTargets(null, targets); // Recheck after the picker has been open.
            if (eligible.Length == 0) return;
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(label);
            try
            {
                var stores = new Dictionary<Scene, SceneMetadataStore>();
                foreach (var target in eligible)
                {
                    if (!stores.TryGetValue(target.scene, out var store))
                    {
                        store = create ? SceneMetadataStore.OpenOrCreate(target.scene)
                            : SceneMetadataStore.Find(target.scene);
                        stores.Add(target.scene, store);
                    }
                    if (store == null) continue;
                    mutation(store, target);
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(group);
                Undo.IncrementCurrentGroup();
                // Store notifications coalesce ManualColorCache's rebuild and repaint on delayCall.
                EditorApplication.RepaintHierarchyWindow();
            }
        }
    }
}



