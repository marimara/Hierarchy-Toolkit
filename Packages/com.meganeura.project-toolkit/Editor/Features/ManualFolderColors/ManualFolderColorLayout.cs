using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal enum ProjectItemPresentation
    {
        Unsupported,
        ListOrTree,
        Grid
    }

    internal readonly struct ManualFolderColorDrawDecision
    {
        internal ManualFolderColorDrawDecision(ProjectItemPresentation presentation, Rect iconRect, Color color)
        {
            Presentation = presentation;
            IconRect = iconRect;
            Color = color;
        }

        internal ProjectItemPresentation Presentation { get; }

        internal Rect IconRect { get; }

        internal Color Color { get; }

        internal bool ShouldDraw => Presentation != ProjectItemPresentation.Unsupported;
    }

    internal static class ManualFolderColorLayout
    {
        private const float NativeListIconSize = 16f;
        private const float MinimumListIconSize = 12f;
        private const float MinimumGridIconSize = 32f;
        private const float MinimumGridVerticalRemainder = 10f;
        private const float LayoutTolerance = 2f;

        internal static ProjectItemPresentation Classify(Rect itemRect, float singleLineHeight)
        {
            if (itemRect.width <= 0f || itemRect.height <= 0f || singleLineHeight <= 0f)
            {
                return ProjectItemPresentation.Unsupported;
            }

            if (itemRect.height <= singleLineHeight + LayoutTolerance)
            {
                return ProjectItemPresentation.ListOrTree;
            }

            if (IsRecognizedGridRect(itemRect, singleLineHeight))
            {
                return ProjectItemPresentation.Grid;
            }

            return ProjectItemPresentation.Unsupported;
        }

        internal static bool TryGetIconRect(
            Rect itemRect,
            float singleLineHeight,
            out ProjectItemPresentation presentation,
            out Rect iconRect)
        {
            presentation = Classify(itemRect, singleLineHeight);
            if (presentation == ProjectItemPresentation.ListOrTree)
            {
                float size = Mathf.Min(itemRect.height, NativeListIconSize);
                if (size < MinimumListIconSize)
                {
                    iconRect = default;
                    presentation = ProjectItemPresentation.Unsupported;
                    return false;
                }

                iconRect = new Rect(
                    itemRect.x,
                    itemRect.y + (itemRect.height - size) * 0.5f,
                    size,
                    size);
                return true;
            }

            if (presentation == ProjectItemPresentation.Grid)
            {
                float verticalRemainder = itemRect.height - itemRect.width;
                iconRect = new Rect(
                    itemRect.x,
                    itemRect.y + verticalRemainder * 0.5f,
                    itemRect.width,
                    itemRect.width);
                return true;
            }

            iconRect = default;
            return false;
        }

        private static bool IsRecognizedGridRect(Rect itemRect, float singleLineHeight)
        {
            float verticalRemainder = itemRect.height - itemRect.width;
            return itemRect.width >= MinimumGridIconSize
                && verticalRemainder >= MinimumGridVerticalRemainder
                && verticalRemainder <= singleLineHeight + LayoutTolerance;
        }
    }
}
