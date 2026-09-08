using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class IconLibraryWindow : EditorWindow
    {
        private const float ToolbarHeight = 24f;
        private const float CellSize = 56f;
        private const float GridPadding = 6f;
        private static readonly int ObjectPickerId = "HierarchyToolkitIconLibrarySpritePicker".GetHashCode();
        private static readonly GUIContent[] ScopeLabels =
        {
            new("All"), new("Unity"), new("Project")
        };

        private readonly List<IconEntry> allEntries = new();
        private readonly List<IconEntry> filteredEntries = new();
        private GameObject[] targets;
        private Vector2 scroll;
        private string search = string.Empty;
        private int scope;
        private GUIStyle iconButton;

        internal static void Open(GameObject[] targets)
        {
            BuiltInIconProvider.EnsureLoaded();
            var window = CreateInstance<IconLibraryWindow>();
            window.targets = targets;
            window.titleContent = new GUIContent("Icon Library");
            window.minSize = new Vector2(420f, 300f);
            window.RebuildEntries();
            window.ShowUtility();
        }

        private void OnEnable() => QuickStylePaletteSettings.instance.Changed += SettingsChanged;
        private void OnDisable() => QuickStylePaletteSettings.instance.Changed -= SettingsChanged;

        private void OnGUI()
        {
            EnsureStyle();
            DrawToolbar();
            HandleObjectPicker();
            DrawGrid();
        }

        private void DrawToolbar()
        {
            var searchRect = new Rect(6f, 3f, Mathf.Max(100f, position.width - 238f), 19f);
            var nextSearch = GUI.TextField(searchRect, search, EditorStyles.toolbarSearchField);
            var scopeRect = new Rect(position.width - 126f, 2f, 90f, 20f);
            var nextScope = GUI.Toolbar(scopeRect, scope, ScopeLabels, EditorStyles.toolbarButton);
            if (nextSearch != search || nextScope != scope)
            {
                search = nextSearch;
                scope = nextScope;
                scroll = Vector2.zero;
                FilterEntries();
            }

            if (GUI.Button(new Rect(position.width - 32f, 2f, 26f, 20f),
                new GUIContent("+", "Add a project Sprite to the reusable icon library"), EditorStyles.toolbarButton))
                EditorGUIUtility.ShowObjectPicker<Sprite>(null, false, string.Empty, ObjectPickerId);
        }

        private void HandleObjectPicker()
        {
            var currentEvent = Event.current;
            if (currentEvent.commandName != "ObjectSelectorClosed"
                || EditorGUIUtility.GetObjectPickerControlID() != ObjectPickerId) return;
            if (EditorGUIUtility.GetObjectPickerObject() is Sprite sprite)
                QuickStylePaletteSettings.instance.AddCustomIcon(sprite);
            currentEvent.Use();
        }

        private void DrawGrid()
        {
            var viewport = new Rect(0f, ToolbarHeight, position.width, position.height - ToolbarHeight);
            if (filteredEntries.Count == 0)
            {
                var message = string.IsNullOrEmpty(BuiltInIconProvider.FailureMessage)
                    ? "No icons match the current search." : BuiltInIconProvider.FailureMessage;
                GUI.Label(new Rect(8f, ToolbarHeight + 8f, position.width - 16f, 40f), message, EditorStyles.wordWrappedLabel);
                return;
            }

            var columns = Mathf.Max(1, Mathf.FloorToInt((viewport.width - GridPadding * 2f - 16f) / CellSize));
            var rows = Mathf.CeilToInt(filteredEntries.Count / (float)columns);
            var contentHeight = rows * CellSize + GridPadding * 2f;
            var contentRect = new Rect(0f, 0f, viewport.width - 16f, contentHeight);
            scroll = GUI.BeginScrollView(viewport, scroll, contentRect);

            var firstRow = Mathf.Max(0, Mathf.FloorToInt(scroll.y / CellSize));
            var lastRow = Mathf.Min(rows - 1, Mathf.CeilToInt((scroll.y + viewport.height) / CellSize));
            var first = firstRow * columns;
            var last = Mathf.Min(filteredEntries.Count, (lastRow + 1) * columns);
            for (var index = first; index < last; ++index)
            {
                var entry = filteredEntries[index];
                var column = index % columns;
                var row = index / columns;
                var cell = new Rect(GridPadding + column * CellSize, GridPadding + row * CellSize, CellSize - 2f, CellSize - 2f);
                DrawEntry(cell, entry);
            }
            GUI.EndScrollView();
        }

        private void DrawEntry(Rect cell, IconEntry entry)
        {
            var tooltip = entry.IsCustom ? "Project Sprite: " + entry.Name : "Unity icon: " + entry.Name;
            if (entry.Texture != null)
            {
                if (GUI.Button(cell, new GUIContent(entry.Texture, tooltip), iconButton))
                    ManualIconOperations.ApplyReference(targets, entry.Reference);
            }
            else
            {
                GUI.Box(cell, new GUIContent("?", "Missing project Sprite"), iconButton);
            }

            if (!entry.IsCustom) return;
            var removeRect = new Rect(cell.xMax - 17f, cell.y + 2f, 15f, 15f);
            if (GUI.Button(removeRect, new GUIContent("×", "Remove this project Sprite from the library"), EditorStyles.miniButton))
                QuickStylePaletteSettings.instance.RemoveCustomIcon(entry.Reference);
        }

        private void SettingsChanged()
        {
            RebuildEntries();
            Repaint();
        }

        private void RebuildEntries()
        {
            allEntries.Clear();
            var custom = QuickStylePaletteSettings.instance.CustomIconLibrary;
            for (var i = 0; i < custom.Count; ++i)
            {
                var asset = HierarchyIconReference.Resolve(custom[i]);
                var sprite = asset as Sprite;
                var name = sprite != null ? sprite.name : "Missing project Sprite";
                allEntries.Add(new IconEntry(name, custom[i], sprite != null ? sprite.texture : null, true));
            }

            var builtIns = BuiltInIconProvider.Icons;
            for (var i = 0; i < builtIns.Count; ++i)
                allEntries.Add(new IconEntry(builtIns[i].Name, builtIns[i].Reference, builtIns[i].Texture, false));
            FilterEntries();
        }

        private void FilterEntries()
        {
            filteredEntries.Clear();
            for (var i = 0; i < allEntries.Count; ++i)
            {
                var entry = allEntries[i];
                if ((scope == 1 && entry.IsCustom) || (scope == 2 && !entry.IsCustom)) continue;
                if (!string.IsNullOrEmpty(search)
                    && entry.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                filteredEntries.Add(entry);
            }
        }

        private void EnsureStyle()
        {
            if (iconButton != null) return;
            iconButton = new GUIStyle(GUI.skin.button)
            {
                imagePosition = ImagePosition.ImageOnly,
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset()
            };
        }

        private readonly struct IconEntry
        {
            internal readonly string Name;
            internal readonly string Reference;
            internal readonly Texture Texture;
            internal readonly bool IsCustom;

            internal IconEntry(string name, string reference, Texture texture, bool isCustom)
            {
                Name = name;
                Reference = reference;
                Texture = texture;
                IsCustom = isCustom;
            }
        }
    }
}
