using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    // A child of Unity's label follows its visibility during rename. Never overwrite native
    // label.text: Unity uses it to seed the rename field. No input callbacks or object references.
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
            header = new Label { name = "hierarchy-toolkit-header", pickingMode = PickingMode.Ignore, enableRichText = false };
            header.style.position = Position.Absolute;
            header.style.left = header.style.right = header.style.top = header.style.bottom = 0f;
            header.style.marginLeft = header.style.marginRight = header.style.marginTop = header.style.marginBottom = 0f;
            header.style.paddingLeft = header.style.paddingRight = header.style.paddingTop = header.style.paddingBottom = 0f;
            header.style.unityTextAlign = TextAnchor.MiddleLeft;
            header.style.overflow = Overflow.Hidden;
            header.style.textOverflow = TextOverflow.Ellipsis;
            header.style.whiteSpace = WhiteSpace.NoWrap;
        }

        internal void Apply(SeparatorStyle style, bool selected)
        {
            if (!style.Enabled) { Restore(); return; }
            if (!applied) { native.Add(header); applied = true; }
            header.text = style.ResolveText(native.text);
            header.style.unityFontStyleAndWeight = style.Bold ? FontStyle.Bold : FontStyle.Normal;
            header.style.color = selected ? Color.white : style.HasTextColor ? style.TextColor
                : EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.12f, 0.12f, 0.12f);
            native.style.color = Color.clear;
            // Reserve only the header's measured width; the native row still shrinks it in narrow views.
            measureStyle ??= new GUIStyle(EditorStyles.label);
            measureStyle.fontStyle = style.Bold ? FontStyle.Bold : FontStyle.Normal;
            measureContent.text = header.text;
            native.style.width = measureStyle.CalcSize(measureContent).x + 2f;
            native.style.flexShrink = 1f;
            icon.style.visibility = Visibility.Hidden;
        }

        private void Restore()
        {
            if (!applied) return;
            header.RemoveFromHierarchy();
            native.style.color = originalColor;
            native.style.width = originalWidth;
            native.style.flexShrink = originalShrink;
            icon.style.visibility = originalIconVisibility;
            applied = false;
        }

        public void Dispose() => Restore();
    }
}
