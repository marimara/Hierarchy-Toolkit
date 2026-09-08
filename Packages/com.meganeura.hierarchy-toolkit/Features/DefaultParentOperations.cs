using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class DefaultParentOperations
    {
        internal static bool IsActive(GameObject target)
        {
            if (!ManualColorOperations.IsSupported(target)) return false;
            var store = SceneMetadataStore.Find(target.scene);
            return store != null && store.IsDefaultParent(target);
        }

        internal static bool Toggle(GameObject target)
        {
            if (!ManualColorOperations.IsSupported(target)) return false;
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Toggle Default Parent");
            try
            {
                var store = SceneMetadataStore.OpenOrCreate(target.scene);
                if (store.IsDefaultParent(target)) store.ClearDefaultParent();
                else store.SetDefaultParent(target);
            }
            finally
            {
                Undo.CollapseUndoOperations(group);
                Undo.IncrementCurrentGroup();
                EditorApplication.RepaintHierarchyWindow();
            }
            return true;
        }
    }
}
