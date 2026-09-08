using System;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualIconFeature : IHierarchyFeature, IDisposable
    {
        private readonly ManualIconCache cache = new();
        private readonly ManualIconHierarchyBinding binding;
        private bool enabled = true;
        internal event Action Changed;

        internal ManualIconFeature() => binding = new ManualIconHierarchyBinding(cache, () => Enabled);

        internal bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                binding.Refresh();
                Changed?.Invoke();
            }
        }

        // Unity 6.6 renders icons through its retained Hierarchy row elements. Updating their
        // native icon slot preserves layout and interaction; no IMGUI overlay is needed.
        public void Draw(in HierarchyRowContext context) { }

        public void Dispose()
        {
            binding.Dispose();
            cache.Dispose();
        }
    }
}
