using System;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class SeparatorFeature : IHierarchyFeature, IDisposable
    {
        internal sealed class SeparatorCache : ManualMetadataCache<SeparatorStyle>
        {
            internal bool TryGet(EntityId id, out SeparatorStyle style) => TryGetValue(id, out style);
            protected override bool TryResolve(GameObjectMetadata entry, out SeparatorStyle value)
            {
                value = entry.Separator;
                return value.Enabled;
            }
        }

        private readonly SeparatorCache cache = new();
        internal SeparatorCache Cache => cache;
        private bool enabled = true;
        internal event Action Changed;
        internal bool Enabled
        {
            get => enabled;
            set { if (enabled == value) return; enabled = value; Changed?.Invoke(); }
        }

        internal SeparatorFeature() => cache.Changed += NotifyChanged;
        private void NotifyChanged() => Changed?.Invoke();
        internal bool TryGet(EntityId id, out SeparatorStyle style)
        {
            style = default;
            return Enabled && cache.TryGet(id, out style);
        }

        // Explicit manual color (including transparent) owns the background. Native selection wins over all colors.
        internal static bool ShouldDrawBackground(bool selected, bool manualColor) => !selected && !manualColor;
        internal static bool ShouldDrawManualColor(bool selected, bool manualColor) => !selected && manualColor;
        internal static Color Background(SeparatorStyle style, bool dark) => style.HasBackgroundColor
            ? style.BackgroundColor : dark ? new Color(1f, 1f, 1f, 0.085f) : new Color(0f, 0f, 0f, 0.085f);

        // Native retained rows supply the name label and preserve Unity's rename/foldout interactions.
        public void Draw(in HierarchyRowContext context) { }
        public void Dispose() { cache.Changed -= NotifyChanged; cache.Dispose(); }
    }
}
