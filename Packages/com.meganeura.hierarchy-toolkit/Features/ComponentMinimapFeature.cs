using System;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ComponentMinimapFeature : IHierarchyFeature, IDisposable
    {
        internal readonly ComponentMinimapCache Cache = new();
        internal event Action Changed;
        private bool enabled = true;

        internal ComponentMinimapFeature() => Cache.Changed += NotifyChanged;

        internal bool Enabled
        {
            get => enabled;
            set { if (enabled == value) return; enabled = value; NotifyChanged(); }
        }

        internal ComponentMinimapCache.Icon[] GetIcons(GameObject target)
            => Enabled ? Cache.GetIcons(target) : Array.Empty<ComponentMinimapCache.Icon>();

        private void NotifyChanged() => Changed?.Invoke();

        // Presentation belongs to the existing UI Toolkit row binding.
        public void Draw(in HierarchyRowContext context) { }

        public void Dispose()
        {
            Cache.Changed -= NotifyChanged;
            Cache.Dispose();
        }
    }
}
