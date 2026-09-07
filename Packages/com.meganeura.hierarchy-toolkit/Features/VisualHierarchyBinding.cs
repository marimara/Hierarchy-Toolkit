using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    // Owns only realized row decorations. Recycled/unbound rows release all references.
    // Relationship caches follow the native visible model, including collapsed/hidden rows.
    internal sealed class VisualHierarchyBinding : IDisposable
    {
        private readonly Dictionary<HierarchyViewItem, Decoration> rows = new();
        private readonly Dictionary<HierarchyView, HierarchyLinesFeature.BranchCache> branches = new();
        private readonly ZebraStripingFeature zebra;
        private readonly HierarchyLinesFeature lines;
        private readonly ManualColorCache colors;

        internal VisualHierarchyBinding(ZebraStripingFeature zebra, HierarchyLinesFeature lines, ManualColorCache colors)
        {
            this.zebra = zebra;
            this.lines = lines;
            this.colors = colors;
            HierarchyWindow.BindViewItem += BindItem;
            HierarchyWindow.UnbindViewItem += UnbindItem;
            HierarchyWindow.UnbindView += UnbindView;
            Selection.selectionChanged += Refresh;
            EditorApplication.hierarchyChanged += HierarchyChanged;
            Undo.undoRedoPerformed += HierarchyChanged;
            colors.Changed += Refresh;
            zebra.Changed += Refresh;
            lines.Changed += Refresh;
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
            var id = item.Handler is HierarchyGameObjectHandler handler ? handler.GetEntityId(item.Node) : default;
            if (!branches.TryGetValue(view, out var cache))
                branches.Add(view, cache = new HierarchyLinesFeature.BranchCache());
            cache.Invalidate();
            rows.Add(item, new Decoration(item, id, zebra, lines, cache));
            // Expansion/reordering can change the parity of already realized rows too.
            Refresh();
        }

        private void UnbindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (rows.Remove(item, out var decoration)) decoration.Dispose();
            if (branches.TryGetValue(view, out var cache)) cache.Invalidate();
        }

        private void UnbindView(HierarchyWindow window, HierarchyView view)
        {
            view.Query<HierarchyViewItem>().ForEach(item => UnbindItem(window, view, item));
            branches.Remove(view);
        }

        private void HierarchyChanged()
        {
            foreach (var cache in branches.Values) cache.Invalidate();
            Refresh();
        }

        private void Refresh()
        {
            foreach (var row in rows.Values) row.MarkDirtyRepaint();
            EditorApplication.RepaintHierarchyWindow();
        }

        public void Dispose()
        {
            EditorApplication.delayCall -= BindExistingRows;
            HierarchyWindow.BindViewItem -= BindItem;
            HierarchyWindow.UnbindViewItem -= UnbindItem;
            HierarchyWindow.UnbindView -= UnbindView;
            Selection.selectionChanged -= Refresh;
            EditorApplication.hierarchyChanged -= HierarchyChanged;
            Undo.undoRedoPerformed -= HierarchyChanged;
            colors.Changed -= Refresh;
            zebra.Changed -= Refresh;
            lines.Changed -= Refresh;
            foreach (var row in rows.Values) row.Dispose();
            rows.Clear();
            branches.Clear();
        }

        private sealed class Decoration : VisualElement, IDisposable
        {
            private readonly HierarchyViewItem item;
            private readonly EntityId id;
            private readonly ZebraStripingFeature zebra;
            private readonly HierarchyLinesFeature lines;
            private readonly HierarchyLinesFeature.BranchCache branches;

            internal Decoration(HierarchyViewItem item, EntityId id, ZebraStripingFeature zebra, HierarchyLinesFeature lines,
                HierarchyLinesFeature.BranchCache branches)
            {
                this.item = item;
                this.id = id;
                this.zebra = zebra;
                this.lines = lines;
                this.branches = branches;
                name = "hierarchy-toolkit-row-visuals";
                pickingMode = PickingMode.Ignore;
                style.position = Position.Absolute;
                style.left = style.right = style.top = style.bottom = 0f;
                item.RowContainer.Insert(0, this);
                generateVisualContent += Paint;
                item.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Toggle.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Toggle.parent.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.RowContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            }

            private void GeometryChanged(GeometryChangedEvent evt) => MarkDirtyRepaint();

            private void Paint(MeshGenerationContext context)
            {
                var model = item.View?.ViewModel;
                if (model == null || !model.IsCreated || model.Updating) return;
                var node = item.Node;
                var index = model.IndexOf(node);
                if (index < 0) return;
                zebra.Draw(context.painter2D, contentRect, id, index,
                    Selection.Contains(id) || item.View.IsSelected(node));
                var depth = model.GetDepth(node);
                if (!lines.Enabled || item.View.Filtering) return;
                if (branches.Dirty || branches.Count != model.Count)
                {
                    branches.Clear();
                    for (var i = 0; i < model.Count; ++i)
                        branches.Add(model.GetDepth(model[i]));
                    branches.Complete();
                }
                var toggle = this.WorldToLocal(item.Toggle.worldBound);
                var gutterEnd = this.WorldToLocal(item.OverrideBarContainer.worldBound).xMax;
                var contentStart = this.WorldToLocal(item.Toggle.parent.worldBound).xMin;
                lines.Draw(context, contentRect, depth, branches, index,
                    gutterEnd, contentStart, toggle, item.View.Filtering);
            }

            public void Dispose()
            {
                generateVisualContent -= Paint;
                item.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Toggle.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Toggle.parent.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.RowContainer.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
                RemoveFromHierarchy();
            }
        }
    }
}
