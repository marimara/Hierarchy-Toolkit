using System;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualIconFeature : IHierarchyFeature, IDisposable
    {
        private readonly ManualIconCache cache = new();
        private readonly ManualIconHierarchyBinding binding;

        internal ManualIconFeature() => binding = new ManualIconHierarchyBinding(cache);

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
