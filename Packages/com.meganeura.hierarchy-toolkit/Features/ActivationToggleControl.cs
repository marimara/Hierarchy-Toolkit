using System;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ActivationToggleControl : VisualElement, IDisposable
    {
        private readonly HierarchyViewItem item;
        private readonly ActivationToggleFeature feature;
        private GameObject target;
        private readonly Toggle toggle;
        private readonly ComponentMinimapFeature minimap;
        private readonly ComponentMinimapControl minimapControl;

        internal ActivationToggleControl(HierarchyViewItem item, GameObject target, ActivationToggleFeature feature, ComponentMinimapFeature minimap)
        {
            this.item = item;
            this.target = target;
            this.feature = feature;
            this.minimap = minimap;
            minimapControl = new ComponentMinimapControl();
            item.RowContainer.Add(minimapControl);
            name = "hierarchy-toolkit-activation";
            style.position = Position.Absolute;
            style.justifyContent = Justify.Center;
            toggle = new Toggle { focusable = false, tooltip = "Toggle this GameObject's own active state (activeSelf). If selected, apply the same state to all eligible selected GameObjects." };
            toggle.style.marginLeft = toggle.style.marginRight = toggle.style.marginTop = toggle.style.marginBottom = 0f;
            Add(toggle);
            // Let Toggle handle the click, but do not let its pointer/mouse events select or drag the row.
            RegisterCallback<PointerDownEvent>(StopPointer);
            RegisterCallback<PointerUpEvent>(StopPointer);
            RegisterCallback<MouseDownEvent>(StopMouse);
            RegisterCallback<MouseUpEvent>(StopMouse);
            RegisterCallback<ClickEvent>(evt => evt.StopPropagation());
            toggle.RegisterValueChangedCallback(OnChanged);
            item.RowContainer.Add(this);
            item.RowContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            item.Name.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            item.RightCustomContainer.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            if (item.NavigateIntoButton != null)
                item.NavigateIntoButton.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            Refresh();
        }

        private static void StopPointer(PointerDownEvent evt) => evt.StopPropagation();
        private static void StopPointer(PointerUpEvent evt) => evt.StopPropagation();
        private static void StopMouse(MouseDownEvent evt) => evt.StopPropagation();
        private static void StopMouse(MouseUpEvent evt) => evt.StopPropagation();
        private void OnChanged(ChangeEvent<bool> evt) { feature.Toggle(target); Refresh(); evt.StopPropagation(); }
        private void GeometryChanged(GeometryChangedEvent evt) => Refresh();

        internal void Refresh()
        {
            var supported = feature.TryGetState(target, out var active);
            toggle.SetValueWithoutNotify(active);
            var row = item.RowContainer;
            var labelEnd = row.WorldToLocal(item.Name.worldBound).xMax;
            var right = row.WorldToLocal(item.RightCustomContainer.worldBound).xMax;
            for (var i = 0; i < item.RightCustomContainer.childCount; ++i)
            {
                var nativeControl = item.RightCustomContainer[i];
                if (nativeControl.resolvedStyle.display != DisplayStyle.None && nativeControl.worldBound.width > 0f)
                    right = Mathf.Min(right, row.WorldToLocal(nativeControl.worldBound).xMin);
            }
            if (item.NavigateIntoButton != null &&
                item.NavigateIntoButton.resolvedStyle.display != DisplayStyle.None)
            {
                right = Mathf.Min(
                    right,
                    row.WorldToLocal(item.NavigateIntoButton.worldBound).xMin
                );
            }
            var layout = new HierarchyRowLayout(row.contentRect, labelEnd, right);
            var rect = default(Rect);
            var visible = supported && layout.TryReserveRight(18f, out rect);
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            minimapControl.Refresh(supported && !visible ? Array.Empty<ComponentMinimapCache.Icon>() : minimap.GetIcons(target), ref layout);
            if (!visible) return;
            style.left = rect.x;
            style.top = rect.y;
            style.width = rect.width;
            style.height = rect.height;
        }

        public void Dispose()
        {
            item.RowContainer.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
            item.Name.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
            item.RightCustomContainer.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
            if (item.NavigateIntoButton != null)
                item.NavigateIntoButton.UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
            toggle.UnregisterValueChangedCallback(OnChanged);
            target = null;
            minimapControl.RemoveFromHierarchy();
            RemoveFromHierarchy();
        }
    }
}
