using UnityEditor;

namespace Meganeura.HierarchyToolkit
{
    internal static class HierarchyToolkitMenu
    {
        private const string Root = "Tools/Hierarchy Toolkit/";
        private const string FeaturesHeader = Root + "Features";
        private const string QuickStylePalette = Root + "Quick Style Palette";
        private const string SceneSelector = Root + "Scene Selector";
        private const string ComponentMinimap = Root + "Component Minimap";
        private const string ComponentPopupInspector = Root + "Component Popup Inspector";
        private const string ActivationToggle = Root + "Activation Toggle";
        private const string HierarchyLines = Root + "Hierarchy Lines";
        private const string ZebraStriping = Root + "Zebra Striping";
        private const string ManualColors = Root + "Manual Colors";
        private const string ManualIcons = Root + "Manual Icons";
        private const string Separators = Root + "Separators";
        private const string GameObjectBookmarks = Root + "GameObject Bookmarks";
        private const string DefaultParent = Root + "Default Parent";
        private const string ShortcutsHeader = Root + "Shortcuts";
        private const string DefaultParentShortcut = Root + "D - Toggle Default Parent";
        private const string ExpandShortcut = Root + "E - Expand or Collapse Hovered";
        private const string IsolateShortcut = Root + "Shift+E - Isolate Hovered Branch";
        private const string DisableToolkit = Root + "Disable Hierarchy Toolkit";

        [MenuItem(FeaturesHeader, false, 0)] private static void FeaturesLabel() { }
        [MenuItem(FeaturesHeader, true)] private static bool DisableFeaturesLabel() => false;
        [MenuItem(QuickStylePalette, false, 1)] private static void ToggleQuickStylePalette() => Toggle(HierarchyFeature.QuickStylePalette);
        [MenuItem(QuickStylePalette, true)] private static bool CheckQuickStylePalette() => Check(QuickStylePalette, HierarchyFeature.QuickStylePalette);
        [MenuItem(SceneSelector, false, 2)] private static void ToggleSceneSelector() => Toggle(HierarchyFeature.SceneSelector);
        [MenuItem(SceneSelector, true)] private static bool CheckSceneSelector() => Check(SceneSelector, HierarchyFeature.SceneSelector);
        [MenuItem(ComponentMinimap, false, 3)] private static void ToggleComponentMinimap() => Toggle(HierarchyFeature.ComponentMinimap);
        [MenuItem(ComponentMinimap, true)] private static bool CheckComponentMinimap() => Check(ComponentMinimap, HierarchyFeature.ComponentMinimap);
        [MenuItem(ComponentPopupInspector, false, 4)] private static void ToggleComponentPopupInspector() => Toggle(HierarchyFeature.ComponentPopupInspector);
        [MenuItem(ComponentPopupInspector, true)] private static bool CheckComponentPopupInspector() => Check(ComponentPopupInspector, HierarchyFeature.ComponentPopupInspector);
        [MenuItem(ActivationToggle, false, 5)] private static void ToggleActivationToggle() => Toggle(HierarchyFeature.ActivationToggle);
        [MenuItem(ActivationToggle, true)] private static bool CheckActivationToggle() => Check(ActivationToggle, HierarchyFeature.ActivationToggle);
        [MenuItem(HierarchyLines, false, 6)] private static void ToggleHierarchyLines() => Toggle(HierarchyFeature.HierarchyLines);
        [MenuItem(HierarchyLines, true)] private static bool CheckHierarchyLines() => Check(HierarchyLines, HierarchyFeature.HierarchyLines);
        [MenuItem(ZebraStriping, false, 7)] private static void ToggleZebraStriping() => Toggle(HierarchyFeature.ZebraStriping);
        [MenuItem(ZebraStriping, true)] private static bool CheckZebraStriping() => Check(ZebraStriping, HierarchyFeature.ZebraStriping);
        [MenuItem(ManualColors, false, 8)] private static void ToggleManualColors() => Toggle(HierarchyFeature.ManualColors);
        [MenuItem(ManualColors, true)] private static bool CheckManualColors() => Check(ManualColors, HierarchyFeature.ManualColors);
        [MenuItem(ManualIcons, false, 9)] private static void ToggleManualIcons() => Toggle(HierarchyFeature.ManualIcons);
        [MenuItem(ManualIcons, true)] private static bool CheckManualIcons() => Check(ManualIcons, HierarchyFeature.ManualIcons);
        [MenuItem(Separators, false, 10)] private static void ToggleSeparators() => Toggle(HierarchyFeature.Separators);
        [MenuItem(Separators, true)] private static bool CheckSeparators() => Check(Separators, HierarchyFeature.Separators);
        [MenuItem(GameObjectBookmarks, false, 11)] private static void ToggleGameObjectBookmarks() => Toggle(HierarchyFeature.GameObjectBookmarks);
        [MenuItem(GameObjectBookmarks, true)] private static bool CheckGameObjectBookmarks() => Check(GameObjectBookmarks, HierarchyFeature.GameObjectBookmarks);
        [MenuItem(DefaultParent, false, 12)] private static void ToggleDefaultParent() => Toggle(HierarchyFeature.DefaultParent);
        [MenuItem(DefaultParent, true)] private static bool CheckDefaultParent() => Check(DefaultParent, HierarchyFeature.DefaultParent);

