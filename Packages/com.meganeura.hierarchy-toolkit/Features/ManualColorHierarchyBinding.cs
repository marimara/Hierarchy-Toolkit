using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    // The Unity 6.6 UI Toolkit Hierarchy does not dispatch the IMGUI row callback.
    internal sealed class ManualColorHierarchyBinding : IDisposable
    {
        private readonly ManualColorCache cache;
        private readonly Dictionary<VisualElement, (EntityId id, StyleColor background)> rows = new();

        internal ManualColorHierarchyBinding(ManualColorCache cache)
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
            UnbindRow(item.RowContainer);
            if (item.Handler is HierarchyGameObjectHandler handler)
                BindRow(item.RowContainer, handler.GetEntityId(item.Node));
        }

        private void UnbindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
            => UnbindRow(item.RowContainer);

        private void UnbindView(HierarchyWindow window, HierarchyView view)
            => view.Query<HierarchyViewItem>().ForEach(item => UnbindRow(item.RowContainer));

        internal void BindRow(VisualElement row, EntityId id)
        {
            UnbindRow(row);
            rows.Add(row, (id, row.style.backgroundColor));
            Apply(row, id, row.style.backgroundColor);
        }

        internal void UnbindRow(VisualElement row)
        {
            if (row != null && rows.Remove(row, out var binding))
                row.style.backgroundColor = binding.background;
        }

        private void Refresh()
        {
            foreach (var row in rows)
                Apply(row.Key, row.Value.id, row.Value.background);
        }

        private void Apply(VisualElement row, EntityId id, StyleColor background)
        {
            if (!Selection.Contains(id) && cache.TryGetColor(id, out var color) && color.a > 0f)
            {
                color.a = Mathf.Clamp01(color.a) * 0.18f * cache.HierarchyIntensity(id);
                row.style.backgroundColor = color;
            }
            else row.style.backgroundColor = background;
        }

        public void Dispose()
        {
            EditorApplication.delayCall -= BindExistingRows;
            HierarchyWindow.BindViewItem -= BindItem;
            HierarchyWindow.UnbindViewItem -= UnbindItem;
            HierarchyWindow.UnbindView -= UnbindView;
            Selection.selectionChanged -= Refresh;
            cache.Changed -= Refresh;
            foreach (var row in rows) row.Key.style.backgroundColor = row.Value.background;
            rows.Clear();
        }
    }
}
