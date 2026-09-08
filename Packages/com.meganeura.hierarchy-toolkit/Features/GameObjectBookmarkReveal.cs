using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class GameObjectBookmarkReveal
    {
        internal static bool Reveal(GameObjectBookmarkStore.Entry entry, HierarchyWindow window)
        {
            var target = GameObjectBookmarkStore.Resolve(entry);
            if (target == null || window == null || window.View?.Source == null
                || PrefabStageUtility.GetCurrentPrefabStage() != null) return false;
            var view = window.View;
            var source = view.Source;
            if (!source.IsCreated) return false;
            window.SetSearchText(string.Empty);
            var handler = source.GetOrCreateNodeTypeHandler<HierarchyGameObjectHandler>();
            var node = handler.GetOrCreateNode(target);
            HierarchyNavigation.ExpandAncestors(view, node);
            view.Update();
            Selection.activeGameObject = target;
            view.Frame(node);
            window.Focus();
            return true;
        }
    }
}
