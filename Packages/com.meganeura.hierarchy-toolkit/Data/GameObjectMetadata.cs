using System;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    /// <summary>Serialized customization for one stable scene object. Mutate through its store.</summary>
    [Serializable]
    public sealed class GameObjectMetadata
    {
        [SerializeField] private string objectId;
        [SerializeField] private Color customColor;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("customIconPath")] private string customIconReference;
        [SerializeField] private bool hasColorOverride;
        [SerializeField] private bool hasIconOverride;
        [SerializeField] private SeparatorStyle separator;

        /// <summary>The string representation of the object's GlobalObjectId.</summary>
        public string ObjectId => objectId;
        /// <summary>The custom color, meaningful only when HasColorOverride is true.</summary>
        public Color CustomColor => customColor;
        /// <summary>A stable asset GUID/local ID or prefixed built-in name, meaningful only with an explicit override.</summary>
        public string CustomIconReference => customIconReference;
        /// <summary>Whether a color was explicitly assigned.</summary>
        public bool HasColorOverride => hasColorOverride;
        /// <summary>Whether an icon was explicitly assigned.</summary>
        public bool HasIconOverride => hasIconOverride;
        /// <summary>Whether the entry has no customization.</summary>
        public bool IsEmpty => !hasColorOverride && !hasIconOverride && !separator.Enabled;

        /// <summary>Optional header styling; disabled by default.</summary>
        public SeparatorStyle Separator => separator;

        internal GameObjectMetadata(string id) { objectId = id; }
        internal void SetColor(Color value) { customColor = value; hasColorOverride = true; }
        internal void ClearColor() { customColor = default; hasColorOverride = false; }
        internal void SetIcon(string value) { customIconReference = value; hasIconOverride = true; }
        internal bool MigrateIconReference()
        {
            if (!hasIconOverride || string.IsNullOrEmpty(customIconReference)) return false;
            var stable = HierarchyIconReference.Normalize(customIconReference);
            if (stable == customIconReference) return false;
            customIconReference = stable;
            return true;
        }
        internal void ClearIcon() { customIconReference = null; hasIconOverride = false; }
        internal void SetSeparator(SeparatorStyle value) { separator = value; }
    }
}


