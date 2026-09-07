using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualIconPicker : OdinEditorWindow
    {
        private GameObject[] targets;

        [ShowInInspector, ReadOnly, PropertyOrder(0), LabelText("Applies to")]
        private string TargetDescription => targets == null ? "No objects" : targets.Length + " GameObject(s)";

        [ShowInInspector, PropertyOrder(1), LabelText("Icon"), AssetsOnly, PreviewField(64),
         AssetSelector(Filter = "t:Texture2D t:Sprite"), ValidateInput(nameof(IsValid), "Choose a project Texture2D or Sprite.")]
        private UnityEngine.Object chosenIcon;

        private bool IsValid(UnityEngine.Object value) =>
            (value is Texture2D || value is Sprite) && AssetDatabase.Contains(value);

        internal static void Open(GameObject[] targets, UnityEngine.Object initial)
        {
            var window = CreateInstance<ManualIconPicker>();
            window.targets = targets;
            window.chosenIcon = initial;
            window.titleContent = new GUIContent("Hierarchy Icon");
            window.minSize = new Vector2(340, 190);
            window.ShowUtility();
        }

        [Button("Apply"), HorizontalGroup("Actions"), PropertyOrder(2), EnableIf(nameof(CanApply))]
        private void Apply()
        {
            if (!CanApply) return;
            ManualIconOperations.Apply(targets, chosenIcon);
            Close();
        }

        private bool CanApply => IsValid(chosenIcon);

        [Button("Cancel"), HorizontalGroup("Actions"), PropertyOrder(2)]
        private void Cancel() => Close();
    }
}
