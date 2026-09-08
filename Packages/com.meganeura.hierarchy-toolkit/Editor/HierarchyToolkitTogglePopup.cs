using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class HierarchyToolkitTogglePopup : PopupWindowContent
    {
        private static readonly Vector2 Size = new(286f, 398f);

        internal static void ShowAtPointer()
        {
            EditorApplication.delayCall += () =>
            {
                var mainWindow = EditorGUIUtility.GetMainWindowPosition();
                var anchor = new Rect(mainWindow.x + 92f, mainWindow.y + 24f, 1f, 1f);
                UnityEditor.PopupWindow.Show(anchor, new HierarchyToolkitTogglePopup());
            };
        }

        public override Vector2 GetWindowSize() => Size;

        public override void OnOpen() => HierarchyToolkitPreferences.Changed += Repaint;
        public override void OnClose() => HierarchyToolkitPreferences.Changed -= Repaint;

        public override void OnGUI(Rect rect)
        {
            GUILayout.Space(5f);
            DrawHeader("Features");
            DrawFeature("Quick Style Palette", HierarchyFeature.QuickStylePalette);
            DrawFeature("Scene Selector", HierarchyFeature.SceneSelector);
            DrawFeature("Component Minimap", HierarchyFeature.ComponentMinimap);
            DrawFeature("Component Popup Inspector", HierarchyFeature.ComponentPopupInspector);
            DrawFeature("Activation Toggle", HierarchyFeature.ActivationToggle);
            DrawFeature("Hierarchy Lines", HierarchyFeature.HierarchyLines);
            DrawFeature("Zebra Striping", HierarchyFeature.ZebraStriping);
            DrawFeature("Manual Colors", HierarchyFeature.ManualColors);
            DrawFeature("Manual Icons", HierarchyFeature.ManualIcons);
            DrawFeature("Separators", HierarchyFeature.Separators);
            DrawFeature("GameObject Bookmarks", HierarchyFeature.GameObjectBookmarks);
            DrawFeature("Default Parent", HierarchyFeature.DefaultParent);

            GUILayout.Space(3f);
            DrawHeader("Shortcuts");
            DrawShortcut("D - Toggle Default Parent", HierarchyShortcut.ToggleDefaultParent);
            DrawShortcut("E - Expand or Collapse Hovered", HierarchyShortcut.ExpandCollapseHovered);
            DrawShortcut("Shift+E - Isolate Hovered Branch", HierarchyShortcut.IsolateHoveredBranch);

            GUILayout.Space(4f);
            EditorGUILayout.LabelField(GUIContent.none, GUI.skin.horizontalSlider);
            var disabled = !HierarchyToolkitPreferences.ToolkitEnabled;
            var nextDisabled = EditorGUILayout.ToggleLeft("Disable Hierarchy Toolkit", disabled);
            if (nextDisabled != disabled)
                HierarchyToolkitPreferences.ToolkitEnabled = !nextDisabled;
        }

        private static void DrawHeader(string label)
        {
            var style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                normal = { textColor = EditorGUIUtility.isProSkin
                    ? new Color(0.58f, 0.58f, 0.58f)
                    : new Color(0.42f, 0.42f, 0.42f) }
            };
            EditorGUILayout.LabelField(label, style);
        }

        private static void DrawFeature(string label, HierarchyFeature feature)
        {
            var selected = HierarchyToolkitPreferences.IsFeatureSelected(feature);
            var next = EditorGUILayout.ToggleLeft(label, selected);
            if (next != selected) HierarchyToolkitPreferences.SetFeature(feature, next);
        }

        private static void DrawShortcut(string label, HierarchyShortcut shortcut)
        {
            var selected = HierarchyToolkitPreferences.IsShortcutSelected(shortcut);
            var next = EditorGUILayout.ToggleLeft(label, selected);
            if (next != selected) HierarchyToolkitPreferences.SetShortcut(shortcut, next);
        }

        private void Repaint() => editorWindow?.Repaint();
    }
}
