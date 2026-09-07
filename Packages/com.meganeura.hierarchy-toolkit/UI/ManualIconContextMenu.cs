using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class ManualIconContextMenu
    {
        private const string SetPath = "GameObject/Hierarchy Toolkit/Icon/Set Icon...";
        private const string ClearPath = "GameObject/Hierarchy Toolkit/Icon/Clear Icon";
        private static bool queued;

        [MenuItem(SetPath, false, 51)]
        private static void SetIcon(MenuCommand command) => Queue(command, false);

        [MenuItem(ClearPath, false, 52)]
        private static void ClearIcon(MenuCommand command) => Queue(command, true);

        [MenuItem(SetPath, true)]
        [MenuItem(ClearPath, true)]
        private static bool Validate(MenuCommand command) =>
            ManualColorOperations.ResolveTargets(command.context as GameObject, Selection.gameObjects).Length > 0;

        private static void Queue(MenuCommand command, bool clear)
        {
            // Unity may invoke GameObject menu commands once per selected object.
            // Capture the first invocation and execute the complete operation only once.
            if (queued) return;
            var clicked = command.context as GameObject;
            var targets = ManualColorOperations.ResolveTargets(clicked, Selection.gameObjects);
            if (targets.Length == 0) return;
            var initialIcon = ManualIconOperations.InitialIcon(clicked != null ? clicked : targets[0]);
            queued = true;
            EditorApplication.delayCall += () =>
            {
                queued = false;
                if (clear) ManualIconOperations.Apply(targets, null);
                else ManualIconPicker.Open(targets, initialIcon);
            };
        }
    }
}

