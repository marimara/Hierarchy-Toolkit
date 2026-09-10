using UnityEditor;

namespace Meganeura.ProjectToolkit
{
    [InitializeOnLoad]
    internal static class ProjectToolkitBootstrap
    {
        private static ProjectFeatureCoordinator coordinator;

        static ProjectToolkitBootstrap()
        {
            Initialize();
        }

        internal static bool Register(IProjectFeature feature)
        {
            return coordinator.Register(feature);
        }

        internal static bool Unregister(IProjectFeature feature)
        {
            return coordinator.Unregister(feature);
        }

        private static void Initialize()
        {
            Shutdown();

            coordinator = new ProjectFeatureCoordinator(
                new UnityProjectWindowIntegration(),
                new ProjectItemResolver());
            coordinator.Register(new ManualFolderColorFeature(ProjectFolderMetadataService.Repository));

            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        private static void Shutdown()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= Shutdown;
            EditorApplication.quitting -= Shutdown;
            coordinator?.Dispose();
            coordinator = null;
        }
    }
}
