using System;

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

        // UI Toolkit binding owns interaction so the picked element can take priority over the row.
        public void Draw(in HierarchyRowContext context) { }

        public void Dispose()
        {
            binding.Dispose();
            QuickStylePalette.CancelQueuedOpen();
        }
    }
}
