using System;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    /// <summary>Value snapshot of a header's optional overrides. A default value disables the header.</summary>
    [Serializable]
    public struct SeparatorStyle : IEquatable<SeparatorStyle>
    {
        [SerializeField] private bool enabled;
        [SerializeField] private string displayText;
        [SerializeField] private bool hasBackgroundColor;
        [SerializeField] private Color backgroundColor;
        [SerializeField] private bool hasTextColor;
        [SerializeField] private Color textColor;
        [SerializeField] private bool bold;

        /// <summary>Whether this object renders as a header.</summary>
        public bool Enabled => enabled;
        /// <summary>Empty text uses the GameObject name.</summary>
        public string DisplayText => displayText;
        /// <summary>Whether to replace the theme background.</summary>
        public bool HasBackgroundColor => hasBackgroundColor;
        /// <summary>Custom background, used only when explicitly enabled.</summary>
        public Color BackgroundColor => backgroundColor;
        /// <summary>Whether to replace the theme text color.</summary>
        public bool HasTextColor => hasTextColor;
        /// <summary>Custom text color, used only when explicitly enabled.</summary>
        public Color TextColor => textColor;
        /// <summary>Whether to use bold text.</summary>
        public bool Bold => bold;

        /// <summary>Creates an enabled header with optional colors and a name fallback.</summary>
        public SeparatorStyle(string displayText = null, Color? backgroundColor = null, Color? textColor = null, bool bold = true)
        {
            enabled = true;
            this.displayText = string.IsNullOrWhiteSpace(displayText) ? null : displayText.Replace('\r', ' ').Replace('\n', ' ');
            hasBackgroundColor = backgroundColor.HasValue;
            this.backgroundColor = backgroundColor.GetValueOrDefault();
            hasTextColor = textColor.HasValue;
            this.textColor = textColor.GetValueOrDefault();
            this.bold = bold;
        }

        /// <summary>Resolves display text without changing the object's name.</summary>
        public string ResolveText(string objectName) => string.IsNullOrEmpty(displayText) ? objectName : displayText;
        /// <summary>Compares all serialized options.</summary>
        public bool Equals(SeparatorStyle other) => enabled == other.enabled && (displayText ?? string.Empty) == (other.displayText ?? string.Empty)
            && hasBackgroundColor == other.hasBackgroundColor && backgroundColor.Equals(other.backgroundColor)
            && hasTextColor == other.hasTextColor && textColor.Equals(other.textColor) && bold == other.bold;
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is SeparatorStyle other && Equals(other);
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(enabled, displayText ?? string.Empty, hasBackgroundColor, backgroundColor, hasTextColor, textColor, bold);
    }
}
