using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class QuickStylePalette : PopupWindowContent
    {
        private const float Padding = 6f;
        private const float DragHeight = 14f;
        private const float Cell = 28f;
        private const int IconColumns = 10;
        private static readonly int DragControlHash = "HierarchyToolkitQuickStylePaletteDrag".GetHashCode();
        private static readonly GUIContent CloseContent = new("×", "Close the palette without changing styling");

        private static QuickStylePalette current;
        private static GameObject queuedTarget;
        private static Rect queuedAnchor;
        private readonly GameObject target;
        private readonly GameObject[] targets;
        private readonly Rect anchor;
        private readonly List<ResolvedIcon> icons = new();
        private GUIStyle iconButton;
        private bool dragging;
        private Vector2 dragOffset;

        private QuickStylePalette(GameObject target, Rect anchor)
        {
            this.target = target;
            this.anchor = anchor;
            targets = ManualColorOperations.ResolveTargets(target, Selection.gameObjects);
            var presets = QuickStylePaletteSettings.instance.IconPresets;
            for (var i = 0; i < presets.Count; ++i)
            {
                var asset = HierarchyIconReference.Resolve(presets[i]);
                var texture = asset is Sprite sprite ? sprite.texture : asset as Texture;
                if (texture != null) icons.Add(new ResolvedIcon(presets[i], texture));
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
            var colorCells = QuickStylePaletteSettings.instance.ColorPresets.Count + 4;
            var width = Mathf.Max(IconColumns, colorCells) * Cell + Padding * 2f;
            var iconRows = Mathf.CeilToInt((icons.Count + 3) / (float)IconColumns);
            return new Vector2(width, Padding * 2f + DragHeight + Cell * (1 + iconRows));
        }

        public override void OnOpen() => editorWindow.wantsMouseMove = true;
        public override void OnClose() { if (ReferenceEquals(current, this)) current = null; }

        public override void OnGUI(Rect rect)
        {
            EnsureStyle();
            if (DrawHeader()) return;
            DrawColors();
            DrawIcons();
        }

        private bool DrawHeader()
        {
            var closeRect = new Rect(editorWindow.position.width - Padding - DragHeight, 0f, DragHeight, DragHeight);
            var dragRect = new Rect(Padding, 0f, closeRect.x - Padding, DragHeight);
            GUI.Label(dragRect, new GUIContent("•••", "Drag the palette"), EditorStyles.centeredGreyMiniLabel);
            var currentEvent = Event.current;
            var controlId = GUIUtility.GetControlID(DragControlHash, FocusType.Passive, dragRect);
            switch (currentEvent.GetTypeForControl(controlId))
            {
                case EventType.MouseDown when currentEvent.button == 0 && dragRect.Contains(currentEvent.mousePosition):
                    dragging = true;
                    GUIUtility.hotControl = controlId;
                    dragOffset = GUIUtility.GUIToScreenPoint(currentEvent.mousePosition) - editorWindow.position.position;
                    currentEvent.Use();
                    break;
                case EventType.MouseDrag when dragging && GUIUtility.hotControl == controlId:
                    var position = editorWindow.position;
                    position.position = GUIUtility.GUIToScreenPoint(currentEvent.mousePosition) - dragOffset;
                    editorWindow.position = position;
                    currentEvent.Use();
                    break;
                case EventType.MouseUp when dragging && GUIUtility.hotControl == controlId:
                    dragging = false;
                    GUIUtility.hotControl = 0;
                    currentEvent.Use();
                    break;
            }

            if (!GUI.Button(closeRect, CloseContent, EditorStyles.miniButton)) return false;
            Close();
            return true;
        }

        private void DrawColors()
        {
            var x = Padding;
            var y = Padding + DragHeight;
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
            if (GUI.Button(CellRect(x, y), new GUIContent("+", "Choose and apply a one-off custom color"), iconButton))
                OpenCustomColorPicker();
            x += Cell;
            GUI.enabled = ManualColorOperations.TryGetColor(target, out var currentColor);
            if (GUI.Button(CellRect(x, y), new GUIContent("☆", "Add or remove the current color from favorites"), iconButton))
            {
                QuickStylePaletteSettings.instance.ToggleColor(currentColor);
                Close();
            }
            GUI.enabled = true;
            x += Cell;
            if (GUI.Button(CellRect(x, y), new GUIContent("⚙", "Edit and reorder color favorites"), iconButton)) OpenPresetEditor();
        }

        private void DrawIcons()
        {
            var index = 0;
            DrawIconButton(index++, new GUIContent("×", "Clear manual icon"), () => ManualIconOperations.Apply(targets, null));
            for (var i = 0; i < icons.Count; ++i)
            {
                var icon = icons[i];
                DrawIconButton(index++, new GUIContent(icon.Texture, icon.Tooltip),
                    () => ManualIconOperations.ApplyReference(targets, icon.Reference));
            }
            DrawIconButton(index++, new GUIContent("+", "Open the Icon Library"), OpenIconLibrary);
            var currentReference = ManualIconOperations.InitialReference(target);
            GUI.enabled = !string.IsNullOrEmpty(currentReference);
            DrawIconButton(index, new GUIContent("☆", "Add or remove the current icon from favorites"), () =>
            {
                QuickStylePaletteSettings.instance.ToggleIcon(currentReference);
            });
            GUI.enabled = true;
        }

        private void DrawIconButton(int index, GUIContent content, System.Action action)
        {
            var column = index % IconColumns;
            var row = index / IconColumns;
            if (!GUI.Button(CellRect(Padding + column * Cell, Padding + DragHeight + Cell + row * Cell), content, iconButton)) return;
            action();
            Close();
        }

        private void ApplyColor(Color? color, bool recursive)
        {
            var recipients = recursive ? ManualColorOperations.ResolveRecursiveTargets(target) : targets;
            ManualColorOperations.Apply(recipients, color);
            Close();
        }

        private void OpenIconLibrary()
        {
            EditorApplication.delayCall += () => IconLibraryWindow.Open(targets);
        }

        private void OpenCustomColorPicker()
        {
            var initial = ManualColorOperations.InitialColor(target);
            Close();
            EditorApplication.delayCall += () => ManualColorPicker.Open(targets, initial);
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

        private readonly struct ResolvedIcon
        {
            internal readonly string Reference;
            internal readonly Texture Texture;
            internal string Tooltip => Reference.StartsWith("builtin:", System.StringComparison.Ordinal)
                ? Reference.Substring(8) : "Project icon favorite";
            internal ResolvedIcon(string reference, Texture texture) { Reference = reference; Texture = texture; }
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
