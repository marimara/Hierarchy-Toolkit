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
        private bool enabled = true;

        internal bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                foreach (var row in rows.Values) row.Refresh(enabled);
            }
        }

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
            rows.Add(item, new RowBinding(window, item, scene, catalog, favorites, () => Enabled));
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
            private readonly Func<bool> enabled;

            private readonly Label arrow;
            private readonly string previousTooltip;

            internal RowBinding(
                HierarchyWindow window,
                HierarchyViewItem item,
                Scene scene,
                SceneCatalog catalog,
                SceneFavoriteStore favorites,
                Func<bool> enabled)
            {
                this.window = window;
                this.item = item;
                this.scene = scene;
                this.catalog = catalog;
                this.favorites = favorites;
                this.enabled = enabled;

                previousTooltip = item.Name.tooltip;
                item.Name.tooltip = enabled() ? "Open project scene selector" : previousTooltip;

                item.Name.RegisterCallback<PointerDownEvent>(
                    PointerDown,
                    TrickleDown.TrickleDown);

                arrow = new Label("▾")
                {
                    name = "hierarchy-toolkit-scene-selector-arrow",
                    tooltip = "Open project scene selector"
                };

                // IMPORTANT:
                // The arrow must NOT participate in Unity's native scene-row layout.
                arrow.style.position = Position.Absolute;

                arrow.style.width = 12f;
                arrow.style.height = 16f;

                arrow.style.marginLeft = 0f;
                arrow.style.marginRight = 0f;
                arrow.style.marginTop = 0f;
                arrow.style.marginBottom = 0f;

                arrow.style.paddingLeft = 0f;
                arrow.style.paddingRight = 0f;
                arrow.style.paddingTop = 0f;
                arrow.style.paddingBottom = 0f;

                arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
                arrow.style.flexShrink = 0f;

                arrow.RegisterCallback<PointerDownEvent>(
                    PointerDown,
                    TrickleDown.TrickleDown);

                // Add to the row as an overlay instead of adding another
                // flex child beside Unity's native scene name.
                item.RowContainer.Add(arrow);

                item.Name.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.RowContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);

                PositionArrow();
                Refresh(enabled());
            }

            internal void Refresh(bool value)
            {
                arrow.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                item.Name.tooltip = value ? "Open project scene selector" : previousTooltip;
            }

            private void GeometryChanged(GeometryChangedEvent evt)
            {
                PositionArrow();
            }

            private void PositionArrow()
            {
                if (arrow == null || item?.Name == null || item.RowContainer == null)
                    return;

                var nameRect = item.RowContainer.WorldToLocal(item.Name.worldBound);

                if (nameRect.width <= 0f || nameRect.height <= 0f)
                    return;

                const float spacing = 1f;

                arrow.style.left = nameRect.xMax + spacing;

                // Center the arrow vertically against Unity's native scene label.
                arrow.style.top =
                    nameRect.yMin + ((nameRect.height - 16f) * 0.5f);
            }

            private void PointerDown(PointerDownEvent evt)
            {
                if (!enabled() || evt.button != 0)
                    return;

                evt.StopImmediatePropagation();

                var bound = item.Name.worldBound;

                var anchor = new Rect(
                    window.position.x + bound.x,
                    window.position.y + bound.y,
                    bound.width,
                    bound.height);

                UnityEditor.PopupWindow.Show(
                    anchor,
                    new SceneSelectorPopup(catalog, favorites, scene.path));
            }

            public void Dispose()
            {
                item.Name.tooltip = previousTooltip;

                item.Name.UnregisterCallback<PointerDownEvent>(
                    PointerDown,
                    TrickleDown.TrickleDown);

                item.Name.UnregisterCallback<GeometryChangedEvent>(
                    GeometryChanged);

                item.RowContainer.UnregisterCallback<GeometryChangedEvent>(
                    GeometryChanged);

                arrow.UnregisterCallback<PointerDownEvent>(
                    PointerDown,
                    TrickleDown.TrickleDown);

                arrow.RemoveFromHierarchy();
            }
        }
    }
}
