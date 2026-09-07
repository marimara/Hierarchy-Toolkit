using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class SeparatorLabel : IDisposable
    {
        private readonly Label native;
        private readonly VisualElement icon;
        private readonly Label header;

        private readonly StyleColor originalColor;
        private readonly StyleLength originalWidth;
        private readonly StyleFloat originalShrink;
        private readonly StyleEnum<Visibility> originalIconVisibility;

        private readonly GUIContent measureContent = new();
        private GUIStyle measureStyle;

        private bool applied;

        internal SeparatorLabel(Label native, VisualElement icon)
        {
            this.native = native;
            this.icon = icon;

            originalColor = native.style.color;
            originalWidth = native.style.width;
            originalShrink = native.style.flexShrink;
            originalIconVisibility = icon.style.visibility;

            header = new Label
            {
                name = "hierarchy-toolkit-header",
                pickingMode = PickingMode.Ignore,
                enableRichText = false
            };

            header.style.position = Position.Absolute;
            header.style.marginLeft = 0f;
            header.style.marginRight = 0f;
            header.style.marginTop = 0f;
            header.style.marginBottom = 0f;

            header.style.paddingLeft = 0f;
            header.style.paddingRight = 0f;
            header.style.paddingTop = 0f;
            header.style.paddingBottom = 0f;

            header.style.unityTextAlign = TextAnchor.MiddleLeft;
            header.style.overflow = Overflow.Hidden;
            header.style.textOverflow = TextOverflow.Ellipsis;
            header.style.whiteSpace = WhiteSpace.NoWrap;
        }

        internal void Apply(SeparatorStyle style, bool selected)
        {
            if (!style.Enabled)
            {
                Restore();
                return;
            }

            var parent = native.parent;
            if (parent == null)
                return;

            if (!applied)
            {
                var index = parent.IndexOf(native);
                parent.Insert(index + 1, header);
                applied = true;
            }

            header.text = style.ResolveText(native.text);

            header.style.unityFontStyleAndWeight =
                style.Bold ? FontStyle.Bold : FontStyle.Normal;

            header.style.color =
                selected
                    ? Color.white
                    : style.HasTextColor
                        ? style.TextColor
                        : EditorGUIUtility.isProSkin
                            ? new Color(0.9f, 0.9f, 0.9f)
                            : new Color(0.12f, 0.12f, 0.12f);

            measureStyle ??= new GUIStyle(EditorStyles.label);
            measureStyle.fontStyle =
                style.Bold ? FontStyle.Bold : FontStyle.Normal;

            measureContent.text = header.text;

            var width = measureStyle.CalcSize(measureContent).x + 4f;

            // Position the custom label where Unity's native name normally starts.
            header.style.left = native.layout.x;
            header.style.top = native.layout.y;
            header.style.width = width;
            header.style.height = native.layout.height;

            // Hide Unity's native text without changing native.text.
            native.style.color = Color.clear;
            native.style.width = width;
            native.style.flexShrink = 1f;

            icon.style.visibility = Visibility.Hidden;
        }

        private void Restore()
        {
            if (!applied)
                return;

            header.RemoveFromHierarchy();

            native.style.color = originalColor;
            native.style.width = originalWidth;
            native.style.flexShrink = originalShrink;

            icon.style.visibility = originalIconVisibility;

            applied = false;
        }

        public void Dispose()
        {
            Restore();
        }
    }
}