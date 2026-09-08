using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class GameObjectBookmarkContextMenu
    {
        private const string AddPath = "GameObject/Hierarchy Toolkit/Bookmarks/Add Bookmark";
        private const string RemovePath = "GameObject/Hierarchy Toolkit/Bookmarks/Remove Bookmark";
        private static bool queued;

        [MenuItem(AddPath, false, 81)]
        private static void Add(MenuCommand command) => Queue(command, false);
        [MenuItem(RemovePath, false, 82)]
        private static void Remove(MenuCommand command) => Queue(command, true);
        [MenuItem(AddPath, true)]
        [MenuItem(RemovePath, true)]
        private static bool Validate(MenuCommand command) =>
            HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.GameObjectBookmarks)
            && ManualColorOperations.ResolveTargets(command.context as GameObject, Selection.gameObjects).Length > 0;

        private static void Queue(MenuCommand command, bool remove)
        {
            if (queued || !HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.GameObjectBookmarks)) return;
            var targets = ManualColorOperations.ResolveTargets(command.context as GameObject, Selection.gameObjects);
            if (targets.Length == 0) return;
            queued = true;
            EditorApplication.delayCall += () =>
            {
                queued = false;
                if (!HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.GameObjectBookmarks)) return;
                if (remove) HierarchyToolkitBootstrap.Bookmarks.Remove(null, targets);
                else HierarchyToolkitBootstrap.Bookmarks.Add(null, targets);
            };
        }
    }
}
