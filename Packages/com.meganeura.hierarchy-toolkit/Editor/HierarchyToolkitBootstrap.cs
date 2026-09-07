using UnityEditor;

namespace Meganeura.HierarchyToolkit
{
    [InitializeOnLoad]
    internal static class HierarchyToolkitBootstrap
    {
        private static readonly ManualColorFeature ManualColors = new ManualColorFeature();

        private static readonly ManualIconFeature ManualIcons = new ManualIconFeature();

        static HierarchyToolkitBootstrap()
        {
            HierarchyDrawer.Initialize();
            HierarchyDrawer.Register(ManualColors);
            HierarchyDrawer.Register(ManualIcons);
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        private static void Shutdown()
        {
            ManualColors.Dispose();
            ManualIcons.Dispose();
            HierarchyDrawer.Shutdown();
        }
    }
}


