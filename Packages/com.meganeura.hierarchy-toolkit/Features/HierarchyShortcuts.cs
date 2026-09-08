using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class HierarchyShortcuts : IShortcutContext, IDisposable
    {
        private const string ToggleShortcutId = "Hierarchy Toolkit/Expand or Collapse Hovered";
        private const string IsolateShortcutId = "Hierarchy Toolkit/Isolate Hovered Branch";

        private readonly Dictionary<HierarchyViewItem, RowBinding> rows = new();
        private HierarchyWindow hoveredWindow;
        private HierarchyViewItem hoveredItem;

        internal HierarchyShortcuts()
        {
            HierarchyWindow.BindViewItem += BindItem;
            HierarchyWindow.UnbindViewItem += UnbindItem;
            HierarchyWindow.UnbindView += UnbindView;
            EditorApplication.delayCall += BindExisting;
            ShortcutManager.RegisterContext(this);
        }

        public bool active => TryGetHoveredTarget(out _, out _, out _) && !IsEditingText(hoveredWindow);

        [Shortcut(ToggleShortcutId, typeof(HierarchyShortcuts), KeyCode.E)]
        private static void ToggleHovered(ShortcutArguments arguments)
        {
            if (arguments.context is HierarchyShortcuts shortcuts
                && shortcuts.TryGetHoveredTarget(out _, out var view, out var node)
                && !IsEditingText(shortcuts.hoveredWindow))
                HierarchyNavigation.ToggleExpanded(view, node);
        }

        [Shortcut(IsolateShortcutId, typeof(HierarchyShortcuts), KeyCode.E, ShortcutModifiers.Shift)]
        private static void IsolateHovered(ShortcutArguments arguments)
        {
            if (arguments.context is HierarchyShortcuts shortcuts
                && shortcuts.TryGetHoveredTarget(out _, out var view, out var node)
                && !IsEditingText(shortcuts.hoveredWindow))
                HierarchyNavigation.Isolate(view, node);
        }

        private void BindExisting()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<HierarchyWindow>())
                window.rootVisualElement.Query<HierarchyViewItem>().ForEach(item => BindItem(window, item.View, item));
        }

        private void BindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            UnbindItem(window, view, item);
            if (item.Handler is not HierarchyGameObjectHandler) return;
            rows.Add(item, new RowBinding(this, window, item));
        }

        private void UnbindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (rows.Remove(item, out var binding)) binding.Dispose();
            ClearHovered(item);
        }

        private void UnbindView(HierarchyWindow window, HierarchyView view)
        {
            view.Query<HierarchyViewItem>().ForEach(item => UnbindItem(window, view, item));
            if (hoveredWindow == window) ClearHovered();
        }

        private void SetHovered(HierarchyWindow window, HierarchyViewItem item)
        {
            hoveredWindow = window;
            hoveredItem = item;
        }

        private void ClearHovered(HierarchyViewItem item = null)
        {
            if (item != null && hoveredItem != item) return;
            hoveredWindow = null;
            hoveredItem = null;
        }

        private bool TryGetHoveredTarget(out GameObject target, out HierarchyView view, out HierarchyNode node)
        {
            target = null;
            view = hoveredItem?.View;
            node = hoveredItem?.Node ?? HierarchyNode.Null;
            if (hoveredWindow == null || hoveredItem == null || hoveredItem.panel == null
                || view?.Source == null || !view.Source.IsCreated
                || hoveredItem.Handler is not HierarchyGameObjectHandler handler)
                return false;

            target = handler.GetGameObject(node);
            return target != null;
        }

        internal static bool IsEditingText(HierarchyWindow window)
        {
            var focused = window?.rootVisualElement?.panel?.focusController?.focusedElement;
            return IsTextEditingElement(focused);
        }

        internal static bool IsTextEditingElement(object focused)
            => focused is ITextEdition
               || focused is TextField
               || focused is VisualElement element && element.GetFirstAncestorOfType<TextField>() != null;

        public void Dispose()
        {
            ShortcutManager.UnregisterContext(this);
            EditorApplication.delayCall -= BindExisting;
            HierarchyWindow.BindViewItem -= BindItem;
            HierarchyWindow.UnbindViewItem -= UnbindItem;
            HierarchyWindow.UnbindView -= UnbindView;
            foreach (var binding in rows.Values) binding.Dispose();
            rows.Clear();
            ClearHovered();
        }

        private sealed class RowBinding : IDisposable
        {
            private readonly HierarchyShortcuts owner;
            private readonly HierarchyWindow window;
            private readonly HierarchyViewItem item;

            internal RowBinding(HierarchyShortcuts owner, HierarchyWindow window, HierarchyViewItem item)
            {
                this.owner = owner;
                this.window = window;
                this.item = item;
                item.RegisterCallback<PointerEnterEvent>(PointerEnter);
                item.RegisterCallback<PointerLeaveEvent>(PointerLeave);
            }

            private void PointerEnter(PointerEnterEvent evt) => owner.SetHovered(window, item);
            private void PointerLeave(PointerLeaveEvent evt) => owner.ClearHovered(item);

            public void Dispose()
            {
                item.UnregisterCallback<PointerEnterEvent>(PointerEnter);
                item.UnregisterCallback<PointerLeaveEvent>(PointerLeave);
                owner.ClearHovered(item);
            }
        }
    }
}
