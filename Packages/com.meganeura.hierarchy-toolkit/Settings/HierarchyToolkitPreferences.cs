using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal enum HierarchyFeature
    {
        QuickStylePalette,
        SceneSelector,
        ComponentMinimap,
        ComponentPopupInspector,
        ActivationToggle,
        HierarchyLines,
        ZebraStriping,
        ManualColors,
        ManualIcons,
        Separators,
        GameObjectBookmarks,
        DefaultParent,
        Count
    }

    internal enum HierarchyShortcut
    {
        ToggleDefaultParent,
        ExpandCollapseHovered,
        IsolateHoveredBranch,
        Count
    }

    /// <summary>Cached, project-scoped personal preferences for Hierarchy Toolkit behavior.</summary>
    internal static class HierarchyToolkitPreferences
    {
        private static readonly string KeyPrefix = "Meganeura.HierarchyToolkit."
            + Hash128.Compute(Application.dataPath.Replace('\\', '/').ToLowerInvariant()) + ".";
        private static readonly bool[] Features = Load("Feature.", (int)HierarchyFeature.Count);
        private static readonly bool[] Shortcuts = Load("Shortcut.", (int)HierarchyShortcut.Count);
        private static bool toolkitEnabled = EditorPrefs.GetBool(KeyPrefix + "Enabled", true);

        internal static event Action Changed;

        internal static bool ToolkitEnabled
        {
            get => toolkitEnabled;
            set
            {
                if (toolkitEnabled == value) return;
                toolkitEnabled = value;
                EditorPrefs.SetBool(KeyPrefix + "Enabled", value);
                Changed?.Invoke();
            }
        }

        internal static bool IsFeatureSelected(HierarchyFeature feature) => Features[(int)feature];
        internal static bool IsFeatureEnabled(HierarchyFeature feature) => toolkitEnabled && Features[(int)feature];
        internal static bool IsShortcutSelected(HierarchyShortcut shortcut) => Shortcuts[(int)shortcut];
        internal static bool IsShortcutEnabled(HierarchyShortcut shortcut) => toolkitEnabled && Shortcuts[(int)shortcut];
        internal static string ToolkitPreferenceKey => KeyPrefix + "Enabled";
        internal static string FeaturePreferenceKey(HierarchyFeature feature) => KeyPrefix + "Feature." + feature;
        internal static string ShortcutPreferenceKey(HierarchyShortcut shortcut) => KeyPrefix + "Shortcut." + shortcut;

        internal static void SetFeature(HierarchyFeature feature, bool value)
        {
            var index = (int)feature;
            if (Features[index] == value) return;
            Features[index] = value;
            EditorPrefs.SetBool(FeaturePreferenceKey(feature), value);
            Changed?.Invoke();
        }

        internal static void SetShortcut(HierarchyShortcut shortcut, bool value)
        {
            var index = (int)shortcut;
            if (Shortcuts[index] == value) return;
            Shortcuts[index] = value;
            EditorPrefs.SetBool(ShortcutPreferenceKey(shortcut), value);
            Changed?.Invoke();
        }

        private static bool[] Load(string category, int count)
        {
            var values = new bool[count];
            for (var i = 0; i < count; ++i)
                values[i] = EditorPrefs.GetBool(KeyPrefix + category + (category == "Feature."
                    ? ((HierarchyFeature)i).ToString()
                    : ((HierarchyShortcut)i).ToString()), true);
            return values;
        }
    }
}
