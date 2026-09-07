using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class ComponentPopupInspectorTests
    {
        private Scene scene;
        private GameObject target;
        private Object[] selection;
        private string folder;

        [SetUp]
        public void SetUp()
        {
            selection = Selection.objects;
            scene = EditorSceneManager.NewPreviewScene();
            target = new GameObject("Popup Test");
            SceneManager.MoveGameObjectToScene(target, scene);
        }

        [TearDown]
        public void TearDown()
        {
            ComponentPopupInspector.CloseCurrent();
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponents<Component>())
                        if (component != null) Undo.ClearUndo(component);
                EditorSceneManager.ClosePreviewScene(scene);
            }
            Selection.objects = selection;
            if (folder != null) AssetDatabase.DeleteAsset(folder);
        }

        private static void Open(Component component)
            => ComponentPopupInspector.Open(component, new Rect(200, 200, 14, 14));

        [Test]
        public void MissingDestroyedAndNotEditableComponentsDoNotOpen()
        {
            Assert.DoesNotThrow(() => Open(null));
            var component = target.AddComponent<BoxCollider>();
            component.hideFlags = HideFlags.NotEditable;
            Open(component);
            Assert.That(ComponentPopupInspector.Current == null, Is.True);
            Object.DestroyImmediate(component);
            Assert.DoesNotThrow(() => Open(component));
            Assert.That(ComponentPopupInspector.Current == null, Is.True);
        }

        [Test]
        public void TypeIconResolvesFirstExactComponentOnItsOwnObject()
        {
            var first = target.AddComponent<BoxCollider>();
            target.AddComponent<BoxCollider>();
            Assert.That(ComponentMinimapControl.ResolveComponent(target, typeof(BoxCollider)), Is.SameAs(first));
            Assert.That(ComponentMinimapControl.ResolveComponent(target, typeof(Camera)), Is.Null);
            Assert.That(ComponentMinimapControl.ResolveComponent(null, typeof(BoxCollider)), Is.Null);
        }

        [Test]
        public void OpeningPreservesSelectionAndReplacementAndCloseDestroyEditors()
        {
            var first = target.AddComponent<BoxCollider>();
            var second = target.AddComponent<AudioSource>();
            Selection.objects = Array.Empty<Object>();
            Open(first);
            var oldEditor = ComponentPopupInspector.Current.Inspector;
            Assert.That(oldEditor.target, Is.SameAs(first));
            Assert.That(Selection.objects, Is.Empty);
            Open(second);
            Assert.That(oldEditor == null, Is.True);
            var editor = ComponentPopupInspector.Current.Inspector;
            Assert.That(editor.target, Is.SameAs(second));
            ComponentPopupInspector.CloseCurrent();
            Assert.That(editor == null, Is.True);
            Assert.That(ComponentPopupInspector.Current == null, Is.True);
        }

        [Test]
        public void SerializedInspectorEditSupportsUndoAndRedo()
        {
            var collider = target.AddComponent<BoxCollider>();
            Open(collider);
            var serialized = ComponentPopupInspector.Current.Inspector.serializedObject;
            Undo.IncrementCurrentGroup();
            serialized.Update();
            serialized.FindProperty("m_Center").vector3Value = Vector3.one;
            Assert.That(serialized.ApplyModifiedProperties(), Is.True);
            Undo.FlushUndoRecordObjects();
            Assert.That(collider.center, Is.EqualTo(Vector3.one));
            Undo.PerformUndo();
            Assert.That(collider.center, Is.EqualTo(Vector3.zero));
            Undo.PerformRedo();
            Assert.That(collider.center, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void MonoBehaviourUsesItsOwnSerializedInspector()
        {
            var component = target.AddComponent<ComponentPopupTestBehaviour>();
            Open(component);
            var editor = ComponentPopupInspector.Current.Inspector;
            Assert.That(editor.target, Is.SameAs(component));
            var serialized = editor.serializedObject;
            serialized.Update();
            serialized.FindProperty("Number").intValue = 42;
            serialized.ApplyModifiedProperties();
            Assert.That(component.Number, Is.EqualTo(42));
        }

        [UnityTest]
        public IEnumerator DestroyedComponentClosesThroughEditorEvents()
        {
            var component = target.AddComponent<BoxCollider>();
            Open(component);
            var editor = ComponentPopupInspector.Current.Inspector;
            Object.DestroyImmediate(component);
            yield return null;
            yield return null;
            Assert.That(ComponentPopupInspector.Current == null, Is.True);
            Assert.That(editor == null, Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator SceneCloseReleasesEditor()
        {
            Open(target.AddComponent<BoxCollider>());
            var editor = ComponentPopupInspector.Current.Inspector;
            EditorSceneManager.ClosePreviewScene(scene);
            yield return null;
            Assert.That(editor == null, Is.True);
            Assert.That(ComponentPopupInspector.Current == null, Is.True);
        }

        [Test]
        public void PrefabInstanceEditsAreOverridesAndAssetIsRejected()
        {
            var name = "PopupTest_" + Guid.NewGuid().ToString("N");
            const string root = "Packages/com.meganeura.hierarchy-toolkit/Tests";
            AssetDatabase.CreateFolder(root, name);
            folder = root + "/" + name;
            target.AddComponent<BoxCollider>();
            var asset = PrefabUtility.SaveAsPrefabAsset(target, folder + "/Target.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
            var collider = instance.GetComponent<BoxCollider>();
            Assert.That(ComponentPopupInspector.IsSupported(asset.GetComponent<BoxCollider>()), Is.False);
            Open(collider);
            var serialized = ComponentPopupInspector.Current.Inspector.serializedObject;
            serialized.Update();
            serialized.FindProperty("m_Center").vector3Value = Vector3.one;
            serialized.ApplyModifiedProperties();
            Undo.FlushUndoRecordObjects();
            Assert.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instance, false), Is.True);
            Assert.That(asset.GetComponent<BoxCollider>().center, Is.EqualTo(Vector3.zero));
            Undo.PerformUndo();
            Assert.That(collider.center, Is.EqualTo(Vector3.zero));
            Undo.PerformRedo();
            Assert.That(collider.center, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void NormalClickBubblesAndAltClickOpensWithoutBubbling()
        {
            var component = target.AddComponent<BoxCollider>();
            using var cache = new ComponentMinimapCache();
            var control = new ComponentMinimapControl();
            control.Bind(target);
            var layout = new HierarchyRowLayout(new Rect(0, 0, 300, 18), 100, 280);
            control.Refresh(cache.Collect(new Component[] { component }), ref layout);
            var host = ScriptableObject.CreateInstance<EditorWindow>();
            try
            {
                host.ShowUtility();
                host.rootVisualElement.Add(control);
                var bubbled = 0;
                control.RegisterCallback<PointerDownEvent>(_ => ++bubbled);
                using (var evt = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0 }))
                    control[0].SendEvent(evt);
                Assert.That(bubbled, Is.EqualTo(1));
                Assert.That(ComponentPopupInspector.Current == null, Is.True);
                using (var evt = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, modifiers = EventModifiers.Alt }))
                    control[0].SendEvent(evt);
                Assert.That(bubbled, Is.EqualTo(1));
                Assert.That(ComponentPopupInspector.Current.Inspector.target, Is.SameAs(component));
                control.Bind(null);
                Assert.That(ComponentPopupInspector.Current.Inspector.target, Is.SameAs(component), "Popup owns its editor independently of recycled rows.");
            }
            finally { host.Close(); }
        }
    }
}
