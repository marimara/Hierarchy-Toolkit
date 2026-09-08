using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class QuickStylePaletteHierarchyBinding : IDisposable
    {
        private readonly Dictionary<HierarchyViewItem, RowBinding> rows = new();
        private readonly Func<bool> enabled;

        internal QuickStylePaletteHierarchyBinding(Func<bool> enabled = null)
        {
            this.enabled = enabled ?? (() => true);
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
            if (item.Handler is not HierarchyGameObjectHandler handler) return;
            rows.Add(item, new RowBinding(window, item, handler.GetEntityId(item.Node), enabled));
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
            private readonly EntityId id;
            private readonly Func<bool> enabled;

            internal RowBinding(HierarchyWindow window, HierarchyViewItem item, EntityId id, Func<bool> enabled)
            {
                this.window = window;
                this.item = item;
                this.id = id;
                this.enabled = enabled;
                item.RowContainer.RegisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
            }

            private void PointerDown(PointerDownEvent evt)
            {
                if (!enabled() || evt.button != 0 || !evt.altKey || item.Toggle.worldBound.Contains(evt.position)) return;
                var target = EditorUtility.EntityIdToObject(id) as GameObject;
                if (!ManualColorOperations.IsSupported(target)) return;
                var bound = item.RowContainer.worldBound;
                var anchor = new Rect(window.position.x + bound.x, window.position.y + bound.y, bound.width, bound.height);
                // Defer until native propagation is complete so selection/rename handling remains Unity-owned.
                QuickStylePalette.QueueOpen(target, anchor);
            }

            public void Dispose()
                => item.RowContainer.UnregisterCallback<PointerDownEvent>(PointerDown, TrickleDown.TrickleDown);
        }
    }
}
