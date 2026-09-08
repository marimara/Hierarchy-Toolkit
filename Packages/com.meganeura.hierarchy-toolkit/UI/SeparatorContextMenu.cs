using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class SeparatorContextMenu
    {
        private const string MarkPath = "GameObject/Hierarchy Toolkit/Separator/Mark as Separator";
        private const string EditPath = "GameObject/Hierarchy Toolkit/Separator/Edit Separator...";
        private const string ClearPath = "GameObject/Hierarchy Toolkit/Separator/Clear Separator";
        private static bool queued;

        [MenuItem(MarkPath, false, 61)]
        private static void Mark(MenuCommand command) => Queue(command, 0);
        [MenuItem(EditPath, false, 62)]
        private static void Edit(MenuCommand command) => Queue(command, 1);
        [MenuItem(ClearPath, false, 63)]
        private static void Clear(MenuCommand command) => Queue(command, 2);

        [MenuItem(MarkPath, true), MenuItem(EditPath, true), MenuItem(ClearPath, true)]
        private static bool Validate(MenuCommand command) =>
            HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.Separators)
            && ManualColorOperations.ResolveTargets(command.context as GameObject, Selection.gameObjects).Length > 0;

        private static void Queue(MenuCommand command, int action)
        {
            if (queued || !HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.Separators)) return;
            var clicked = command.context as GameObject;
            var targets = ManualColorOperations.ResolveTargets(clicked, Selection.gameObjects);
            if (targets.Length == 0) return;
            var editTarget = clicked != null ? clicked : Selection.activeGameObject;
            if (!ManualColorOperations.IsSupported(editTarget)) editTarget = targets[0];
            queued = true;
            EditorApplication.delayCall += () =>
            {
                queued = false;
                if (!HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.Separators)) return;
                if (action == 0) SeparatorOperations.Mark(targets);
                else if (action == 2) SeparatorOperations.Clear(targets);
                else if (ManualColorOperations.IsSupported(editTarget)) SeparatorEditor.Open(editTarget);
            };
        }
    }
}
