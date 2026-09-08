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
        [SerializeField, Range(0f, 1f)] private float gradientStartAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float gradientEndAlpha;

        internal event Action Changed;
        internal IReadOnlyList<Color> ColorPresets => colorPresets;
        internal float GradientStartAlpha => Mathf.Clamp01(gradientStartAlpha);
        internal float GradientEndAlpha => Mathf.Clamp01(gradientEndAlpha);

        internal void AddColor(Color color) => Change("Add Hierarchy Color Preset", () => colorPresets.Add(color));

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
