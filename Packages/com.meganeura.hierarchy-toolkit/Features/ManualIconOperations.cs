using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class ManualIconOperations
    {
        internal static UnityEngine.Object InitialIcon(GameObject target)
        {
            if (!ManualColorOperations.IsSupported(target)) return null;
            var store = SceneMetadataStore.Find(target.scene);
            if (store == null) return null;
            store.MigrateIconReferences();
            return store.TryGetMetadata(target, out var entry) && entry.HasIconOverride
                ? HierarchyIconReference.Resolve(entry.CustomIconReference) : null;
        }

        internal static void Apply(GameObject[] targets, UnityEngine.Object icon)
        {
            var reference = icon != null ? HierarchyIconReference.FromAsset(icon) : null;
            ManualColorOperations.ApplyMetadata(targets, reference != null,
                reference != null ? "Set Hierarchy Icon" : "Clear Hierarchy Icon",
                (store, target) => { if (reference != null) store.SetIcon(target, reference); else store.ClearIcon(target); });
        }
    }
}
