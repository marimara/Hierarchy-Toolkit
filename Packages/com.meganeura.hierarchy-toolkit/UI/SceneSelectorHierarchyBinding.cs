using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class SceneSelectorHierarchyBinding : IDisposable
    {
        private readonly SceneCatalog catalog;
        private readonly SceneFavoriteStore favorites;
        private readonly Dictionary<HierarchyViewItem, RowBinding> rows = new();

        internal SceneSelectorHierarchyBinding(SceneCatalog catalog, SceneFavoriteStore favorites)
        {
            this.catalog = catalog;
            this.favorites = favorites;
            HierarchyWindow.BindViewItem += BindItem;
            HierarchyWindow.UnbindViewItem += UnbindItem;
            HierarchyWindow.UnbindView += UnbindView;
            EditorApplication.delayCall += BindExisting;
        }

        private void BindExisting()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<HierarchyWindow>())
                window.rootVisualElement.Query<HierarchyViewItem>().ForEach(item => BindItem(window, item.View, item));
        }

        private void BindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            UnbindItem(window, view, item);
            if (item.Handler is not HierarchySceneHandler handler) return;
            var node = item.Node;
            var scene = handler.GetScene(node);
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path)) return;
            rows.Add(item, new RowBinding(window, item, scene, catalog, favorites));
        }

        private void UnbindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (rows.Remove(item, out var binding)) binding.Dispose();
        }

        private void UnbindView(HierarchyWindow window, HierarchyView view)
            => view.Query<HierarchyViewItem>().ForEach(item => UnbindItem(window, view, item));

        public void Dispose()
        {
            EditorApplication.delayCall -= BindExisting;
            HierarchyWindow.BindViewItem -= BindItem;
            HierarchyWindow.UnbindViewItem -= UnbindItem;
            HierarchyWindow.UnbindView -= UnbindView;
            foreach (var binding in rows.Values) binding.Dispose();
            rows.Clear();
        }

        private sealed class RowBinding : IDisposable
        {
            private readonly HierarchyWindow window;
            private readonly HierarchyViewItem item;
            private readonly Scene scene;
            private readonly SceneCatalog catalog;
            private readonly SceneFavoriteStore favorites;
            private readonly Label arrow;
            private readonly string previousTooltip;

            internal RowBinding(HierarchyWindow window, HierarchyViewItem item, Scene scene, SceneCatalog catalog, SceneFavoriteStore favorites)
            {
                this.window = window;
                this.item = item;
                this.scene = scene;
                this.catalog = catalog;
                this.favorites = favorites;
                previousTooltip = item.Name.tooltip;
                item.Name.tooltip = "Open project scene selector";
                item.Name.RegisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
                arrow = new Label("▾") { name = "hierarchy-toolkit-scene-selector-arrow", tooltip = "Open project scene selector" };
                arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
                arrow.style.width = 12f;
                arrow.style.minWidth = 12f;
                arrow.style.flexShrink = 0f;
                arrow.RegisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
                item.Name.parent.Add(arrow);
            }

            private void PointerDown(PointerDownEvent evt)
            {
                if (evt.button != 0) return;
                evt.StopImmediatePropagation();
                var bound = item.Name.worldBound;
                var anchor = new Rect(window.position.x + bound.x, window.position.y + bound.y, bound.width, bound.height);
                UnityEditor.PopupWindow.Show(anchor, new SceneSelectorPopup(catalog, favorites, scene.path));
            }

            public void Dispose()
            {
                item.Name.tooltip = previousTooltip;
                item.Name.UnregisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
                arrow.UnregisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
                arrow.RemoveFromHierarchy();
            }
        }
    }
}
