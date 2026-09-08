using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit
{
    // Sparse event-driven snapshot used by retained Hierarchy rows.
    internal sealed class DefaultParentCache : IDisposable
    {
        private readonly HashSet<EntityId> values = new();
        private bool disposed;
        internal event Action Changed;

        internal DefaultParentCache()
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

        internal bool Contains(EntityId id) => values.Contains(id);

        internal void Invalidate()
        {
            values.Clear();
            EditorApplication.delayCall -= Rebuild;
            if (!disposed) EditorApplication.delayCall += Rebuild;
            Changed?.Invoke();
        }

        internal void Rebuild()
        {
            EditorApplication.delayCall -= Rebuild;
            values.Clear();
            if (!disposed && !EditorApplication.isPlayingOrWillChangePlaymode)
                for (var i = 0; i < SceneManager.sceneCount; ++i)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded || string.IsNullOrEmpty(scene.path) || EditorSceneManager.IsPreviewScene(scene)) continue;
                    var store = SceneMetadataStore.Find(scene);
                    if (store != null && store.TryGetDefaultParent(scene, out var target))
                        values.Add(target.GetEntityId());
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
