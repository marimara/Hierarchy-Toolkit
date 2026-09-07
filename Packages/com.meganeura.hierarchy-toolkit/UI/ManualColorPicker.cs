using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualColorPicker : OdinEditorWindow
    {
        private GameObject[] targets;

        [ShowInInspector, ReadOnly, PropertyOrder(0), LabelText("Applies to")]
        private string TargetDescription => targets == null ? "No objects" : targets.Length + " GameObject(s)";

        [ShowInInspector, PropertyOrder(1), LabelText("Color"), ColorUsage(true)]
        private Color chosenColor;

        internal static void Open(GameObject[] targets, Color initialColor)
        {
            var window = CreateInstance<ManualColorPicker>();
            window.targets = targets;
            window.chosenColor = initialColor;
            window.titleContent = new GUIContent("Hierarchy Color");
            window.minSize = new Vector2(320f, 140f);
            window.ShowUtility();
        }

        [Button("Apply"), HorizontalGroup("Actions"), PropertyOrder(2)]
        private void Apply()
        {
            ManualColorOperations.Apply(targets, chosenColor);
            Close();
        }

        [Button("Cancel"), HorizontalGroup("Actions"), PropertyOrder(2)]
        private void Cancel() => Close();
    }
}
