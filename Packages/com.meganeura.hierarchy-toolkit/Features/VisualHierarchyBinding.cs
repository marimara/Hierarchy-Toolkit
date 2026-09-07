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
        private readonly SeparatorFeature separators;
        private readonly ActivationToggleFeature activation;
        private readonly ComponentMinimapFeature minimap;

        internal VisualHierarchyBinding(ZebraStripingFeature zebra, HierarchyLinesFeature lines, ManualColorCache colors, SeparatorFeature separators, ActivationToggleFeature activation, ComponentMinimapFeature minimap)
        {
            this.zebra = zebra;
            this.lines = lines;
            this.colors = colors;
            this.separators = separators;
            this.activation = activation;
            this.minimap = minimap;
            HierarchyWindow.BindViewItem += BindItem;
            HierarchyWindow.UnbindViewItem += UnbindItem;
            HierarchyWindow.UnbindView += UnbindView;
            Selection.selectionChanged += Refresh;
            EditorApplication.hierarchyChanged += HierarchyChanged;
            Undo.undoRedoPerformed += HierarchyChanged;
            colors.Changed += Refresh;
            zebra.Changed += Refresh;
            lines.Changed += Refresh;
            separators.Changed += Refresh;
            activation.Changed += RefreshActivation;
            minimap.Changed += RefreshActivation;
            ObjectChangeEvents.changesPublished += ObjectsChanged;
            EditorApplication.playModeStateChanged += PlayModeChanged;
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
            rows.Add(item, new Decoration(item, id, zebra, lines, cache, colors, separators, activation, minimap));
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
            foreach (var row in rows.Values) row.Refresh();
            EditorApplication.RepaintHierarchyWindow();
        }

        private void ObjectsChanged(ref ObjectChangeEventStream stream) => RefreshActivation();
        private void PlayModeChanged(PlayModeStateChange state) => RefreshActivation();
        private void RefreshActivation()
        {
            foreach (var row in rows.Values) row.RefreshActivation();
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
            separators.Changed -= Refresh;
            activation.Changed -= RefreshActivation;
            minimap.Changed -= RefreshActivation;
            ObjectChangeEvents.changesPublished -= ObjectsChanged;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
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
            private readonly ManualColorCache colors;
            private readonly SeparatorFeature separators;
            private readonly SeparatorLabel label;
            private readonly ActivationToggleControl activationControl;

            internal Decoration(HierarchyViewItem item, EntityId id, ZebraStripingFeature zebra, HierarchyLinesFeature lines,
                HierarchyLinesFeature.BranchCache branches, ManualColorCache colors, SeparatorFeature separators, ActivationToggleFeature activation, ComponentMinimapFeature minimap)
            {
                this.item = item;
                this.id = id;
                this.zebra = zebra;
                this.lines = lines;
                this.branches = branches;
                this.colors = colors;
                this.separators = separators;
                label = new SeparatorLabel(item.Name, item.Icon);
                name = "hierarchy-toolkit-row-visuals";
                pickingMode = PickingMode.Ignore;
                style.position = Position.Absolute;
                style.left = style.right = style.top = style.bottom = 0f;
                item.RowContainer.Insert(0, this);
                if (item.Handler is HierarchyGameObjectHandler)
                    activationControl = new ActivationToggleControl(item, EditorUtility.EntityIdToObject(id) as GameObject, activation, minimap);
                generateVisualContent += Paint;
                item.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Toggle.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.Toggle.parent.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
                item.RowContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            }

            private void GeometryChanged(GeometryChangedEvent evt) => MarkDirtyRepaint();

            internal void Refresh()
            {
                RefreshActivation();
                separators.TryGet(id, out var style);
                // Store/scene notifications can arrive after a native node was removed but
                // before its row was unbound. Selection's entity lookup tolerates that interval.
                label.Apply(style, Selection.Contains(id));
                MarkDirtyRepaint();
            }

            internal void RefreshActivation() => activationControl?.Refresh();

            private void Paint(MeshGenerationContext context)
            {
                var model = item.View?.ViewModel;
                if (model == null || !model.IsCreated || model.Updating) return;
                var node = item.Node;
                var index = model.IndexOf(node);
                if (index < 0) return;
                var selected = Selection.Contains(id) || item.View.IsSelected(node);
                if (separators.TryGet(id, out var separator))
                {
                    // Header replaces zebra. Manual color lives on the native row behind this decoration.
                    if (SeparatorFeature.ShouldDrawBackground(selected, colors.TryGetColor(id, out _)))
                        DrawBackground(context.painter2D, contentRect, SeparatorFeature.Background(separator, EditorGUIUtility.isProSkin));
                    return; // No hierarchy lines through headers, including their indentation.
                }
                zebra.Draw(context.painter2D, contentRect, id, index, selected);
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

            private static void DrawBackground(Painter2D painter, Rect rect, Color color)
            {
                if (rect.width <= 0f || rect.height <= 0f || color.a <= 0f) return;
                painter.fillColor = color;
                painter.BeginPath();
                painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
                painter.LineTo(new Vector2(rect.xMax, rect.yMin));
                painter.LineTo(new Vector2(rect.xMax, rect.yMax));
                painter.LineTo(new Vector2(rect.xMin, rect.yMax));
                painter.ClosePath();
                painter.Fill();
            }

            public void Dispose()
            {
                label.Dispose();
                activationControl?.Dispose();
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