        [MenuItem(ShortcutsHeader, false, 30)] private static void ShortcutsLabel() { }
        [MenuItem(ShortcutsHeader, true)] private static bool DisableShortcutsLabel() => false;
        [MenuItem(DefaultParentShortcut, false, 31)] private static void ToggleDefaultParentShortcut() => Toggle(HierarchyShortcut.ToggleDefaultParent);
        [MenuItem(DefaultParentShortcut, true)] private static bool CheckDefaultParentShortcut() => Check(DefaultParentShortcut, HierarchyShortcut.ToggleDefaultParent);
        [MenuItem(ExpandShortcut, false, 32)] private static void ToggleExpandShortcut() => Toggle(HierarchyShortcut.ExpandCollapseHovered);
        [MenuItem(ExpandShortcut, true)] private static bool CheckExpandShortcut() => Check(ExpandShortcut, HierarchyShortcut.ExpandCollapseHovered);
        [MenuItem(IsolateShortcut, false, 33)] private static void ToggleIsolateShortcut() => Toggle(HierarchyShortcut.IsolateHoveredBranch);
        [MenuItem(IsolateShortcut, true)] private static bool CheckIsolateShortcut() => Check(IsolateShortcut, HierarchyShortcut.IsolateHoveredBranch);

        [MenuItem(DisableToolkit, false, 50)] private static void ToggleToolkit() => HierarchyToolkitPreferences.ToolkitEnabled = !HierarchyToolkitPreferences.ToolkitEnabled;
        [MenuItem(DisableToolkit, true)] private static bool CheckToolkit()
        {
            Menu.SetChecked(DisableToolkit, !HierarchyToolkitPreferences.ToolkitEnabled);
            return true;
        }

        private static void Toggle(HierarchyFeature feature) => HierarchyToolkitPreferences.SetFeature(feature, !HierarchyToolkitPreferences.IsFeatureSelected(feature));
        private static void Toggle(HierarchyShortcut shortcut) => HierarchyToolkitPreferences.SetShortcut(shortcut, !HierarchyToolkitPreferences.IsShortcutSelected(shortcut));
        private static bool Check(string path, HierarchyFeature feature) { Menu.SetChecked(path, HierarchyToolkitPreferences.IsFeatureSelected(feature)); return true; }
        private static bool Check(string path, HierarchyShortcut shortcut) { Menu.SetChecked(path, HierarchyToolkitPreferences.IsShortcutSelected(shortcut)); return true; }
    }
}
