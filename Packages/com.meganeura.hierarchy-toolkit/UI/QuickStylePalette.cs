using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class QuickStylePalette : PopupWindowContent
    {
        private const float Padding = 6f;
        private const float Cell = 28f;
        private const int IconColumns = 10;
        private static readonly IconPreset[] IconPresets =
        {
            new("Folder Icon", "Folder"), new("Camera Icon", "Camera"), new("Light Icon", "Light"),
            new("AudioSource Icon", "Audio"), new("Canvas Icon", "Canvas"), new("Prefab Icon", "Prefab"),
            new("d_UnityEditor.ConsoleWindow", "Console"), new("d_SceneViewTools", "Tools"),
            new("Favorite", "Favorite"), new("Settings", "Settings")
        };

        private static QuickStylePalette current;
        private static GameObject queuedTarget;
        private static Rect queuedAnchor;
        private readonly GameObject target;
        private readonly GameObject[] targets;
        private readonly Rect anchor;
        private readonly List<ResolvedIcon> icons = new();
        private GUIStyle iconButton;

        private QuickStylePalette(GameObject target, Rect anchor)
        {
            this.target = target;
            this.anchor = anchor;
            targets = ManualColorOperations.ResolveTargets(target, Selection.gameObjects);
            for (var i = 0; i < IconPresets.Length; ++i)
            {
                var texture = EditorGUIUtility.FindTexture(IconPresets[i].Name);
                if (texture != null) icons.Add(new ResolvedIcon(IconPresets[i], texture));
            }
        }

        internal static void QueueOpen(GameObject target, Rect anchor)
        {
            queuedTarget = target;
            queuedAnchor = anchor;
            EditorApplication.delayCall -= OpenQueued;
            EditorApplication.delayCall += OpenQueued;
        }

        internal static void CancelQueuedOpen()
        {
            EditorApplication.delayCall -= OpenQueued;
            queuedTarget = null;
        }

        private static void OpenQueued()
        {
            EditorApplication.delayCall -= OpenQueued;
            var target = queuedTarget;
            var anchor = queuedAnchor;
            queuedTarget = null;
            if (!ManualColorOperations.IsSupported(target)) return;
            current?.Close();
            current = new QuickStylePalette(target, anchor);
            PopupWindow.Show(anchor, current);
        }

        public override Vector2 GetWindowSize()
        {
            var colorCells = QuickStylePaletteSettings.instance.ColorPresets.Count + 2;
            var width = Mathf.Max(IconColumns, colorCells) * Cell + Padding * 2f;
            var iconRows = Mathf.CeilToInt((icons.Count + 2) / (float)IconColumns);
            return new Vector2(width, Padding * 2f + Cell * (1 + iconRows));
        }

        public override void OnOpen() => editorWindow.wantsMouseMove = true;
        public override void OnClose() { if (ReferenceEquals(current, this)) current = null; }

        public override void OnGUI(Rect rect)
        {
            EnsureStyle();
            DrawColors();
            DrawIcons();
        }

        private void DrawColors()
        {
            var x = Padding;
            var y = Padding;
            if (GUI.Button(CellRect(x, y), new GUIContent("×", "Clear manual color. Hold Alt to include descendants."), iconButton))
                ApplyColor(null, Event.current.alt);
            x += Cell;
            var presets = QuickStylePaletteSettings.instance.ColorPresets;
            for (var i = 0; i < presets.Count; ++i)
            {
                var swatch = new Rect(x + 4f, y + 4f, Cell - 8f, Cell - 8f);
                EditorGUI.DrawRect(swatch, presets[i]);
                var recursive = Event.current.alt;
                if (GUI.Button(CellRect(x, y), new GUIContent(string.Empty, "Apply color. Hold Alt to include descendants."), GUIStyle.none))
                    ApplyColor(presets[i], recursive);
                x += Cell;
            }
            if (GUI.Button(CellRect(x, y), new GUIContent("+", "Edit color presets"), iconButton)) OpenPresetEditor();
        }

        private void DrawIcons()
        {
            var index = 0;
            DrawIconButton(index++, new GUIContent("×", "Clear manual icon"), () => ManualIconOperations.Apply(targets, null));
            for (var i = 0; i < icons.Count; ++i)
            {
                var icon = icons[i];
                DrawIconButton(index++, new GUIContent(icon.Texture, icon.Preset.Tooltip),
                    () => ManualIconOperations.ApplyReference(targets, "builtin:" + icon.Preset.Name));
            }
            DrawIconButton(index, new GUIContent("+", "Choose a custom project icon"), OpenCustomIconPicker);
        }

        private void DrawIconButton(int index, GUIContent content, System.Action action)
        {
            var column = index % IconColumns;
            var row = index / IconColumns;
            if (!GUI.Button(CellRect(Padding + column * Cell, Padding + Cell + row * Cell), content, iconButton)) return;
            action();
            Close();
        }

        private void ApplyColor(Color? color, bool recursive)
        {
            var recipients = recursive ? ManualColorOperations.ResolveRecursiveTargets(target) : targets;
            ManualColorOperations.Apply(recipients, color);
            Close();
        }

        private void OpenCustomIconPicker()
        {
            var initial = ManualIconOperations.InitialIcon(target);
            EditorApplication.delayCall += () => ManualIconPicker.Open(targets, initial);
        }

        private void OpenPresetEditor()
        {
            var savedAnchor = anchor;
            Close();
            EditorApplication.delayCall += () => PopupWindow.Show(savedAnchor, new QuickStyleColorPresetPopup());
        }

        private void Close() => editorWindow?.Close();
        private static Rect CellRect(float x, float y) => new(x, y, Cell, Cell);

        private void EnsureStyle()
        {
            if (iconButton != null) return;
            iconButton = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                padding = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset()
            };
        }

        private readonly struct IconPreset
        {
            internal readonly string Name;
            internal readonly string Tooltip;
            internal IconPreset(string name, string tooltip) { Name = name; Tooltip = tooltip; }
        }

        private readonly struct ResolvedIcon
        {
            internal readonly IconPreset Preset;
            internal readonly Texture Texture;
            internal ResolvedIcon(IconPreset preset, Texture texture) { Preset = preset; Texture = texture; }
        }
    }

    internal sealed class QuickStyleColorPresetPopup : PopupWindowContent
    {
        private const float RowHeight = 24f;
        public override Vector2 GetWindowSize()
            => new(260f, 10f + (QuickStylePaletteSettings.instance.ColorPresets.Count + 1) * RowHeight);

        public override void OnGUI(Rect rect)
        {
            var settings = QuickStylePaletteSettings.instance;
            var colors = settings.ColorPresets;
            for (var i = 0; i < colors.Count; ++i)
            {
                var y = 5f + i * RowHeight;
                var next = EditorGUI.ColorField(new Rect(6f, y + 2f, 158f, 20f), GUIContent.none, colors[i], true, true, false);
                if (!next.Equals(colors[i])) settings.SetColor(i, next);
                GUI.enabled = i > 0;
                if (GUI.Button(new Rect(168f, y + 2f, 24f, 20f), "↑")) { settings.MoveColor(i, -1); return; }
                GUI.enabled = i + 1 < colors.Count;
                if (GUI.Button(new Rect(194f, y + 2f, 24f, 20f), "↓")) { settings.MoveColor(i, 1); return; }
                GUI.enabled = true;
                if (GUI.Button(new Rect(224f, y + 2f, 28f, 20f), "×")) { settings.RemoveColor(i); return; }
            }
            var addY = 5f + colors.Count * RowHeight;
            if (GUI.Button(new Rect(6f, addY + 2f, 246f, 20f), "+ Add color preset"))
                settings.AddColor(new Color(0.35f, 0.65f, 1f, 1f));
        }
    }
}
