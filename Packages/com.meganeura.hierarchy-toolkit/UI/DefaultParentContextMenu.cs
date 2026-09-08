using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class DefaultParentContextMenu
    {
        private const string SetPath = "GameObject/Hierarchy Toolkit/Default Parent/Set as Default Parent";
        private const string ClearPath = "GameObject/Hierarchy Toolkit/Default Parent/Clear Default Parent";
        private static bool queued;

        [MenuItem(SetPath, false, 71)]
        private static void Set(MenuCommand command) => Queue(command, false);

        [MenuItem(ClearPath, false, 72)]
        private static void Clear(MenuCommand command) => Queue(command, true);

        [MenuItem(SetPath, true)]
        private static bool ValidateSet(MenuCommand command)
        {
            var target = command.context as GameObject;
            return HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent)
                && ManualColorOperations.IsSupported(target) && !DefaultParentOperations.IsActive(target);
        }

        [MenuItem(ClearPath, true)]
        private static bool ValidateClear(MenuCommand command)
            => HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent)
                && DefaultParentOperations.IsActive(command.context as GameObject);

        private static void Queue(MenuCommand command, bool clear)
        {
            if (queued || !HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent)
                || command.context is not GameObject target || !ManualColorOperations.IsSupported(target)) return;
            queued = true;
            EditorApplication.delayCall += () =>
            {
                queued = false;
                if (!HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent)) return;
                if (ManualColorOperations.IsSupported(target) && DefaultParentOperations.IsActive(target) == clear)
                    DefaultParentOperations.Toggle(target);
            };
        }
    }
}
