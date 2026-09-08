using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed partial class SceneMetadataStoreTests
    {
        private Texture2D CreateIcon(string name = "Icon")
        {
            var texture = new Texture2D(8, 8) { name = name };
            AssetDatabase.CreateAsset(texture, folder + "/" + name + ".asset");
            return texture;
        }

        [Test]
        public void IconAssignmentChangeClearPreservesColorAndInitialPicker()
        {
            var first = CreateIcon();
            var second = CreateIcon("Second");
            store.SetColor(target, Color.cyan);
            ManualIconOperations.Apply(new[] { target }, first);
            Assert.That(ManualIconOperations.InitialIcon(target), Is.EqualTo(first));
            Assert.That(store.TryGetMetadata(target, out var entry), Is.True);
            Assert.That(entry.CustomIconReference, Does.StartWith("asset:" + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(first)) + ":"));
            ManualIconOperations.Apply(new[] { target }, second);
            Assert.That(ManualIconOperations.InitialIcon(target), Is.EqualTo(second));
            ManualIconOperations.Apply(new[] { target }, null);
            Assert.That(store.TryGetMetadata(target, out entry), Is.True);
            Assert.That(entry.HasIconOverride, Is.False);
            Assert.That(entry.CustomColor, Is.EqualTo(Color.cyan));
            Undo.PerformUndo();
            Assert.That(ManualIconOperations.InitialIcon(target), Is.EqualTo(second));
            Undo.PerformRedo();
            Assert.That(ManualIconOperations.InitialIcon(target), Is.Null);
        }

        [Test]
        public void IconGuidSurvivesMoveAndDeletionReturnsNull()
        {
            var texture = CreateIcon();
            var reference = HierarchyIconReference.FromAsset(texture);
            store.SetIcon(target, AssetDatabase.GetAssetPath(texture));
            Assert.That(store.GetOrCreateMetadata(target).CustomIconReference, Is.EqualTo(reference));
            Assert.That(AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(texture), folder + "/Renamed.asset"), Is.Empty);
            Assert.That(HierarchyIconReference.Resolve(reference), Is.EqualTo(texture));
            Assert.That(AssetDatabase.DeleteAsset(folder + "/Renamed.asset"), Is.True);
            Assert.That(HierarchyIconReference.Resolve(reference), Is.Null);
            Assert.That(store.GetOrCreateMetadata(target).HasIconOverride, Is.True);
        }

        [Test]
        public void SpriteSubassetUsesLocalIdAndResolvesCorrectSprite()
        {
            var texture = CreateIcon();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero);
            sprite.name = "Subasset";
            AssetDatabase.AddObjectToAsset(sprite, texture);
            AssetDatabase.SaveAssetIfDirty(texture);
            var reference = HierarchyIconReference.FromAsset(sprite);
            Assert.That(reference, Is.Not.EqualTo(HierarchyIconReference.FromAsset(texture)));
            Assert.That(HierarchyIconReference.Resolve(reference), Is.EqualTo(sprite));
        }

        [Test]
        public void CustomSpriteLibraryPreventsDuplicatesAndMissingEntryCanBeRemoved()
        {
            var texture = CreateIcon();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.zero);
            sprite.name = "Library Sprite";
            AssetDatabase.AddObjectToAsset(sprite, texture);
            AssetDatabase.SaveAssetIfDirty(texture);
            var reference = HierarchyIconReference.FromAsset(sprite);
            var settings = QuickStylePaletteSettings.instance;
            settings.RemoveCustomIcon(reference);
            try
            {
                Assert.That(settings.AddCustomIcon(sprite), Is.True);
                Assert.That(settings.AddCustomIcon(sprite), Is.False);
                Assert.That(QuickStylePaletteSettings.MatchingReferenceIndex(settings.CustomIconLibrary, reference),
                    Is.GreaterThanOrEqualTo(0));
                Assert.That(AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(texture)), Is.True);
                Assert.That(HierarchyIconReference.Resolve(reference), Is.Null);
                Assert.That(settings.RemoveCustomIcon(reference), Is.True);
                Assert.That(settings.RemoveCustomIcon(reference), Is.False);
            }
            finally
            {
                settings.RemoveCustomIcon(reference);
            }
        }

        [Test]
        public void LegacyIconMetadataMigratesAndPersistsWithoutChangingColor()
        {
            var texture = CreateIcon();
            store.SetColor(target, Color.red);
            store.SetIcon(target, "GameObject Icon");
            var legacy = System.IO.File.ReadAllText(storePath).Replace("customIconReference:", "customIconPath:")
                .Replace("builtin:GameObject Icon", AssetDatabase.GetAssetPath(texture));
            Undo.ClearUndo(store);
            Resources.UnloadAsset(store);
            System.IO.File.WriteAllText(storePath, legacy);
            AssetDatabase.ImportAsset(storePath, ImportAssetOptions.ForceUpdate);
            store = AssetDatabase.LoadAssetAtPath<SceneMetadataStore>(storePath);
            store.MigrateIconReferences();
            Assert.That(store.GetOrCreateMetadata(target).CustomIconReference, Is.EqualTo(HierarchyIconReference.FromAsset(texture)));
            ReloadStore();
            Assert.That(store.GetOrCreateMetadata(target).CustomColor, Is.EqualTo(Color.red));
            Assert.That(ManualIconOperations.InitialIcon(target), Is.EqualTo(texture));
        }

        [UnityTest]
        public IEnumerator IconCacheInvalidatesOnProjectChangeAndMissingAsset()
        {
            var texture = CreateIcon();
            using var cache = new ManualIconCache();
            ManualIconOperations.Apply(new[] { target }, texture);
            cache.Rebuild();
            Assert.That(cache.TryGetIcon(target.GetEntityId(), out var found), Is.True);
            Assert.That(found, Is.EqualTo(texture));
            var notified = false;
            cache.Changed += () => notified = true;
            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(texture), folder + "/Moved.asset");
            for (var i = 0; i < 30 && !notified; i++) yield return null;
            Assert.That(notified, Is.True, "Project change must invalidate the icon snapshot.");
            cache.Rebuild();
            Assert.That(cache.TryGetIcon(target.GetEntityId(), out _), Is.True);
            AssetDatabase.DeleteAsset(folder + "/Moved.asset");
            cache.Rebuild();
            Assert.That(cache.TryGetIcon(target.GetEntityId(), out _), Is.False);
        }

        [Test]
        public void IconCacheAndNativeSlotRestoreOnClearUndoAndRecycle()
        {
            var texture = CreateIcon();
            using var cache = new ManualIconCache();
            using var binding = new ManualIconHierarchyBinding(cache);
            var element = new VisualElement();
            var original = element.style.backgroundImage;
            binding.BindIcon(element, target.GetEntityId());
            ManualIconOperations.Apply(new[] { target }, texture);
            cache.Rebuild();
            Assert.That(element.style.backgroundImage.value.texture, Is.EqualTo(texture));
            ManualIconOperations.Apply(new[] { target }, null);
            Assert.That(cache.TryGetIcon(target.GetEntityId(), out _), Is.False);
            cache.Rebuild();
            Assert.That(element.style.backgroundImage, Is.EqualTo(original));
            Undo.PerformUndo();
            cache.Rebuild();
            Assert.That(element.style.backgroundImage.value.texture, Is.EqualTo(texture));
            binding.UnbindIcon(element);
            Assert.That(element.style.backgroundImage, Is.EqualTo(original));
            binding.BindIcon(element, default);
            cache.Rebuild();
            Assert.That(element.style.backgroundImage, Is.EqualTo(original));
        }

        [Test]
        public void IconMultiSceneSelectionSkipsUnsupportedAndUndoesTogether()
        {
            var texture = CreateIcon();
            AssetDatabase.CopyAsset(scenePath, folder + "/IconOther.unity");
            var otherScene = EditorSceneManager.OpenScene(folder + "/IconOther.unity", OpenSceneMode.Additive);
            var otherStorePath = SceneMetadataStore.GetStorePath(otherScene);
            SceneMetadataStore otherStore = null;
            try
            {
                var other = otherScene.GetRootGameObjects()[0];
                var prefab = PrefabUtility.SaveAsPrefabAsset(target, folder + "/IconPrefab.prefab");
                var targets = ManualColorOperations.ResolveTargets(target, new[] { target, other, target, prefab, null });
                Assert.That(targets, Is.EqualTo(new[] { target, other }));
                Assert.That(ManualColorOperations.ResolveTargets(target, new[] { other }), Is.EqualTo(new[] { target }));
                ManualIconOperations.Apply(targets, texture);
                otherStore = SceneMetadataStore.Find(otherScene);
                Assert.That(ManualIconOperations.InitialIcon(other), Is.EqualTo(texture));
                Undo.PerformUndo();
                Assert.That(store.Count + otherStore.Count, Is.Zero);
                Undo.PerformRedo();
                Assert.That(store.Count + otherStore.Count, Is.EqualTo(2));
                ManualIconOperations.Apply(new[] { target, other, prefab, null }, null);
                Assert.That(store.Count + otherStore.Count, Is.Zero);
                Undo.PerformUndo();
                Assert.That(store.Count + otherStore.Count, Is.EqualTo(2));
            }
            finally
            {
                if (otherStore != null) Undo.ClearUndo(otherStore);
                EditorSceneManager.CloseScene(otherScene, true);
                AssetDatabase.DeleteAsset(otherStorePath);
            }
        }
    }
}


