using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualColorCache : ManualMetadataCache<Color>
    {
        internal bool TryGetColor(EntityId id, out Color color) => TryGetValue(id, out color);

        protected override bool TryResolve(GameObjectMetadata entry, out Color color)
        {
            color = entry.CustomColor;
            return entry.HasColorOverride;
        }
    }
}
