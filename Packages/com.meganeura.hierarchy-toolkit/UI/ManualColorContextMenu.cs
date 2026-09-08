using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class ManualColorContextMenu
    {
        private const string SetPath = "GameObject/Hierarchy Toolkit/Color/Set Color...";
        private const string ClearPath = "GameObject/Hierarchy Toolkit/Color/Clear Color";
        private static bool queued;

        [MenuItem(SetPath, false, 49)]
        private static void SetColor(MenuCommand command) => Queue(command, false);

        [MenuItem(ClearPath, false, 50)]
        private static void ClearColor(MenuCommand command) => Queue(command, true);

        [MenuItem(SetPath, true)]
        [MenuItem(ClearPath, true)]
        private static bool Validate(MenuCommand command) =>
            HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ManualColors)
            && ManualColorOperations.ResolveTargets(command.context as GameObject, Selection.gameObjects).Length > 0;

        private static void Queue(MenuCommand command, bool clear)
        {
            // Unity may invoke GameObject menu commands once per selected object.
            // Capture the first invocation and execute the complete operation only once.
            if (queued || !HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ManualColors)) return;
            var clicked = command.context as GameObject;
            var targets = ManualColorOperations.ResolveTargets(clicked, Selection.gameObjects);
            if (targets.Length == 0) return;
            var initialColor = ManualColorOperations.InitialColor(clicked != null ? clicked : targets[0]);
            queued = true;
            EditorApplication.delayCall += () =>
            {
                queued = false;
                if (!HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ManualColors)) return;
                if (clear) ManualColorOperations.Apply(targets, null);
                else ManualColorPicker.Open(targets, initialColor);
            };
        }
    }
}
