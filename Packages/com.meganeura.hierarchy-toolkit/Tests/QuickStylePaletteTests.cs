using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed partial class SceneMetadataStoreTests
    {
        [Test]
        public void RecursiveColorTargetsRootAndDescendantsOnlyAndUndoesAsOneAction()
        {
            var child = new GameObject("Child");
            var grandchild = new GameObject("Grandchild");
            var unrelated = new GameObject("Unrelated");
            SceneManager.MoveGameObjectToScene(child, scene);
            SceneManager.MoveGameObjectToScene(grandchild, scene);
            SceneManager.MoveGameObjectToScene(unrelated, scene);
            child.transform.SetParent(target.transform);
            grandchild.transform.SetParent(child.transform);
            Assert.That(EditorSceneManager.SaveScene(scene), Is.True);

            var targets = ManualColorOperations.ResolveRecursiveTargets(target);
            Assert.That(targets, Is.EqualTo(new[] { target, child, grandchild }));
            ManualColorOperations.Apply(targets, Color.magenta);
            Assert.That(store.TryGetMetadata(target, out var rootEntry) && rootEntry.CustomColor == Color.magenta, Is.True);
            Assert.That(store.TryGetMetadata(child, out var childEntry) && childEntry.CustomColor == Color.magenta, Is.True);
            Assert.That(store.TryGetMetadata(grandchild, out var grandchildEntry) && grandchildEntry.CustomColor == Color.magenta, Is.True);
            Assert.That(store.TryGetMetadata(unrelated, out _), Is.False);

            Undo.PerformUndo();
            Assert.That(store.Count, Is.Zero);
            Undo.PerformRedo();
            Assert.That(store.Count, Is.EqualTo(3));
            ManualColorOperations.Apply(targets, null);
            Assert.That(store.Count, Is.Zero);
            Undo.PerformUndo();
            Assert.That(store.Count, Is.EqualTo(3));
        }

        [Test]
        public void BuiltInIconPresetUsesExistingManualIconMetadataAndUndo()
        {
            ManualIconOperations.ApplyReference(new[] { target }, "builtin:Folder Icon");
            Assert.That(store.TryGetMetadata(target, out var entry), Is.True);
            Assert.That(entry.CustomIconReference, Is.EqualTo("builtin:Folder Icon"));
            Assert.That(entry.HasIconOverride, Is.True);
            Undo.PerformUndo();
            Assert.That(store.Count, Is.Zero);
            Undo.PerformRedo();
            Assert.That(store.GetOrCreateMetadata(target).CustomIconReference, Is.EqualTo("builtin:Folder Icon"));
        }
    }

    public sealed class QuickStylePaletteTests
    {
        [Test]
        public void GradientUsesConfiguredHorizontalEndpointsWithoutChangingSourceColor()
        {
            var source = new Color(0.2f, 0.4f, 0.6f, 0.7f);
            var copy = source;
            Assert.That(ManualColorGradient.Alpha(0f), Is.EqualTo(1f));
            Assert.That(ManualColorGradient.Alpha(1f), Is.EqualTo(0f));
            Assert.That(ManualColorGradient.Alpha(0.5f), Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(source, Is.EqualTo(copy));
        }

        [Test]
        public void ProjectPaletteStartsWithCompactReusableColorSet()
        {
            var presets = QuickStylePaletteSettings.instance.ColorPresets;
            Assert.That(presets.Count, Is.GreaterThanOrEqualTo(9));
            for (var i = 0; i < presets.Count; ++i)
                Assert.That(presets[i].a, Is.GreaterThan(0f));
        }
    }
}
