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
        private const float ListHorizontalInset = 2f;
        private const float GridHorizontalInset = 6f;
        private const float GridTopInset = 2f;
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

            if (itemRect.width >= singleLineHeight * 2f && itemRect.height >= singleLineHeight * 2f)
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
                float size = Mathf.Min(itemRect.height, singleLineHeight);
                iconRect = new Rect(itemRect.x + ListHorizontalInset, itemRect.y, size, size);
                return true;
            }

            if (presentation == ProjectItemPresentation.Grid)
            {
                float availableHeight = itemRect.height - singleLineHeight - GridTopInset;
                float size = Mathf.Min(itemRect.width - GridHorizontalInset * 2f, availableHeight);
                if (size < singleLineHeight)
                {
                    iconRect = default;
                    presentation = ProjectItemPresentation.Unsupported;
                    return false;
                }

                iconRect = new Rect(
                    itemRect.x + (itemRect.width - size) * 0.5f,
                    itemRect.y + GridTopInset,
                    size,
                    size);
                return true;
            }

            iconRect = default;
            return false;
        }
    }
}
