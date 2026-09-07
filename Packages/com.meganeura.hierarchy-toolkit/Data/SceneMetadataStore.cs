using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit
{
    /// <summary>
    /// Scene-scoped metadata asset. Keep one asset per scene in source control, outside scene objects.
    /// Mutations save immediately; Undo/Redo is also persisted. Returned entries are read-only views
    /// and should be retrieved again after Undo or reload. Scene GUIDs survive moves; Save As and cross-scene migration are not supported.
    /// </summary>
    public sealed class SceneMetadataStore : ScriptableObject
    {
        [SerializeField] private string sceneGuid;
        [SerializeField] private List<GameObjectMetadata> entries = new List<GameObjectMetadata>();
        [NonSerialized] private Dictionary<string, GameObjectMetadata> index;
        [NonSerialized] private Dictionary<GameObject, string> objectIds;

        /// <summary>The owning scene asset GUID.</summary>
        public string SceneGuid => sceneGuid;
        /// <summary>Number of entries, including explicitly created empty entries.</summary>
        public int Count => entries.Count;

        internal const string StoreFolder = "Packages/com.meganeura.hierarchy-toolkit/Editor/SceneMetadata";
        internal static event Action Changed;
        internal IReadOnlyList<GameObjectMetadata> Entries => entries;

        /// <summary>Returns the unique GUID-based path for a loaded saved scene.</summary>
        public static string GetStorePath(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path)
                || EditorSceneManager.IsPreviewScene(scene))
                throw new ArgumentException("Metadata requires a loaded, saved, non-preview scene.", nameof(scene));
            var guid = AssetDatabase.AssetPathToGUID(scene.path);
            if (string.IsNullOrEmpty(guid) || AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) == null)
                throw new ArgumentException("Metadata requires a saved scene asset.", nameof(scene));
            return StoreFolder + "/" + guid + ".asset";
        }

        /// <summary>Finds the canonical store without creating an asset.</summary>
        public static SceneMetadataStore Find(Scene scene)
        {
            var path = GetStorePath(scene);
            var store = AssetDatabase.LoadAssetAtPath<SceneMetadataStore>(path);
            return store != null && store.sceneGuid == AssetDatabase.AssetPathToGUID(scene.path) ? store : null;
        }

        /// <summary>Opens or creates the scene's canonical store. Entry changes support Undo.</summary>
        public static SceneMetadataStore OpenOrCreate(Scene scene)
        {
            var assetPath = GetStorePath(scene);
            var guid = AssetDatabase.AssetPathToGUID(scene.path);
            if (!AssetDatabase.IsValidFolder(StoreFolder))
                AssetDatabase.CreateFolder("Packages/com.meganeura.hierarchy-toolkit/Editor", "SceneMetadata");
            var existing = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (existing != null)
            {
                if (existing is SceneMetadataStore store && store.sceneGuid == guid) return store;
                throw new InvalidOperationException("The metadata path is already owned by another asset or scene.");
            }
            if (System.IO.File.Exists(assetPath))
                throw new InvalidOperationException("The metadata path contains a file Unity cannot load.");
            var created = CreateInstance<SceneMetadataStore>();
            created.sceneGuid = guid;
            try
            {
                AssetDatabase.CreateAsset(created, assetPath);
                if (!AssetDatabase.Contains(created)) throw new InvalidOperationException("Could not create metadata asset at " + assetPath);
                created.Persist();
                return created;
            }
            catch { DestroyImmediate(created); throw; }
        }

        /// <summary>Finds metadata without creating entries. Unsupported or other-scene objects return false.</summary>
        public bool TryGetMetadata(GameObject gameObject, out GameObjectMetadata metadata)
        {
            metadata = null;
            return TryGetId(gameObject, out var id) && Index.TryGetValue(id, out metadata);
        }

        /// <summary>Creates an undoable empty entry if needed. Throws for unsupported objects.</summary>
        public GameObjectMetadata GetOrCreateMetadata(GameObject gameObject)
        {
            var id = RequireId(gameObject);
            if (Index.TryGetValue(id, out var metadata)) return metadata;
            Undo.RegisterCompleteObjectUndo(this, "Create Hierarchy Metadata");
            metadata = Add(id);
            Persist();
            return metadata;
        }

        /// <summary>Assigns an explicit color with Undo and persistence.</summary>
        public void SetColor(GameObject gameObject, Color color)
        {
            var id = RequireId(gameObject);
            Index.TryGetValue(id, out var metadata);
            if (metadata != null && metadata.HasColorOverride && metadata.CustomColor.Equals(color)) return;
            Undo.RegisterCompleteObjectUndo(this, "Set Hierarchy Color");
            (metadata ?? Add(id)).SetColor(color);
            Persist();
        }

        /// <summary>Clears the color and removes the entry if it becomes empty.</summary>
        public void ClearColor(GameObject gameObject)
        {
            if (!TryGetMetadata(gameObject, out var metadata)) return;
            if (!metadata.HasColorOverride && !metadata.IsEmpty) return;
            Undo.RegisterCompleteObjectUndo(this, "Clear Hierarchy Color");
            metadata.ClearColor();
            RemoveIfEmpty(metadata);
            Persist();
        }

        /// <summary>Stores a stable icon reference with Undo. Legacy paths/names are normalized at assignment.</summary>
        public void SetIcon(GameObject gameObject, string iconPath)
        {
            iconPath = HierarchyIconReference.Normalize(iconPath);
            var id = RequireId(gameObject);
            Index.TryGetValue(id, out var metadata);
            if (metadata != null && metadata.HasIconOverride && metadata.CustomIconReference == iconPath) return;
            Undo.RegisterCompleteObjectUndo(this, "Set Hierarchy Icon");
            (metadata ?? Add(id)).SetIcon(iconPath);
            Persist();
        }

        /// <summary>Clears the icon and removes the entry if it becomes empty.</summary>
        public void ClearIcon(GameObject gameObject)
        {
            if (!TryGetMetadata(gameObject, out var metadata)) return;
            if (!metadata.HasIconOverride && !metadata.IsEmpty) return;
            Undo.RegisterCompleteObjectUndo(this, "Clear Hierarchy Icon");
            metadata.ClearIcon();
            RemoveIfEmpty(metadata);
            Persist();
        }

        /// <summary>Assigns header options with Undo and persistence. A disabled style clears only header data.</summary>
        public void SetSeparator(GameObject gameObject, SeparatorStyle style)
        {
            if (!style.Enabled) { ClearSeparator(gameObject); return; }
            var id = RequireId(gameObject);
            Index.TryGetValue(id, out var metadata);
            if (metadata != null && metadata.Separator.Equals(style)) return;
            Undo.RegisterCompleteObjectUndo(this, "Set Hierarchy Separator");
            (metadata ?? Add(id)).SetSeparator(style);
            Persist();
        }

        /// <summary>Clears only header data with Undo, preserving manual colors and icons.</summary>
        public void ClearSeparator(GameObject gameObject)
        {
            if (!TryGetMetadata(gameObject, out var metadata) || !metadata.Separator.Enabled) return;
            Undo.RegisterCompleteObjectUndo(this, "Clear Hierarchy Separator");
            metadata.SetSeparator(default);
            RemoveIfEmpty(metadata);
            Persist();
        }

        /// <summary>Removes all metadata for an object with Undo. Returns whether an entry existed.</summary>
        public bool RemoveMetadata(GameObject gameObject)
        {
            if (!TryGetMetadata(gameObject, out var metadata)) return false;
            Undo.RegisterCompleteObjectUndo(this, "Remove Hierarchy Metadata");
            entries.Remove(metadata);
            Index.Remove(metadata.ObjectId);
            Persist();
            return true;
        }

        /// <summary>Removes empty entries with Undo; never deletes unresolved objects' non-empty entries.</summary>
        public int CleanupEmptyMetadata()
        {
            var count = 0;
            for (var i = entries.Count - 1; i >= 0; --i)
            {
                if (!entries[i].IsEmpty) continue;
                if (count == 0) Undo.RegisterCompleteObjectUndo(this, "Clean Empty Hierarchy Metadata");
                entries.RemoveAt(i);
                ++count;
            }
            if (count > 0) { index = null; Persist(); }
            return count;
        }

        private Dictionary<string, GameObjectMetadata> Index
        {
            get
            {
                if (index != null) return index;
                index = new Dictionary<string, GameObjectMetadata>(StringComparer.Ordinal);
                foreach (var entry in entries) index.Add(entry.ObjectId, entry);
                return index;
            }
        }

        private GameObjectMetadata Add(string id)
        {
            var metadata = new GameObjectMetadata(id);
            entries.Add(metadata);
            Index.Add(id, metadata);
            return metadata;
        }

        private void RemoveIfEmpty(GameObjectMetadata metadata)
        {
            if (!metadata.IsEmpty) return;
            entries.Remove(metadata);
            Index.Remove(metadata.ObjectId);
        }

        private string RequireId(GameObject gameObject)
        {
            if (!AssetDatabase.Contains(this)) throw new InvalidOperationException("Use a persistent SceneMetadataStore asset.");
            if (!TryGetId(gameObject, out var id))
                throw new ArgumentException("Object must belong to this store's saved scene, outside Play Mode and Prefab Mode.", nameof(gameObject));
            return id;
        }

        private bool TryGetId(GameObject gameObject, out string id)
        {
            id = null;
            if (gameObject == null || EditorApplication.isPlayingOrWillChangePlaymode || EditorUtility.IsPersistent(gameObject)
                || !gameObject.scene.IsValid() || !gameObject.scene.isLoaded || string.IsNullOrEmpty(gameObject.scene.path)
                || EditorSceneManager.IsPreviewScene(gameObject.scene) || PrefabStageUtility.GetPrefabStage(gameObject) != null)
                return false;
            // Check scene ownership before the cache: objects may have moved between scenes.
            if (AssetDatabase.AssetPathToGUID(gameObject.scene.path) != sceneGuid) return false;
            objectIds ??= new Dictionary<GameObject, string>();
            if (objectIds.TryGetValue(gameObject, out id)) return true;
            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);
            if (globalId.identifierType != 2 || globalId.targetObjectId == 0 || globalId.assetGUID.ToString() != sceneGuid) return false;
            id = globalId.ToString();
            objectIds.Add(gameObject, id);
            return true;
        }

        private void Persist()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
            Changed?.Invoke();
        }

        private void Invalidate() { index = null; objectIds = null; }
        private void OnValidate() { Invalidate(); Changed?.Invoke(); }
        private void SceneChanged(Scene scene) { objectIds = null; }
        private void OnUndoRedo()
        {
            Invalidate();
            if (EditorUtility.IsDirty(this) && AssetDatabase.Contains(this)) Persist();
        }
        internal void MigrateIconReferences()
        {
            EditorApplication.delayCall -= MigrateIconReferences;
            if (this == null || !AssetDatabase.Contains(this)) return;
            var changed = false;
            foreach (var entry in entries) changed |= entry.MigrateIconReference();
            if (changed) { Invalidate(); Persist(); }
        }

        private void OnEnable()
        {
            Invalidate();
            EditorApplication.delayCall += MigrateIconReferences;
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.hierarchyChanged += Invalidate;
            EditorApplication.projectChanged += Invalidate;
            EditorSceneManager.sceneSaved += SceneChanged;
            EditorSceneManager.sceneClosed += SceneChanged;
        }
        private void OnDisable()
        {
            EditorApplication.delayCall -= MigrateIconReferences;
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.hierarchyChanged -= Invalidate;
            EditorApplication.projectChanged -= Invalidate;
            EditorSceneManager.sceneSaved -= SceneChanged;
            EditorSceneManager.sceneClosed -= SceneChanged;
        }
    }
}





