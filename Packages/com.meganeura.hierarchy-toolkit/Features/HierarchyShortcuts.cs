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
        private const string DefaultParentShortcutId = "Hierarchy Toolkit/Toggle Default Parent";

        private readonly Dictionary<HierarchyViewItem, RowBinding> rows = new();
        private HierarchyWindow hoveredWindow;
        private Vector2 hoveredPosition;
        private bool hasHoveredPosition;

        internal HierarchyShortcuts()
        {
            HierarchyWindow.BindViewItem += BindItem;
            HierarchyWindow.UnbindViewItem += UnbindItem;
            HierarchyWindow.UnbindView += UnbindView;
            EditorApplication.delayCall += BindExisting;
            ShortcutManager.RegisterContext(this);
        }

        public bool active => HierarchyToolkitPreferences.ToolkitEnabled
            && (HierarchyToolkitPreferences.IsShortcutSelected(HierarchyShortcut.ToggleDefaultParent)
                || HierarchyToolkitPreferences.IsShortcutSelected(HierarchyShortcut.ExpandCollapseHovered)
                || HierarchyToolkitPreferences.IsShortcutSelected(HierarchyShortcut.IsolateHoveredBranch))
            && TryGetHoveredTarget(out _, out _, out _) && !IsEditingText(hoveredWindow);

        [Shortcut(ToggleShortcutId, typeof(HierarchyShortcuts), KeyCode.E)]
        private static void ToggleHovered(ShortcutArguments arguments)
        {
            if (arguments.context is HierarchyShortcuts shortcuts)
                shortcuts.TryToggleHovered();
        }

        [Shortcut(IsolateShortcutId, typeof(HierarchyShortcuts), KeyCode.E, ShortcutModifiers.Shift)]
        private static void IsolateHovered(ShortcutArguments arguments)
        {
            if (arguments.context is HierarchyShortcuts shortcuts)
                shortcuts.TryIsolateHovered();
        }

        [Shortcut(DefaultParentShortcutId, typeof(HierarchyShortcuts), KeyCode.D)]
        private static void ToggleDefaultParentHovered(ShortcutArguments arguments)
        {
            if (arguments.context is HierarchyShortcuts shortcuts)
                shortcuts.TryToggleDefaultParentHovered();
        }

        internal bool TryToggleHovered()
            => HierarchyToolkitPreferences.IsShortcutEnabled(HierarchyShortcut.ExpandCollapseHovered)
               && !IsEditingText(hoveredWindow)
               && TryGetHoveredTarget(out _, out var view, out var node)
               && HierarchyNavigation.ToggleExpanded(view, node);

        internal bool TryIsolateHovered()
            => HierarchyToolkitPreferences.IsShortcutEnabled(HierarchyShortcut.IsolateHoveredBranch)
               && !IsEditingText(hoveredWindow)
               && TryGetHoveredTarget(out _, out var view, out var node)
               && HierarchyNavigation.Isolate(view, node);

        internal bool TryToggleDefaultParentHovered()
            => HierarchyToolkitPreferences.IsShortcutEnabled(HierarchyShortcut.ToggleDefaultParent)
               && HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.DefaultParent)
               && !IsEditingText(hoveredWindow)
               && TryGetHoveredTarget(out var target, out _, out _)
               && DefaultParentOperations.Toggle(target);

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
        }

        private void UnbindView(HierarchyWindow window, HierarchyView view)
        {
            view.Query<HierarchyViewItem>().ForEach(item => UnbindItem(window, view, item));
            if (hoveredWindow == window) ClearHovered();
        }

        internal void SetHovered(HierarchyWindow window, Vector2 pointerPosition)
        {
            hoveredWindow = window;
            hoveredPosition = pointerPosition;
            hasHoveredPosition = true;
        }

        private void ClearHovered(HierarchyWindow window = null)
        {
            if (window != null && hoveredWindow != window) return;
            hoveredWindow = null;
            hasHoveredPosition = false;
        }

        internal bool TryGetHoveredTarget(out GameObject target, out HierarchyView view, out HierarchyNode node)
        {
            target = null;
            view = null;
            node = HierarchyNode.Null;
            var panel = hoveredWindow?.rootVisualElement?.panel;
            if (!hasHoveredPosition || panel == null) return false;

            var picked = panel.Pick(hoveredPosition);
            var item = picked as HierarchyViewItem ?? picked?.GetFirstAncestorOfType<HierarchyViewItem>();
            if (item == null || item.panel != panel || item.Handler is not HierarchyGameObjectHandler handler)
                return false;

            view = item.View;
            node = item.Node;
            if (view?.Source == null || !view.Source.IsCreated || node == HierarchyNode.Null)
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
                item.RegisterCallback<PointerMoveEvent>(PointerMove);
                item.RegisterCallback<PointerLeaveEvent>(PointerLeave);
            }

            private void PointerEnter(PointerEnterEvent evt) => owner.SetHovered(window, evt.position);
            private void PointerMove(PointerMoveEvent evt) => owner.SetHovered(window, evt.position);
            private void PointerLeave(PointerLeaveEvent evt) => owner.ClearHovered(window);

            public void Dispose()
            {
                item.UnregisterCallback<PointerEnterEvent>(PointerEnter);
                item.UnregisterCallback<PointerMoveEvent>(PointerMove);
                item.UnregisterCallback<PointerLeaveEvent>(PointerLeave);
            }
        }
    }
}
