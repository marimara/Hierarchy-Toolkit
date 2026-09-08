using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ComponentMinimapControl : VisualElement
    {
        internal const float IconSize = 14f;
        internal const float Spacing = 2f;
        private readonly List<Image> images = new();
        private ComponentMinimapCache.Icon[] snapshot;
        private GameObject target;

        internal void Bind(GameObject value) => target = value;

        internal static Component ResolveComponent(GameObject target, Type type)
        {
            if (!ComponentMinimapCache.IsSupported(target) || type == null) return null;
            // The existing type icon represents the first exact-type component in attachment order.
            // Resolve only on explicit interaction; never collect components during row refresh.
            foreach (var component in target.GetComponents(type))
                if (component != null && component.GetType() == type) return component;
            return null;
        }

        private void PointerDown(PointerDownEvent evt)
        {
            if (!HierarchyToolkitPreferences.IsFeatureEnabled(HierarchyFeature.ComponentPopupInspector)
                || evt.button != 0 || !evt.altKey || evt.currentTarget is not Image image) return;
            evt.StopImmediatePropagation();
            var index = images.IndexOf(image);
            if (snapshot == null || index < 0 || index >= snapshot.Length) return;
            var component = ResolveComponent(target, snapshot[index].Type);
            if (component != null)
                ComponentPopupInspector.Open(component, GUIUtility.GUIToScreenRect(image.worldBound));
        }

        private static void StopAltPointerUp(PointerUpEvent evt)
        { if (evt.button == 0 && evt.altKey) evt.StopPropagation(); }
        private static void StopAltMouseDown(MouseDownEvent evt)
        { if (evt.button == 0 && evt.altKey) evt.StopPropagation(); }
        private static void StopAltMouseUp(MouseUpEvent evt)
        { if (evt.button == 0 && evt.altKey) evt.StopPropagation(); }
        private static void StopAltClick(ClickEvent evt)
        { if (evt.button == 0 && evt.altKey) evt.StopPropagation(); }

        internal ComponentMinimapControl()
        {
            name = "hierarchy-toolkit-component-minimap";
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
        }

        internal static int Reserve(ref HierarchyRowLayout layout, int count, out Rect bounds)
        {
            bounds = default;
            if (count <= 0) return 0;
            // Probe a copy to find the largest prefix that fits, then reserve one compact strip.
            var probe = layout;
            var visible = 0;
            while (visible < count && probe.TryReserveRight(IconSize, out _)) ++visible;
            if (visible == 0) return 0;
            return layout.TryReserveRight(visible * (IconSize + Spacing) - Spacing, out bounds) ? visible : 0;
        }

        internal void Refresh(ComponentMinimapCache.Icon[] icons, ref HierarchyRowLayout layout)
        {
            if (!ReferenceEquals(snapshot, icons))
            {
                snapshot = icons;
                while (images.Count < icons.Length)
                {
                    // Normal input still bubbles to Unity; only Alt-left-click belongs to the popup.
                    var image = new Image { pickingMode = PickingMode.Position, focusable = false, scaleMode = ScaleMode.ScaleToFit };
                    image.RegisterCallback<PointerDownEvent>(PointerDown);
                    image.RegisterCallback<PointerUpEvent>(StopAltPointerUp);
                    image.RegisterCallback<MouseDownEvent>(StopAltMouseDown);
                    image.RegisterCallback<MouseUpEvent>(StopAltMouseUp);
                    image.RegisterCallback<ClickEvent>(StopAltClick);
                    image.style.position = Position.Absolute;
                    images.Add(image);
                    Add(image);
                }
                for (var i = 0; i < images.Count; ++i)
                {
                    images[i].image = i < icons.Length ? icons[i].Texture : null;
                    images[i].tooltip = i < icons.Length ? icons[i].Name : string.Empty;
                }
            }
            var visible = Reserve(ref layout, icons.Length, out var bounds);
            style.display = visible > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            style.left = bounds.x;
            style.top = bounds.y;
            style.width = bounds.width;
            style.height = bounds.height;
            for (var i = 0; i < images.Count; ++i)
            {
                var image = images[i];
                image.style.display = i < visible ? DisplayStyle.Flex : DisplayStyle.None;
                image.style.left = i * (IconSize + Spacing);
                image.style.top = Mathf.Max(0f, (bounds.height - IconSize) * 0.5f);
                image.style.width = IconSize;
                image.style.height = Mathf.Min(IconSize, bounds.height);
            }
        }
    }
}
