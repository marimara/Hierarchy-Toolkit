using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class DefaultParentCreationHandler : IDisposable
    {
        internal DefaultParentCreationHandler() => ObjectChangeEvents.changesPublished += ObjectsChanged;

        private static void ObjectsChanged(ref ObjectChangeEventStream stream)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (var i = 0; i < stream.length; ++i)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.CreateGameObjectHierarchy) continue;
                stream.GetCreateGameObjectHierarchyEvent(i, out var data);
                TryParentNewObject(EditorUtility.EntityIdToObject(data.entityId) as GameObject);
            }
        }

        internal static bool TryParentNewObject(GameObject target)
        {
            if (!IsSupportedNewObject(target) || target.transform.parent != null) return false;
            var store = SceneMetadataStore.Find(target.scene);
            if (store == null || !store.TryGetDefaultParent(target.scene, out var parent)
                || parent == target || parent.transform.IsChildOf(target.transform)) return false;

            Undo.SetTransformParent(target.transform, parent.transform, "Parent New GameObject Under Default Parent");
            return true;
        }

        private static bool IsSupportedNewObject(GameObject target)
            => target != null && !EditorApplication.isPlayingOrWillChangePlaymode
               && !EditorUtility.IsPersistent(target) && target.scene.IsValid() && target.scene.isLoaded
               && !string.IsNullOrEmpty(target.scene.path) && !EditorSceneManager.IsPreviewScene(target.scene)
               && PrefabStageUtility.GetPrefabStage(target) == null
               && (target.hideFlags & HideFlags.HideInHierarchy) == 0;

        public void Dispose() => ObjectChangeEvents.changesPublished -= ObjectsChanged;
    }
}
