using Unity.Hierarchy;

namespace Meganeura.HierarchyToolkit
{
    internal static class HierarchyNavigation
    {
        internal static bool ToggleExpanded(HierarchyView view, HierarchyNode node)
        {
            if (!IsUsable(view, node)) return false;

            if (view.IsExpanded(node)) view.Collapse(node);
            else view.Expand(node);
            view.Update();
            return true;
        }

        internal static bool Isolate(HierarchyView view, HierarchyNode node)
        {
            if (!IsUsable(view, node)) return false;

            view.CollapseAll();
            ExpandAncestors(view, node);
            view.Expand(node);
            view.Update();
            return true;
        }

        internal static bool ExpandAncestors(HierarchyView view, HierarchyNode node)
        {
            if (!IsUsable(view, node)) return false;

            var source = view.Source;
            var parent = source.GetParent(node);
            while (parent != HierarchyNode.Null && parent != source.Root)
            {
                view.Expand(parent);
                parent = source.GetParent(parent);
            }
            return true;
        }

        private static bool IsUsable(HierarchyView view, HierarchyNode node)
            => view?.Source != null && view.Source.IsCreated && node != HierarchyNode.Null;
    }
}
