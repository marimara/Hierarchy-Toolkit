using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ManualColorCache : ManualMetadataCache<Color>
    {
        private readonly Dictionary<EntityId, float> hierarchyIntensity = new();

        internal bool TryGetColor(EntityId id, out Color color) => TryGetValue(id, out color);
        internal float HierarchyIntensity(EntityId id)
            => hierarchyIntensity.TryGetValue(id, out var intensity) ? intensity : 1f;

        protected override bool TryResolve(GameObjectMetadata entry, out Color color)
        {
            color = entry.CustomColor;
            return entry.HasColorOverride;
        }

        protected override void ClearResolvedAssets() => hierarchyIntensity.Clear();

        protected override void Rebuilt()
        {
            foreach (var pair in Values)
            {
                var target = EditorUtility.EntityIdToObject(pair.Key) as GameObject;
                if (target == null) continue;
                var distance = 0;
                for (var parent = target.transform.parent; parent != null; parent = parent.parent)
                {
                    if (!TryGetColor(parent.gameObject.GetEntityId(), out var parentColor)
                        || !SameSourceColor(pair.Value, parentColor)) break;
                    ++distance;
                }
                hierarchyIntensity[pair.Key] = IntensityForDepth(distance);
            }
        }

        internal static float IntensityForDepth(int depth)
            => Mathf.Max(0.28f, 1f / (1f + Mathf.Max(0, depth) * 0.45f));

        private static bool SameSourceColor(Color left, Color right)
            => Mathf.Abs(left.r - right.r) <= 0.001f && Mathf.Abs(left.g - right.g) <= 0.001f
                && Mathf.Abs(left.b - right.b) <= 0.001f && Mathf.Abs(left.a - right.a) <= 0.001f;
    }
}
