using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    // Reserve from right to left, activation first. Later controls use the remaining space.
    // Native label/control bounds are measured before any reservations are made.
    internal struct HierarchyRowLayout
    {
        private readonly Rect row;
        private readonly float contentEnd;
        private float right;
        internal HierarchyRowLayout(Rect row, float contentEnd, float rightBoundary)
        {
            this.row = row;
            this.contentEnd = Mathf.Max(row.xMin, contentEnd) + 4f;
            right = Mathf.Min(row.xMax, rightBoundary) - 2f;
        }

        internal bool TryReserveRight(float width, out Rect rect)
        {
            rect = default;
            if (width <= 0f || row.height <= 0f || float.IsNaN(right) || float.IsNaN(contentEnd)
                || right - width < contentEnd) return false;
            rect = new Rect(right - width, row.yMin, width, row.height);
            right -= width + 2f;
            return true;
        }
    }
}
