using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class QuickStylePaletteFeature : IHierarchyFeature, IDisposable
    {
        private readonly QuickStylePaletteHierarchyBinding binding;
        private bool enabled = true;
        internal event Action Changed;

        internal QuickStylePaletteFeature() => binding = new QuickStylePaletteHierarchyBinding(() => Enabled);

        internal bool Enabled
        {
            get => enabled;
            set { if (enabled == value) return; enabled = value; if (!enabled) QuickStylePalette.CancelQueuedOpen(); Changed?.Invoke(); }
        }

        public void Draw(in HierarchyRowContext context)
        {
            if (!Enabled) return;
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
