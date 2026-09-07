using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class SeparatorOperations
    {
        internal static SeparatorStyle InitialStyle(GameObject target)
        {
            if (ManualColorOperations.IsSupported(target))
            {
                var store = SceneMetadataStore.Find(target.scene);
                if (store != null && store.TryGetMetadata(target, out var entry) && entry.Separator.Enabled)
                    return entry.Separator;
            }
            return new SeparatorStyle(null);
        }

        internal static void Mark(GameObject[] targets) => ManualColorOperations.ApplyMetadata(targets, true,
            "Mark Hierarchy Separators", (store, target) => store.SetSeparator(target, InitialStyle(target)));

        internal static void Clear(GameObject[] targets) => ManualColorOperations.ApplyMetadata(targets, false,
            "Clear Hierarchy Separators", (store, target) => store.ClearSeparator(target));

        internal static void Edit(GameObject target, SeparatorStyle style) => ManualColorOperations.ApplyMetadata(new[] { target }, true,
            "Edit Hierarchy Separator", (store, item) => store.SetSeparator(item, style));
    }
}
