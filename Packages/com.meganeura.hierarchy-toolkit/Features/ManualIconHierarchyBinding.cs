using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualIconHierarchyBinding : IDisposable
    {
        private readonly ManualIconCache cache;
        private readonly Dictionary<VisualElement, (EntityId id, StyleBackground background)> icons = new();

        internal ManualIconHierarchyBinding(ManualIconCache cache)
        {
            this.cache = cache;
            HierarchyWindow.BindViewItem += BindItem;
            HierarchyWindow.UnbindViewItem += UnbindItem;
            HierarchyWindow.UnbindView += UnbindView;
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
            UnbindIcon(item.Icon);
            if (item.Handler is HierarchyGameObjectHandler handler)
                BindIcon(item.Icon, handler.GetEntityId(item.Node));
        }

        private void UnbindItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item) => UnbindIcon(item.Icon);
        private void UnbindView(HierarchyWindow window, HierarchyView view)
            => view.Query<HierarchyViewItem>().ForEach(item => UnbindIcon(item.Icon));

        internal void BindIcon(VisualElement icon, EntityId id)
        {
            UnbindIcon(icon);
            if (icon == null) return;
            icons.Add(icon, (id, icon.style.backgroundImage));
            Apply(icon, id, icon.style.backgroundImage);
        }

        internal void UnbindIcon(VisualElement icon)
        {
            if (icon != null && icons.Remove(icon, out var original)) icon.style.backgroundImage = original.background;
        }

        private void Refresh()
        {
            foreach (var icon in icons) Apply(icon.Key, icon.Value.id, icon.Value.background);
        }

        private void Apply(VisualElement element, EntityId id, StyleBackground original)
        {
            if (!cache.TryGetIcon(id, out var icon)) element.style.backgroundImage = original;
            else if (icon is Sprite sprite) element.style.backgroundImage = new StyleBackground(sprite);
            else element.style.backgroundImage = new StyleBackground((Texture2D)icon);
        }

        public void Dispose()
        {
            EditorApplication.delayCall -= BindExistingRows;
            HierarchyWindow.BindViewItem -= BindItem;
            HierarchyWindow.UnbindViewItem -= UnbindItem;
            HierarchyWindow.UnbindView -= UnbindView;
            cache.Changed -= Refresh;
            foreach (var icon in icons) icon.Key.style.backgroundImage = icon.Value.background;
            icons.Clear();
        }
    }
}
