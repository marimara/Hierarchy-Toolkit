using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit
{
    // Sparse snapshot: misses are free and never resolve objects or assets in a row callback.
    internal abstract class ManualMetadataCache<T> : IDisposable
    {
        private readonly Dictionary<EntityId, T> values = new Dictionary<EntityId, T>();
        private bool disposed;
        internal event Action Changed;

        protected ManualMetadataCache()
        {
            EditorApplication.hierarchyChanged += Invalidate;
            EditorApplication.projectChanged += Invalidate;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorSceneManager.sceneClosed += SceneChanged;
            EditorSceneManager.sceneSaved += SceneChanged;
            Undo.undoRedoPerformed += Invalidate;
            SceneMetadataStore.Changed += Invalidate;
            Invalidate();
        }

        protected bool TryGetValue(EntityId id, out T value) => values.TryGetValue(id, out value);
        protected abstract bool TryResolve(GameObjectMetadata entry, out T value);
        protected virtual void ClearResolvedAssets() { }

        internal void Invalidate()
        {
            values.Clear();
            ClearResolvedAssets();
            EditorApplication.delayCall -= Rebuild;
            if (!disposed) EditorApplication.delayCall += Rebuild;
            Changed?.Invoke();
        }

        internal void Rebuild()
        {
            EditorApplication.delayCall -= Rebuild;
            values.Clear();
            ClearResolvedAssets();
            if (disposed || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Changed?.Invoke();
                return;
            }
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || string.IsNullOrEmpty(scene.path) || EditorSceneManager.IsPreviewScene(scene)) continue;
                var store = SceneMetadataStore.Find(scene);
                if (store == null) continue;
                var entries = store.Entries;
                for (var j = 0; j < entries.Count; ++j)
                {
                    var entry = entries[j];
                    if (!GlobalObjectId.TryParse(entry.ObjectId, out var id)) continue;
                    var target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (target == null || target.scene != scene || PrefabStageUtility.GetPrefabStage(target) != null) continue;
                    if (TryResolve(entry, out var value)) values[target.GetEntityId()] = value;
                }
            }
            Changed?.Invoke();
            EditorApplication.RepaintHierarchyWindow();
        }

        private void SceneOpened(Scene scene, OpenSceneMode mode) => Invalidate();
        private void SceneChanged(Scene scene) => Invalidate();
        private void PlayModeChanged(PlayModeStateChange state) => Invalidate();

        public void Dispose()
        {
            disposed = true;
            EditorApplication.hierarchyChanged -= Invalidate;
            EditorApplication.projectChanged -= Invalidate;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.sceneClosed -= SceneChanged;
            EditorSceneManager.sceneSaved -= SceneChanged;
            Undo.undoRedoPerformed -= Invalidate;
            SceneMetadataStore.Changed -= Invalidate;
            Invalidate();
        }
    }
}

