using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    /// <summary>Data for the current IMGUI row callback; does not resolve or retain scene objects.</summary>
    public readonly struct HierarchyRowContext
    {
        public EntityId EntityId { get; }
        public Rect RowRect { get; }

        public HierarchyRowContext(EntityId entityId, Rect rowRect)
        {
            EntityId = entityId;
            RowRect = rowRect;
        }
    }
}
