using UnityEditor;

namespace Meganeura.HierarchyToolkit
{
    [InitializeOnLoad]
    internal static class HierarchyToolkitBootstrap
    {
        private static readonly ManualColorFeature ManualColors = new ManualColorFeature();

        private static readonly ManualIconFeature ManualIcons = new ManualIconFeature();
        private static readonly QuickStylePaletteFeature QuickStylePalette = new QuickStylePaletteFeature();
        internal static readonly ZebraStripingFeature ZebraStriping = new ZebraStripingFeature(ManualColors.Cache, () => ManualColors.Enabled);
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
        private static readonly VisualHierarchyBinding Visuals = new VisualHierarchyBinding(ZebraStriping, HierarchyLines, ManualColors, Separators, Activation, ComponentMinimap);

        static HierarchyToolkitBootstrap()
        {
            HierarchyToolkitPreferences.Changed += ApplyPreferences;
            ApplyPreferences();
            HierarchyDrawer.Initialize();
            HierarchyDrawer.Register(ZebraStriping);
            HierarchyDrawer.Register(Separators);
            HierarchyDrawer.Register(ManualColors);
            HierarchyDrawer.Register(HierarchyLines);
            HierarchyDrawer.Register(ManualIcons);
            HierarchyDrawer.Register(Activation);
            HierarchyDrawer.Register(ComponentMinimap);
            HierarchyDrawer.Register(QuickStylePalette);
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        private static void ApplyPreferences()
        {
            QuickStylePalette.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.QuickStylePalette);
            SceneSelector.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.SceneSelector);
            ComponentMinimap.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ComponentMinimap);
            Activation.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ActivationToggle);
            HierarchyLines.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.HierarchyLines);
            ZebraStriping.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ZebraStriping);
            ManualColors.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ManualColors);
            ManualIcons.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ManualIcons);
            Separators.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.Separators);
            BookmarkNavigation.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.GameObjectBookmarks);
            DefaultParentVisuals.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent);
            DefaultParentCreation.Enabled = HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent);
            if (!HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ComponentPopupInspector))
                ComponentPopupInspector.CloseCurrent();
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void Shutdown()
        {
            HierarchyToolkitPreferences.Changed -= ApplyPreferences;
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
            QuickStylePalette.Dispose();
            HierarchyDrawer.Shutdown();
        }
    }
}


