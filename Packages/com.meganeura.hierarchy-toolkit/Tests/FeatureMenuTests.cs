using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class FeatureMenuTests
    {
        private bool toolkit;
        private readonly bool[] features = new bool[(int)HierarchyFeature.Count];
        private readonly bool[] shortcuts = new bool[(int)HierarchyShortcut.Count];

        [SetUp]
        public void SetUp()
        {
            toolkit = HierarchyToolkitPreferences.ToolkitEnabled;
            for (var i = 0; i < features.Length; ++i)
                features[i] = HierarchyToolkitPreferences.IsFeatureSelected((HierarchyFeature)i);
            for (var i = 0; i < shortcuts.Length; ++i)
                shortcuts[i] = HierarchyToolkitPreferences.IsShortcutSelected((HierarchyShortcut)i);
            HierarchyToolkitPreferences.ToolkitEnabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = 0; i < features.Length; ++i)
                HierarchyToolkitPreferences.SetFeature((HierarchyFeature)i, features[i]);
            for (var i = 0; i < shortcuts.Length; ++i)
                HierarchyToolkitPreferences.SetShortcut((HierarchyShortcut)i, shortcuts[i]);
            HierarchyToolkitPreferences.ToolkitEnabled = toolkit;
        }

        [Test]
        public void FeaturePreferencesAreIndependentPersistentAndDriveExistingEnabledState()
        {
            var wasDirty = EditorSceneManager.GetActiveScene().isDirty;
            HierarchyToolkitPreferences.SetFeature(HierarchyFeature.HierarchyLines, false);
            HierarchyToolkitPreferences.SetFeature(HierarchyFeature.ZebraStriping, true);

            Assert.That(HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.HierarchyLines), Is.False);
            Assert.That(HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ZebraStriping), Is.True);
            Assert.That(HierarchyToolkitBootstrap.HierarchyLines.Enabled, Is.False);
            Assert.That(HierarchyToolkitBootstrap.ZebraStriping.Enabled, Is.True);
            Assert.That(EditorPrefs.GetBool(HierarchyToolkitPreferences.FeaturePreferenceKey(HierarchyFeature.HierarchyLines), true), Is.False);
            Assert.That(EditorSceneManager.GetActiveScene().isDirty, Is.EqualTo(wasDirty));
        }

        [Test]
        public void ShortcutPreferencesDoNotChangeUnderlyingFeatures()
        {
            HierarchyToolkitPreferences.SetFeature(HierarchyFeature.DefaultParent, true);
            HierarchyToolkitPreferences.SetShortcut(HierarchyShortcut.ToggleDefaultParent, false);

            Assert.That(HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent), Is.True);
            Assert.That(HierarchyToolkitPreferences.IsShortcutEnabled(HierarchyShortcut.ToggleDefaultParent), Is.False);
            Assert.That(EditorPrefs.GetBool(HierarchyToolkitPreferences.ShortcutPreferenceKey(HierarchyShortcut.ToggleDefaultParent), true), Is.False);
        }

        [Test]
        public void MasterDisablePreservesIndividualSelectionsAndRestoresEffectiveState()
        {
            HierarchyToolkitPreferences.SetFeature(HierarchyFeature.HierarchyLines, false);
            HierarchyToolkitPreferences.SetFeature(HierarchyFeature.ZebraStriping, true);
            HierarchyToolkitPreferences.SetShortcut(HierarchyShortcut.ExpandCollapseHovered, true);

            HierarchyToolkitPreferences.ToolkitEnabled = false;
            Assert.That(HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ZebraStriping), Is.False);
            Assert.That(HierarchyToolkitPreferences.IsShortcutEnabled(HierarchyShortcut.ExpandCollapseHovered), Is.False);
            Assert.That(HierarchyToolkitPreferences.IsFeatureSelected(HierarchyFeature.HierarchyLines), Is.False);
            Assert.That(HierarchyToolkitPreferences.IsFeatureSelected(HierarchyFeature.ZebraStriping), Is.True);

            HierarchyToolkitPreferences.ToolkitEnabled = true;
            Assert.That(HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.HierarchyLines), Is.False);
            Assert.That(HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ZebraStriping), Is.True);
            Assert.That(HierarchyToolkitPreferences.IsShortcutEnabled(HierarchyShortcut.ExpandCollapseHovered), Is.True);
            Assert.That(EditorPrefs.GetBool(HierarchyToolkitPreferences.ToolkitPreferenceKey, false), Is.True);
        }
    }
}
