using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    public static class HierarchyDrawer
    {
        private static IHierarchyFeature[] features = Array.Empty<IHierarchyFeature>();

        // Registration is intended for editor initialization, not row callbacks.
        public static void Register(IHierarchyFeature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            for (int i = 0; i < features.Length; i++)
                if (ReferenceEquals(features[i], feature))
                    return;

            var updated = new IHierarchyFeature[features.Length + 1];
            Array.Copy(features, updated, features.Length);
            updated[features.Length] = feature;
            features = updated;
        }

        public static void Unregister(IHierarchyFeature feature)
        {
            for (int i = 0; i < features.Length; i++)
            {
                if (!ReferenceEquals(features[i], feature))
                    continue;

                var updated = features.Length == 1
                    ? Array.Empty<IHierarchyFeature>()
                    : new IHierarchyFeature[features.Length - 1];
                Array.Copy(features, 0, updated, 0, i);
                Array.Copy(features, i + 1, updated, i, features.Length - i - 1);
                features = updated;
                return;
            }
        }

        internal static void Initialize()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= DrawRow;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += DrawRow;
        }

        internal static void Shutdown()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= DrawRow;
            features = Array.Empty<IHierarchyFeature>();
        }

        private static void DrawRow(EntityId entityId, Rect rowRect)
        {
            // A stable array also permits registration changes during dispatch.
            var currentFeatures = features;
            if (currentFeatures.Length == 0)
                return;

            var context = new HierarchyRowContext(entityId, rowRect);
            for (int i = 0; i < currentFeatures.Length; i++)
                currentFeatures[i].Draw(in context);
        }
    }
}
