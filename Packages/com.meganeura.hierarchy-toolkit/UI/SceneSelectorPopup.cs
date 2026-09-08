using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class SceneSelectorPopup : PopupWindowContent
    {
        private const float SearchHeight = 22f;
        private const float RowHeight = 20f;
        private const float MaximumHeight = 420f;
        private static readonly Texture SceneIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image;
        private readonly SceneCatalog catalog;
        private readonly SceneFavoriteStore favorites;
        private readonly string currentPath;
        private readonly List<SceneCatalog.Entry> visible = new();
        private readonly SearchField searchField = new();
        private string search = string.Empty;
        private Vector2 scroll;
        private int favoriteCount;
        private GUIStyle rowStyle;
        private GUIStyle currentStyle;
        private GUIStyle starStyle;

        internal SceneSelectorPopup(SceneCatalog catalog, SceneFavoriteStore favorites, string currentPath)
        {
            this.catalog = catalog;
            this.favorites = favorites;
            this.currentPath = currentPath;
            Rebuild();
        }

        public override Vector2 GetWindowSize()
        {
            var separator = favoriteCount > 0 && favoriteCount < visible.Count ? 7f : 0f;
            return new Vector2(310f, Mathf.Min(MaximumHeight, SearchHeight + 8f + visible.Count * RowHeight + separator));
        }

        public override void OnOpen()
        {
            catalog.Changed += DataChanged;
            favorites.Changed += DataChanged;
            editorWindow.wantsMouseMove = true;
        }

        public override void OnClose()
        {
            catalog.Changed -= DataChanged;
            favorites.Changed -= DataChanged;
        }

        public override void OnGUI(Rect rect)
        {
            EnsureStyles();
            var nextSearch = searchField.OnGUI(new Rect(5f, 4f, rect.width - 10f, 18f), search);
            if (nextSearch != search)
            {
                search = nextSearch;
                Rebuild();
            }

            var listRect = new Rect(0f, SearchHeight + 4f, rect.width, rect.height - SearchHeight - 4f);
            var separator = favoriteCount > 0 && favoriteCount < visible.Count ? 7f : 0f;
            var contentRect = new Rect(0f, 0f, rect.width - 16f, visible.Count * RowHeight + separator);
            scroll = GUI.BeginScrollView(listRect, scroll, contentRect, false, contentRect.height > listRect.height);
            var y = 0f;
            for (var i = 0; i < visible.Count; ++i)
            {
                if (i == favoriteCount && favoriteCount > 0)
                {
                    EditorGUI.DrawRect(new Rect(8f, y + 2f, contentRect.width - 16f, 1f), EditorGUIUtility.isProSkin
                        ? new Color(1f, 1f, 1f, .18f) : new Color(0f, 0f, 0f, .18f));
                    y += separator;
                }
                DrawRow(visible[i], new Rect(0f, y, contentRect.width, RowHeight));
                y += RowHeight;
            }
            if (visible.Count == 0) GUI.Label(new Rect(8f, 2f, contentRect.width - 16f, RowHeight), "No matching scenes", EditorStyles.centeredGreyMiniLabel);
            GUI.EndScrollView();
        }

        private void DrawRow(SceneCatalog.Entry entry, Rect row)
        {
            var starRect = new Rect(row.xMax - 27f, row.y, 24f, row.height);
            var sceneRect = new Rect(row.x, row.y, row.width - 28f, row.height);
            if (Event.current.type == EventType.Repaint && sceneRect.Contains(Event.current.mousePosition))
                EditorGUI.DrawRect(row, EditorGUIUtility.isProSkin ? new Color(.32f, .48f, .66f, .45f) : new Color(.35f, .55f, .75f, .35f));
            var content = new GUIContent(entry.DisplayName, SceneIcon, entry.Path);
            if (GUI.Button(sceneRect, content, entry.Path == currentPath ? currentStyle : rowStyle))
            {
                if (SceneSelectorNavigator.Open(entry)) editorWindow.Close();
                else catalog.Refresh();
            }
            var favorite = favorites.Contains(entry.Guid);
            if (GUI.Button(starRect, new GUIContent(favorite ? "★" : "☆", favorite ? "Remove from favorites" : "Add to favorites"), starStyle))
                favorites.Toggle(entry.Guid);
        }

        private void EnsureStyles()
        {
            if (rowStyle != null) return;
            rowStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(6, 2, 0, 0) };
            rowStyle.normal.textColor = EditorStyles.label.normal.textColor;
            currentStyle = new GUIStyle(rowStyle) { fontStyle = FontStyle.Bold };
            starStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        }

        private void DataChanged()
        {
            Rebuild();
            editorWindow?.Repaint();
        }

        private void Rebuild() => SceneSelectorModel.Build(catalog.Entries, favorites, search, visible, out favoriteCount);
    }
}
