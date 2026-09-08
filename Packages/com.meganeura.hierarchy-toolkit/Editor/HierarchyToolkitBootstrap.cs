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
        internal static readonly ComponentMinimapFeature ComponentMinimap = new ComponentMinimapFeature();
        internal static readonly GameObjectBookmarkStore Bookmarks = GameObjectBookmarkStore.Open(GameObjectBookmarkStore.ProjectKey);
        private static readonly GameObjectBookmarkNavigation BookmarkNavigation = new GameObjectBookmarkNavigation(Bookmarks);
        private static readonly HierarchyShortcuts Shortcuts = new HierarchyShortcuts();
        internal static readonly DefaultParentCache DefaultParents = new DefaultParentCache();
        private static readonly DefaultParentHierarchyBinding DefaultParentVisuals = new DefaultParentHierarchyBinding(DefaultParents);
        private static readonly DefaultParentCreationHandler DefaultParentCreation = new DefaultParentCreationHandler();
        internal static readonly SceneFavoriteStore SceneFavorites = SceneFavoriteStore.Open(SceneFavoriteStore.ProjectKey);
        internal static readonly SceneCatalog Scenes = new SceneCatalog();
        private static readonly SceneSelectorHierarchyBinding SceneSelector = new SceneSelectorHierarchyBinding(Scenes, SceneFavorites);
        private static readonly VisualHierarchyBinding Visuals = new VisualHierarchyBinding(ZebraStriping, HierarchyLines, ManualColors.Cache, Separators, Activation, ComponentMinimap);

        static HierarchyToolkitBootstrap()
        {
            HierarchyDrawer.Initialize();
            HierarchyDrawer.Register(ZebraStriping);
            HierarchyDrawer.Register(Separators);
            HierarchyDrawer.Register(ManualColors);
            HierarchyDrawer.Register(HierarchyLines);
            HierarchyDrawer.Register(ManualIcons);
            HierarchyDrawer.Register(Activation);
            HierarchyDrawer.Register(ComponentMinimap);
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        private static void Shutdown()
        {
            Shortcuts.Dispose();
            DefaultParentCreation.Dispose();
            DefaultParentVisuals.Dispose();
            DefaultParents.Dispose();
            BookmarkNavigation.Dispose();
            UnityEngine.Object.DestroyImmediate(Bookmarks);
            SceneSelector.Dispose();
            Scenes.Dispose();
            UnityEngine.Object.DestroyImmediate(SceneFavorites);
            Visuals.Dispose();
            ComponentMinimap.Dispose();
            Separators.Dispose();
            ManualColors.Dispose();
            ManualIcons.Dispose();
            HierarchyDrawer.Shutdown();
        }
    }
}


