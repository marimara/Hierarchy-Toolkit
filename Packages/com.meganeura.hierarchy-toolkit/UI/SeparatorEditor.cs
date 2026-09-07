using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class SeparatorEditor : OdinEditorWindow
    {
        private GameObject target;
        [ShowInInspector, ReadOnly, PropertyOrder(0), LabelText("GameObject")]
        private string TargetName => target != null ? target.name : "Object unavailable";
        [ShowInInspector, PropertyOrder(1), LabelText("Display text"), Tooltip("Leave empty to use the GameObject name. Renaming still edits the actual GameObject name.")]
        private string displayText;
        [ShowInInspector, PropertyOrder(2), LabelText("Custom background")]
        private bool customBackground;
        [ShowInInspector, PropertyOrder(3), ShowIf(nameof(customBackground)), LabelText("Background"), ColorUsage(true)]
        private Color background;
        [ShowInInspector, PropertyOrder(4), LabelText("Custom text color")]
        private bool customTextColor;
        [ShowInInspector, PropertyOrder(5), ShowIf(nameof(customTextColor)), LabelText("Text color"), ColorUsage(false)]
        private Color textColor;
        [ShowInInspector, PropertyOrder(6), LabelText("Bold")]
        private bool bold;

        internal static void Open(GameObject target)
        {
            var value = SeparatorOperations.InitialStyle(target);
            var window = CreateInstance<SeparatorEditor>();
            window.target = target;
            window.displayText = value.DisplayText;
            window.customBackground = value.HasBackgroundColor;
            window.background = value.HasBackgroundColor ? value.BackgroundColor : SeparatorFeature.Background(value, EditorGUIUtility.isProSkin);
            window.customTextColor = value.HasTextColor;
            window.textColor = value.HasTextColor ? value.TextColor : EditorGUIUtility.isProSkin ? Color.white : Color.black;
            window.bold = value.Bold;
            window.titleContent = new GUIContent("Hierarchy Separator");
            window.minSize = new Vector2(350f, 270f);
            window.position = new Rect(200f, 200f, 370f, 290f);
            window.ShowUtility();
        }

        [Button("Apply"), HorizontalGroup("Actions"), PropertyOrder(7)]
        private void Apply()
        {
            SeparatorOperations.Edit(target, new SeparatorStyle(displayText,
                customBackground ? background : (Color?)null, customTextColor ? textColor : (Color?)null, bold));
            Close();
        }
        [Button("Cancel"), HorizontalGroup("Actions"), PropertyOrder(7)]
        private void Cancel() => Close();
    }
}
