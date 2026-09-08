using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class QuickStylePaletteFeature : IHierarchyFeature, IDisposable
    {
        private readonly QuickStylePaletteHierarchyBinding binding = new();

        public void Draw(in HierarchyRowContext context)
        {
            var current = Event.current;
            if (current == null || current.type != EventType.MouseDown || current.button != 0 || !current.alt
                || !context.RowRect.Contains(current.mousePosition)) return;
            var target = EditorUtility.EntityIdToObject(context.EntityId) as GameObject;
            if (!ManualColorOperations.IsSupported(target)) return;
            QuickStylePalette.QueueOpen(target, GUIUtility.GUIToScreenRect(context.RowRect));
        }

        public void Dispose()
        {
            binding.Dispose();
            QuickStylePalette.CancelQueuedOpen();
        }
    }
}
