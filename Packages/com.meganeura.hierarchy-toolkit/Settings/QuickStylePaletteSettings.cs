using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    [FilePath("ProjectSettings/HierarchyToolkitQuickStylePalette.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class QuickStylePaletteSettings : ScriptableSingleton<QuickStylePaletteSettings>
    {
        [SerializeField] private List<Color> colorPresets = new()
        {
            new Color(0.16f, 0.17f, 0.19f, 1f),
            new Color(0.88f, 0.25f, 0.28f, 1f),
            new Color(0.87f, 0.67f, 0.23f, 1f),
            new Color(0.39f, 0.68f, 0.22f, 1f),
            new Color(0.20f, 0.65f, 0.39f, 1f),
            new Color(0.18f, 0.65f, 0.67f, 1f),
            new Color(0.24f, 0.45f, 0.84f, 1f),
            new Color(0.52f, 0.25f, 0.76f, 1f),
            new Color(0.72f, 0.24f, 0.58f, 1f)
        };
        [SerializeField] private List<string> iconPresets = new()
        {
            "builtin:Folder Icon", "builtin:Camera Icon", "builtin:Light Icon",
            "builtin:AudioSource Icon", "builtin:Canvas Icon", "builtin:Prefab Icon",
            "builtin:d_UnityEditor.ConsoleWindow", "builtin:d_SceneViewTools",
            "builtin:Favorite", "builtin:Settings"
        };
        [SerializeField] private List<string> customIconLibrary = new();
        [SerializeField, Range(0f, 1f)] private float gradientStartAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float gradientEndAlpha;

        internal event Action Changed;
        internal IReadOnlyList<Color> ColorPresets => colorPresets;
        internal IReadOnlyList<string> IconPresets => iconPresets;
        internal IReadOnlyList<string> CustomIconLibrary => customIconLibrary ??= new List<string>();
        internal float GradientStartAlpha => Mathf.Clamp01(gradientStartAlpha);
        internal float GradientEndAlpha => Mathf.Clamp01(gradientEndAlpha);

        internal void AddColor(Color color) => Change("Add Hierarchy Color Preset", () => colorPresets.Add(color));

        internal bool ToggleColor(Color color)
        {
            var index = MatchingColorIndex(colorPresets, color);
            if (index >= 0) Change("Remove Hierarchy Color Favorite", () => colorPresets.RemoveAt(index));
            else Change("Add Hierarchy Color Favorite", () => colorPresets.Add(color));
            return index < 0;
        }

        internal bool ToggleIcon(string reference)
        {
            if (string.IsNullOrEmpty(reference)) return false;
            reference = HierarchyIconReference.Normalize(reference);
            var index = MatchingReferenceIndex(iconPresets, reference);
            if (index >= 0) Change("Remove Hierarchy Icon Favorite", () => iconPresets.RemoveAt(index));
            else Change("Add Hierarchy Icon Favorite", () => iconPresets.Add(reference));
            return index < 0;
        }

        internal bool AddCustomIcon(Sprite sprite)
        {
            if (sprite == null || !AssetDatabase.Contains(sprite)) return false;
            customIconLibrary ??= new List<string>();
            var reference = HierarchyIconReference.FromAsset(sprite);
            if (MatchingReferenceIndex(customIconLibrary, reference) >= 0) return false;
            Change("Add Hierarchy Custom Icon", () => customIconLibrary.Add(reference));
            return true;
        }

        internal bool RemoveCustomIcon(string reference)
        {
            customIconLibrary ??= new List<string>();
            var index = MatchingReferenceIndex(customIconLibrary, reference);
            if (index < 0) return false;
            Change("Remove Hierarchy Custom Icon", () => customIconLibrary.RemoveAt(index));
            return true;
        }

        internal static int MatchingColorIndex(IReadOnlyList<Color> colors, Color color)
        {
            for (var i = 0; i < colors.Count; ++i)
                if (Approximately(colors[i], color)) return i;
            return -1;
        }

        internal static int MatchingReferenceIndex(IReadOnlyList<string> references, string reference)
        {
            for (var i = 0; i < references.Count; ++i)
                if (string.Equals(references[i], reference, StringComparison.Ordinal)) return i;
            return -1;
        }

        private static bool Approximately(Color left, Color right)
            => Mathf.Abs(left.r - right.r) <= 0.001f && Mathf.Abs(left.g - right.g) <= 0.001f
                && Mathf.Abs(left.b - right.b) <= 0.001f && Mathf.Abs(left.a - right.a) <= 0.001f;

        internal void SetColor(int index, Color color)
        {
            if (index < 0 || index >= colorPresets.Count || colorPresets[index].Equals(color)) return;
            Change("Edit Hierarchy Color Preset", () => colorPresets[index] = color);
        }

        internal void RemoveColor(int index)
        {
            if (index < 0 || index >= colorPresets.Count) return;
            Change("Remove Hierarchy Color Preset", () => colorPresets.RemoveAt(index));
        }

        internal void MoveColor(int index, int offset)
        {
            var destination = index + offset;
            if (index < 0 || index >= colorPresets.Count || destination < 0 || destination >= colorPresets.Count) return;
            Change("Reorder Hierarchy Color Presets", () =>
            {
                var color = colorPresets[index];
                colorPresets.RemoveAt(index);
                colorPresets.Insert(destination, color);
            });
        }

        private void Change(string undoName, Action mutation)
        {
            Undo.RegisterCompleteObjectUndo(this, undoName);
            mutation();
            Save(true);
            Changed?.Invoke();
            EditorApplication.RepaintHierarchyWindow();
        }

        private void OnValidate()
        {
            gradientStartAlpha = Mathf.Clamp01(gradientStartAlpha);
            gradientEndAlpha = Mathf.Clamp01(gradientEndAlpha);
            Changed?.Invoke();
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
