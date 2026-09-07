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
        internal static readonly SeparatorFeature Separators = new SeparatorFeature();
        internal static readonly ActivationToggleFeature Activation = new ActivationToggleFeature();
        private static readonly VisualHierarchyBinding Visuals = new VisualHierarchyBinding(ZebraStriping, HierarchyLines, ManualColors.Cache, Separators, Activation);

        static HierarchyToolkitBootstrap()
        {
            HierarchyDrawer.Initialize();
            HierarchyDrawer.Register(ZebraStriping);
            HierarchyDrawer.Register(Separators);
            HierarchyDrawer.Register(ManualColors);
            HierarchyDrawer.Register(HierarchyLines);
            HierarchyDrawer.Register(ManualIcons);
            HierarchyDrawer.Register(Activation);
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        private static void Shutdown()
        {
            Visuals.Dispose();
            Separators.Dispose();
            ManualColors.Dispose();
            ManualIcons.Dispose();
            HierarchyDrawer.Shutdown();
        }
    }
}


