using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    // Adds an absolute, non-picking label without changing Unity's native name element geometry.
    internal sealed class DefaultParentHierarchyBinding : IDisposable
    {
        private readonly DefaultParentCache cache;
        private readonly Dictionary<HierarchyViewItem, RowBinding> rows = new();
        private bool enabled = true;

        internal bool Enabled
        {
            get => enabled;
            set { if (enabled == value) return; enabled = value; Refresh(); }
        }

        internal DefaultParentHierarchyBinding(DefaultParentCache cache)
        {
            this.cache = cache;
            HierarchyWindow.BindViewItem += BindItem;
            HierarchyWindow.UnbindViewItem += UnbindItem;
            HierarchyWindow.UnbindView += UnbindView;
            Selection.selectionChanged += Refresh;
            cache.Changed += Refresh;
            EditorApplication.delayCall += BindExistingRows;
        }

        private void BindExistingRows()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<HierarchyWindow>())
                window.rootVisualElement.Query<HierarchyViewItem>().ForEach(item => BindItem(window, item.View, item));
        }

        private void BindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            UnbindItem(window, view, item);
            if (item.Handler is HierarchyGameObjectHandler handler)
                rows.Add(item, new RowBinding(item, handler.GetEntityId(item.Node), cache, () => Enabled));
        }

        private void UnbindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (rows.Remove(item, out var binding)) binding.Dispose();
        }

        private void UnbindView(HierarchyWindow window, HierarchyView view)
            => view.Query<HierarchyViewItem>().ForEach(item => UnbindItem(window, view, item));

        private void Refresh()
        {
            foreach (var row in rows.Values) row.Refresh();
        }

        public void Dispose()
        {
            EditorApplication.delayCall -= BindExistingRows;
            HierarchyWindow.BindViewItem -= BindItem;
            HierarchyWindow.UnbindViewItem -= UnbindItem;
            HierarchyWindow.UnbindView -= UnbindView;
            Selection.selectionChanged -= Refresh;
            cache.Changed -= Refresh;
            foreach (var row in rows.Values) row.Dispose();
            rows.Clear();
        }

        private sealed class RowBinding : IDisposable
        {
            private const float Width = 78f;
            private readonly HierarchyViewItem item;
            private readonly EntityId id;
            private readonly DefaultParentCache cache;
            private readonly Func<bool> enabled;
            private readonly Label indicator;

            internal RowBinding(HierarchyViewItem item, EntityId id, DefaultParentCache cache, Func<bool> enabled)
            {
                this.item = item;
                this.id = id;
                this.cache = cache;
                this.enabled = enabled;
                indicator = new Label("Default parent")
                {
                    name = "hierarchy-toolkit-default-parent",
                    pickingMode = PickingMode.Ignore,
                    focusable = false
                };
                indicator.style.position = Position.Absolute;
                indicator.style.marginLeft = indicator.style.marginRight = 0f;
                indicator.style.marginTop = indicator.style.marginBottom = 0f;
                indicator.style.paddingLeft = indicator.style.paddingRight = 0f;
                indicator.style.paddingTop = indicator.style.paddingBottom = 0f;
                indicator.style.fontSize = 10f;
                indicator.style.unityTextAlign = TextAnchor.MiddleLeft;
                indicator.style.whiteSpace = WhiteSpace.NoWrap;
                item.RowContainer.Add(indicator);
                item.RowContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Name.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.RightCustomContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                if (item.NavigateIntoButton != null)
                    item.NavigateIntoButton.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                Refresh();
            }

            private void GeometryChanged(GeometryChangedEvent evt) => Refresh();

            internal void Refresh()
            {
                if (!enabled() || !cache.Contains(id))
                {
                    indicator.style.display = DisplayStyle.None;
                    return;
                }

                var row = item.RowContainer;
                var name = row.WorldToLocal(item.Name.worldBound);
                var left = name.xMax + 5f;
                var right = row.WorldToLocal(item.RightCustomContainer.worldBound).xMax;
                for (var i = 0; i < item.RightCustomContainer.childCount; ++i)
                {
                    var control = item.RightCustomContainer[i];
                    if (control.resolvedStyle.display != DisplayStyle.None && control.worldBound.width > 0f)
                        right = Mathf.Min(right, row.WorldToLocal(control.worldBound).xMin);
                }
                if (item.NavigateIntoButton != null && item.NavigateIntoButton.resolvedStyle.display != DisplayStyle.None)
                    right = Mathf.Min(right, row.WorldToLocal(item.NavigateIntoButton.worldBound).xMin);
                LimitToControl(row.Q<VisualElement>("hierarchy-toolkit-component-minimap"), row, ref right);
                LimitToControl(row.Q<VisualElement>("hierarchy-toolkit-activation"), row, ref right);

                var visible = name.height > 0f && left + Width <= right - 2f;
                indicator.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (!visible) return;
                indicator.style.left = left;
                indicator.style.top = name.y;
                indicator.style.width = Width;
                indicator.style.height = name.height;
                indicator.style.color = Selection.Contains(id)
                    ? new Color(1f, 1f, 1f, 0.7f)
                    : EditorGUIUtility.isProSkin ? new Color(0.65f, 0.65f, 0.65f) : new Color(0.38f, 0.38f, 0.38f);
            }

            private static void LimitToControl(VisualElement control, VisualElement row, ref float right)
            {
                if (control != null && control.resolvedStyle.display != DisplayStyle.None && control.worldBound.width > 0f)
                    right = Mathf.Min(right, row.WorldToLocal(control.worldBound).xMin);
            }

            public void Dispose()
            {
                item.RowContainer.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Name.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.RightCustomContainer.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                if (item.NavigateIntoButton != null)
                    item.NavigateIntoButton.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                indicator.RemoveFromHierarchy();
            }
        }
    }
}
