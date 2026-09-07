using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ZebraStripingFeature : IHierarchyFeature
    {
        private readonly ManualColorCache colors;
        private bool enabled = true;
        internal event Action Changed;
        internal bool Enabled
        {
            get => enabled;
            set { if (enabled == value) return; enabled = value; Changed?.Invoke(); }
        }

        internal ZebraStripingFeature(ManualColorCache colors) => this.colors = colors;

        internal bool ShouldDraw(EntityId id, int visibleIndex, bool selected)
            => Enabled && visibleIndex >= 0 && (visibleIndex & 1) != 0
                && !selected && !colors.TryGetColor(id, out _);

        private static Color Tint => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.025f) : new Color(0f, 0f, 0f, 0.025f);

        internal void Draw(Painter2D painter, Rect rect, EntityId id, int index, bool selected)
        {
            if (!ShouldDraw(id, index, selected) || rect.width <= 0f || rect.height <= 0f) return;
            painter.fillColor = Tint;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Fill();
        }

        public void Draw(in HierarchyRowContext context)
        {
            var rect = context.RowRect;
            if (Event.current.type != EventType.Repaint || rect.height <= 0f || rect.width <= 0f) return;
            if (ShouldDraw(context.EntityId, Mathf.FloorToInt(rect.y / rect.height), Selection.Contains(context.EntityId)))
                EditorGUI.DrawRect(rect, Tint);
        }
    }
}
