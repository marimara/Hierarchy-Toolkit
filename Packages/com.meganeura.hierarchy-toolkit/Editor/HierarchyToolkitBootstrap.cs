using UnityEditor;

namespace Meganeura.HierarchyToolkit
{
    [InitializeOnLoad]
    internal static class HierarchyToolkitBootstrap
    {
        private static readonly ManualColorFeature ManualColors = new ManualColorFeature();

        private static readonly ManualIconFeature ManualIcons = new ManualIconFeature();
        internal static readonly ZebraStripingFeature ZebraStriping = new ZebraStripingFeature(ManualColors.Cache);
        internal static readonly HierarchyLinesFeature HierarchyLines = new HierarchyLinesFeature();
        private static readonly VisualHierarchyBinding Visuals = new VisualHierarchyBinding(ZebraStriping, HierarchyLines, ManualColors.Cache);

        static HierarchyToolkitBootstrap()
        {
            HierarchyDrawer.Initialize();
            HierarchyDrawer.Register(ZebraStriping);
            HierarchyDrawer.Register(ManualColors);
            HierarchyDrawer.Register(HierarchyLines);
            HierarchyDrawer.Register(ManualIcons);
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        private static void Shutdown()
        {
            Visuals.Dispose();
            ManualColors.Dispose();
            ManualIcons.Dispose();
            HierarchyDrawer.Shutdown();
        }
    }
}


