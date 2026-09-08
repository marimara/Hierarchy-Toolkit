using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed partial class SceneMetadataStoreTests
    {
        [Test]
        public void SeparatorAssignmentPersistsOptionsAndMarkPreservesEdits()
        {
            var style = new SeparatorStyle("Environment", Color.blue, Color.yellow, false);
            SeparatorOperations.Edit(target, style);
            SeparatorOperations.Mark(new[] { target });
            ReloadStore();
            Assert.That(store.TryGetMetadata(target, out var entry), Is.True);
            Assert.That(entry.Separator, Is.EqualTo(style));
            Assert.That(entry.IsEmpty, Is.False);
            Assert.That(store.CleanupEmptyMetadata(), Is.Zero);
            EditorSceneManager.CloseScene(scene, true);
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            target = scene.GetRootGameObjects()[0];
            Assert.That(SeparatorOperations.InitialStyle(target), Is.EqualTo(style));
        }

        [Test]
        public void ClearSeparatorPreservesColorAndIconAndUndoRestoresOptions()
        {
            store.SetColor(target, Color.cyan);
            store.SetIcon(target, "GameObject Icon");
            var style = new SeparatorStyle("Lighting", Color.blue, Color.yellow, false);
            SeparatorOperations.Edit(target, style);
            SeparatorOperations.Clear(new[] { target });
            Assert.That(store.TryGetMetadata(target, out var entry), Is.True);
            Assert.That(entry.Separator.Enabled, Is.False);
            Assert.That(entry.Separator.DisplayText, Is.Null);
            Assert.That(entry.CustomColor, Is.EqualTo(Color.cyan));
            Assert.That(entry.CustomIconReference, Is.EqualTo("builtin:GameObject Icon"));
            Undo.PerformUndo();
            Assert.That(SeparatorOperations.InitialStyle(target), Is.EqualTo(style));
            Undo.PerformRedo();
            Assert.That(store.TryGetMetadata(target, out entry), Is.True);
            Assert.That(entry.Separator.Enabled, Is.False);
            Assert.That(entry.HasColorOverride && entry.HasIconOverride, Is.True);
            store.ClearColor(target);
            store.ClearIcon(target);
            Assert.That(store.Count, Is.Zero);
        }

        [Test]
        public void ClearingOtherOverridesPreservesSeparatorAndDefaultStyleDisablesIt()
        {
            SeparatorOperations.Mark(new[] { target });
            store.SetColor(target, Color.red);
            store.SetIcon(target, "GameObject Icon");
            store.ClearColor(target);
            store.ClearIcon(target);
            Assert.That(store.TryGetMetadata(target, out var entry), Is.True);
            Assert.That(entry.Separator.Enabled && entry.Separator.Bold, Is.True);
            store.SetSeparator(target, default);
            Assert.That(store.Count, Is.Zero);
        }

        [Test]
        public void SeparatorMultiSelectionAcrossScenesUndoesAndRedoesTogether()
        {
            AssetDatabase.CopyAsset(scenePath, folder + "/SeparatorOther.unity");
            var otherScene = EditorSceneManager.OpenScene(folder + "/SeparatorOther.unity", OpenSceneMode.Additive);
            var otherStorePath = SceneMetadataStore.GetStorePath(otherScene);
            SceneMetadataStore otherStore = null;
            try
            {
                var other = otherScene.GetRootGameObjects()[0];
                var prefab = PrefabUtility.SaveAsPrefabAsset(target, folder + "/SeparatorPrefab.prefab");
                var targets = ManualColorOperations.ResolveTargets(target, new[] { target, other, target, prefab, null });
                Assert.That(targets, Is.EqualTo(new[] { target, other }));
                Assert.That(ManualColorOperations.ResolveTargets(target, new[] { other }), Is.EqualTo(new[] { target }));
                SeparatorOperations.Mark(targets);
                otherStore = SceneMetadataStore.Find(otherScene);
                Assert.That(store.Count + otherStore.Count, Is.EqualTo(2));
                Undo.PerformUndo();
                Assert.That(store.Count + otherStore.Count, Is.Zero);
                Undo.PerformRedo();
                Assert.That(store.Count + otherStore.Count, Is.EqualTo(2));
                SeparatorOperations.Edit(target, new SeparatorStyle("One object"));
                Assert.That(otherStore.TryGetMetadata(other, out var otherEntry), Is.True);
                Assert.That(otherEntry.Separator.DisplayText, Is.Null.Or.Empty);
                SeparatorOperations.Clear(new[] { target, other, prefab, null });
                Assert.That(store.Count + otherStore.Count, Is.Zero);
                Undo.PerformUndo();
                Assert.That(store.Count + otherStore.Count, Is.EqualTo(2));
                Undo.PerformRedo();
                Assert.That(store.Count + otherStore.Count, Is.Zero);
            }
            finally
            {
                if (otherStore != null) Undo.ClearUndo(otherStore);
                EditorSceneManager.CloseScene(otherScene, true);
                AssetDatabase.DeleteAsset(otherStorePath);
            }
        }

        [Test]
        public void SeparatorCacheInvalidatesForEditClearUndoAndSceneClose()
        {
            using var feature = new SeparatorFeature();
            var id = target.GetEntityId();
            SeparatorOperations.Mark(new[] { target });
            feature.Cache.Rebuild();
            Assert.That(feature.TryGet(id, out _), Is.True);
            SeparatorOperations.Edit(target, new SeparatorStyle("Changed"));
            Assert.That(feature.TryGet(id, out _), Is.False);
            feature.Cache.Rebuild();
            Assert.That(feature.TryGet(id, out var value), Is.True);
            Assert.That(value.DisplayText, Is.EqualTo("Changed"));
            feature.Enabled = false;
            Assert.That(feature.TryGet(id, out _), Is.False);
            feature.Enabled = true;
            Assert.That(feature.TryGet(id, out _), Is.True);
            SeparatorOperations.Clear(new[] { target });
            feature.Cache.Rebuild();
            Assert.That(feature.TryGet(id, out _), Is.False);
            Undo.PerformUndo();
            feature.Cache.Rebuild();
            Assert.That(feature.TryGet(id, out _), Is.True);
            EditorSceneManager.CloseScene(scene, true);
            feature.Cache.Rebuild();
            Assert.That(feature.TryGet(id, out _), Is.False);
        }
    }

    public sealed class SeparatorTests
    {
        [TestCase(null, "Object name")]
        [TestCase("", "Object name")]
        [TestCase("   ", "Object name")]
        [TestCase("Environment", "Environment")]
        public void DisplayTextFallsBackToObjectName(string text, string expected)
            => Assert.That(new SeparatorStyle(text).ResolveText("Object name"), Is.EqualTo(expected));

        [TestCase(false, false, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        public void ManualColorAndSelectionTakePrecedence(bool selected, bool manualColor, bool expected)
        {
            Assert.That(SeparatorFeature.ShouldDrawBackground(selected, manualColor), Is.EqualTo(expected));
            Assert.That(SeparatorFeature.ShouldDrawManualColor(selected, manualColor), Is.EqualTo(!selected && manualColor));
        }

        [Test]
        public void BackgroundDefaultsFollowThemeAndExplicitTransparentIsPreserved()
        {
            var style = new SeparatorStyle(null);
            Assert.That(SeparatorFeature.Background(style, true).r, Is.EqualTo(1f));
            Assert.That(SeparatorFeature.Background(style, false).r, Is.Zero);
            Assert.That(SeparatorFeature.Background(new SeparatorStyle(null, Color.clear), true), Is.EqualTo(Color.clear));
        }

        [Test]
        public void HeaderKeepsNativeRenameTextAndRestoresRecycledLabelAndIcon()
        {
            var native = new Label("Actual GameObject name");
            var icon = new VisualElement();
            var originalColor = native.style.color;
            var originalWidth = native.style.width;
            var originalVisibility = icon.style.visibility;
            using var presenter = new SeparatorLabel(native, icon);
            presenter.Apply(new SeparatorStyle("Section title"), false);
            Assert.That(native.text, Is.EqualTo("Actual GameObject name"), "Unity seeds rename from this value.");
            var header = native.Q<Label>("hierarchy-toolkit-header");
            Assert.That(header.text, Is.EqualTo("Section title"));
            Assert.That(header.pickingMode, Is.EqualTo(PickingMode.Ignore));
            Assert.That(icon.style.visibility.value, Is.EqualTo(Visibility.Hidden));
            // Name changes arrive through Unity's native binding; fallback uses the new value.
            native.text = "Renamed GameObject";
            presenter.Apply(new SeparatorStyle(null), false);
            Assert.That(header.text, Is.EqualTo("Renamed GameObject"));
            presenter.Apply(default, false);
            Assert.That(native.text, Is.EqualTo("Renamed GameObject"));
            Assert.That(native.childCount, Is.Zero);
            Assert.That(native.style.color, Is.EqualTo(originalColor));
            Assert.That(native.style.width, Is.EqualTo(originalWidth));
            Assert.That(icon.style.visibility, Is.EqualTo(originalVisibility));
        }
    }
}
