using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualColorFeature : IHierarchyFeature, IDisposable
    {
        private readonly ManualColorCache cache = new ManualColorCache();
        private readonly ManualColorHierarchyBinding hierarchyBinding;

        internal ManualColorFeature()
        {
            hierarchyBinding = new ManualColorHierarchyBinding(cache);
        }

        public void Draw(in HierarchyRowContext context)
        {
            if (Event.current.type != EventType.Repaint || context.RowRect.width <= 0f
                || Selection.Contains(context.EntityId) || !cache.TryGetColor(context.EntityId, out var color)) return;
            // Unity has already drawn its label and prefab indicators. A restrained translucent
            // tint preserves those details without recreating Unity's text or capturing input.
            color.a = Mathf.Clamp01(color.a) * 0.18f;
            if (color.a > 0f) EditorGUI.DrawRect(context.RowRect, color);
        }

        public void Dispose()
        {
            hierarchyBinding.Dispose();
            cache.Dispose();
        }
    }
}
