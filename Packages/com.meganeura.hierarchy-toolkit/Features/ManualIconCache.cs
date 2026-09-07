using System.Collections.Generic;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualIconCache : ManualMetadataCache<UnityEngine.Object>
    {
        private readonly Dictionary<string, UnityEngine.Object> assets = new();

        internal bool TryGetIcon(EntityId id, out UnityEngine.Object icon)
            => TryGetValue(id, out icon) && icon != null;

        protected override void ClearResolvedAssets() => assets?.Clear();

        protected override bool TryResolve(GameObjectMetadata entry, out UnityEngine.Object icon)
        {
            icon = null;
            if (!entry.HasIconOverride || string.IsNullOrEmpty(entry.CustomIconReference)) return false;
            if (!assets.TryGetValue(entry.CustomIconReference, out icon))
            {
                icon = HierarchyIconReference.Resolve(entry.CustomIconReference);
                assets.Add(entry.CustomIconReference, icon);
            }
            return icon != null;
        }
    }
}
