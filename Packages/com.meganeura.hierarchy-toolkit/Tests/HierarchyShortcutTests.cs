using System;
using System.Collections;
using NUnit.Framework;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class HierarchyShortcutTests
    {
        private Scene scene;
        private string folder;
        private Scene previousScene;
        private Object[] previousSelection;
        private GameObject targetParent;
        private GameObject target;
        private GameObject unrelated;

        [SetUp]
        public void SetUp()
        {
            previousScene = SceneManager.GetActiveScene();
            previousSelection = Selection.objects;
            var name = "HierarchyShortcutTest_" + Guid.NewGuid().ToString("N");
            const string root = "Packages/com.meganeura.hierarchy-toolkit/Tests";
            AssetDatabase.CreateFolder(root, name);
            folder = root + "/" + name;
            var path = folder + "/Scene.unity";
            System.IO.File.WriteAllText(path,
                "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!29 &1\nOcclusionCullingSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n");
            AssetDatabase.ImportAsset(path);
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            targetParent = new GameObject("Target Parent");
            target = new GameObject("Target");
            unrelated = new GameObject("Unrelated");
            SceneManager.MoveGameObjectToScene(targetParent, scene);
            SceneManager.MoveGameObjectToScene(target, scene);
            SceneManager.MoveGameObjectToScene(unrelated, scene);
            target.transform.SetParent(targetParent.transform);
            EditorSceneManager.SetActiveScene(scene);
            EditorSceneManager.SaveScene(scene);
        }

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            Selection.objects = previousSelection;
            if (folder != null) AssetDatabase.DeleteAsset(folder);
        }

        [UnityTest]
        public IEnumerator ToggleChangesOnlyHoveredNodeExpansionState()
        {
            var window = ScriptableObject.CreateInstance<HierarchyWindow>();
            window.Show();
            try
            {
                yield return null;
                var view = window.View;
                var handler = view.Source.GetOrCreateNodeTypeHandler<HierarchyGameObjectHandler>();
                var node = handler.GetOrCreateNode(targetParent);
                view.Collapse(node);
                view.Update();
                var selectionBefore = Selection.objects;

                Assert.That(HierarchyNavigation.ToggleExpanded(view, node), Is.True);
                Assert.That(view.IsExpanded(node), Is.True);
                Assert.That(HierarchyNavigation.ToggleExpanded(view, node), Is.True);
                Assert.That(view.IsExpanded(node), Is.False);
                Assert.That(scene.isDirty, Is.False);
                Assert.That(Selection.objects, Is.EqualTo(selectionBefore));
            }
            finally { window.Close(); }
        }

        [UnityTest]
        public IEnumerator HoverTargetSurvivesRowRebindingAcrossRepeatedToggles()
        {
            var window = ScriptableObject.CreateInstance<HierarchyWindow>();
            window.Show();
            var shortcuts = new HierarchyShortcuts();
            try
            {
                yield return null;
                var view = window.View;
                var handler = view.Source.GetOrCreateNodeTypeHandler<HierarchyGameObjectHandler>();
                var node = handler.GetOrCreateNode(targetParent);
                view.Collapse(node);
                view.Update();
                yield return null;

                HierarchyViewItem targetItem = null;
                window.rootVisualElement.Query<HierarchyViewItem>().ForEach(item =>
                {
                    if (item.Node == node) targetItem = item;
                });
                Assert.That(targetItem, Is.Not.Null);

                shortcuts.SetHovered(window, targetItem.worldBound.center);
                for (var press = 0; press < 3; press++)
                {
                    Assert.That(shortcuts.TryGetHoveredTarget(out var hovered, out _, out _), Is.True);
                    Assert.That(hovered, Is.EqualTo(targetParent));
                    Assert.That(shortcuts.TryToggleHovered(), Is.True);
                    Assert.That(view.IsExpanded(node), Is.EqualTo(press % 2 == 0));
                }
            }
            finally
            {
                shortcuts.Dispose();
                window.Close();
            }
        }

        [UnityTest]
        public IEnumerator IsolateCollapsesUnrelatedBranchesAndKeepsTargetBranchVisible()
        {
            var unrelatedChild = new GameObject("Unrelated Child");
            SceneManager.MoveGameObjectToScene(unrelatedChild, scene);
            unrelatedChild.transform.SetParent(unrelated.transform);
            EditorSceneManager.SaveScene(scene);
            var window = ScriptableObject.CreateInstance<HierarchyWindow>();
            window.Show();
            try
            {
                yield return null;
                var view = window.View;
                var handler = view.Source.GetOrCreateNodeTypeHandler<HierarchyGameObjectHandler>();
                var parentNode = handler.GetOrCreateNode(targetParent);
                var targetNode = handler.GetOrCreateNode(target);
                var unrelatedNode = handler.GetOrCreateNode(unrelated);
                view.Expand(parentNode);
                view.Expand(unrelatedNode);
                view.Update();
                var selectionBefore = Selection.objects;

                Assert.That(HierarchyNavigation.Isolate(view, targetNode), Is.True);
                Assert.That(view.IsExpanded(parentNode), Is.True);
                Assert.That(view.IsExpanded(targetNode), Is.True);
                Assert.That(view.IsExpanded(unrelatedNode), Is.False);
                Assert.That(view.ViewModel.IndexOf(targetNode), Is.GreaterThanOrEqualTo(0));
                Assert.That(scene.isDirty, Is.False);
                Assert.That(Selection.objects, Is.EqualTo(selectionBefore));
                Assert.That(target.activeSelf, Is.True);
                Assert.That(unrelated.activeSelf, Is.True);
                Assert.That(target.transform.parent.gameObject, Is.EqualTo(targetParent));
            }
            finally { window.Close(); }
        }

        [Test]
        public void InvalidTargetsFailSafely()
        {
            Assert.That(HierarchyNavigation.ToggleExpanded(null, default), Is.False);
            Assert.That(HierarchyNavigation.Isolate(null, default), Is.False);
        }

        [Test]
        public void TextEditingElementsAreRecognizedByShortcutGuard()
        {
            var field = new TextField();
            var child = new VisualElement();
            field.Add(child);
            Assert.That(HierarchyShortcuts.IsTextEditingElement(field), Is.True);
            Assert.That(HierarchyShortcuts.IsTextEditingElement(child), Is.True);
            Assert.That(HierarchyShortcuts.IsTextEditingElement(new VisualElement()), Is.False);
        }
    }
}
