using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ComponentPopupInspector : EditorWindow
    {
        private static ComponentPopupInspector current;
        [NonSerialized] private Component component;
        [NonSerialized] private UnityEditor.Editor inspector;
        private VisualElement body;
        internal static ComponentPopupInspector Current => current;
        internal UnityEditor.Editor Inspector => inspector;

        internal static bool IsSupported(Component value)
            => value != null && ComponentMinimapCache.IsSupported(value.gameObject)
                && !EditorUtility.IsPersistent(value)
                && (value.hideFlags & HideFlags.NotEditable) == 0
                && (value.gameObject.hideFlags & HideFlags.NotEditable) == 0;

        internal static void Open(Component value, Rect screenAnchor)
        {
            if (!IsSupported(value)) return;
            CloseCurrent();
            var window = CreateInstance<ComponentPopupInspector>();
            current = window;
            window.component = value;
            try
            {
                window.inspector = UnityEditor.Editor.CreateEditor(value);
                if (window.inspector == null) { window.Close(); return; }
                window.titleContent = new GUIContent(value.GetType().Name);
                window.BuildInspector();
                // Keep editing sessions alive when a property opens a color/object picker.
                window.ShowPopup();
                window.position = FitToEditor(new Rect(screenAnchor.x, screenAnchor.yMax, 360f, 320f));
            }
            catch
            {
                window.Close();
                throw;
            }
        }

        internal static void CloseCurrent()
        {
            if (current != null) current.Close();
        }

        private void OnEnable()
        {
            // Temporary editor state is deliberately not restored after reload.
            hideFlags = HideFlags.DontSave;
            EditorApplication.delayCall += ValidateTarget;
            EditorApplication.hierarchyChanged += ValidateTarget;
            ObjectChangeEvents.changesPublished += ObjectsChanged;
            Undo.undoRedoPerformed += ValidateTarget;
            EditorSceneManager.sceneClosed += SceneClosed;
            AssemblyReloadEvents.beforeAssemblyReload += Close;
            EditorApplication.quitting += Close;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        private void BuildInspector()
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            var label = new Label(component.GetType().Name);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.flexGrow = 1;
            header.Add(label);
            header.Add(new Button(Close) { text = "Close" });
            rootVisualElement.Add(header);
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            rootVisualElement.Add(scroll);
            // InspectorElement supports both UI Toolkit and IMGUI custom inspectors.
            // No component header: no enable toggle, add/remove menu, or other Inspector controls.
            body = new InspectorElement(inspector);
            scroll.Add(body);
            body.RegisterCallback<GeometryChangedEvent>(ResizeToInspector);
        }

        private void ResizeToInspector(GeometryChangedEvent evt)
        {
            if (!float.IsFinite(evt.newRect.height) || evt.newRect.height <= 0f) return;
            var rect = position;
            rect.height = Mathf.Clamp(evt.newRect.height + 36f, 100f, 480f);
            rect.width = 360f;
            rect = FitToEditor(rect);
            if (position != rect) position = rect;
        }

        private static Rect FitToEditor(Rect rect)
        {
            var area = EditorGUIUtility.GetMainWindowPosition();
            rect.height = Mathf.Min(rect.height, area.height);
            rect.width = Mathf.Min(rect.width, area.width);
            rect.x = Mathf.Clamp(rect.x, area.xMin, area.xMax - rect.width);
            rect.y = Mathf.Clamp(rect.y, area.yMin, area.yMax - rect.height);
            return rect;
        }

        internal void ValidateTarget()
        {
            if (!IsSupported(component) || inspector == null || inspector.target != component)
                Close();
            else Repaint();
        }

        private void ObjectsChanged(ref ObjectChangeEventStream stream) => ValidateTarget();
        private void SceneClosed(Scene scene) => ValidateTarget();
        private void PlayModeChanged(PlayModeStateChange state) => Close();

        private void OnDisable()
        {
            EditorApplication.delayCall -= ValidateTarget;
            EditorApplication.hierarchyChanged -= ValidateTarget;
            ObjectChangeEvents.changesPublished -= ObjectsChanged;
            Undo.undoRedoPerformed -= ValidateTarget;
            EditorSceneManager.sceneClosed -= SceneClosed;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
            EditorApplication.quitting -= Close;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            if (body != null) body.UnregisterCallback<GeometryChangedEvent>(ResizeToInspector);
            rootVisualElement.Unbind();
            rootVisualElement.Clear();
            body = null;
            if (inspector != null) DestroyImmediate(inspector);
            inspector = null;
            component = null;
            if (current == this) current = null;
        }
    }
}
