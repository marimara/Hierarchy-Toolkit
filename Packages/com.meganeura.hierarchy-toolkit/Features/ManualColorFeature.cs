using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualColorFeature : IHierarchyFeature, IDisposable
    {
        private readonly ManualColorCache cache = new ManualColorCache();
        internal ManualColorCache Cache => cache;
        private Texture2D gradientTexture;

        internal ManualColorFeature() => QuickStylePaletteSettings.instance.Changed += SettingsChanged;

        public void Draw(in HierarchyRowContext context)
        {
            if (Event.current.type != EventType.Repaint || context.RowRect.width <= 0f
                || Selection.Contains(context.EntityId) || !cache.TryGetColor(context.EntityId, out var color)) return;
            if (color.a <= 0f) return;
            EnsureGradientTexture();
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(context.RowRect, gradientTexture, ScaleMode.StretchToFill, true);
            GUI.color = previous;
        }

        private void EnsureGradientTexture()
        {
            if (gradientTexture != null) return;
            const int width = 64;
            gradientTexture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                name = "Hierarchy Toolkit Manual Color Gradient",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[width];
            for (var i = 0; i < width; ++i)
                pixels[i] = new Color(1f, 1f, 1f, ManualColorGradient.Alpha(i / (width - 1f)));
            gradientTexture.SetPixels(pixels);
            gradientTexture.Apply(false, true);
        }

        private void SettingsChanged()
        {
            if (gradientTexture != null) UnityEngine.Object.DestroyImmediate(gradientTexture);
            gradientTexture = null;
        }

        public void Dispose()
        {
            QuickStylePaletteSettings.instance.Changed -= SettingsChanged;
            if (gradientTexture != null) UnityEngine.Object.DestroyImmediate(gradientTexture);
            cache.Dispose();
        }
    }
}
