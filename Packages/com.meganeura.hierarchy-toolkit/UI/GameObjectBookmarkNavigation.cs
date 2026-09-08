using System;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class GameObjectBookmarkNavigation : IDisposable
    {
        private readonly GameObjectBookmarkStore store;
        private readonly Dictionary<HierarchyWindow, ToolbarButton> buttons = new();

        internal GameObjectBookmarkNavigation(GameObjectBookmarkStore store)
        {
            this.store = store;
            HierarchyWindow.BindView += Bind;
            HierarchyWindow.UnbindView += Unbind;
            EditorApplication.delayCall += BindExisting;
        }

        private void BindExisting()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<HierarchyWindow>()) Bind(window, window.View);
        }

        private void Bind(HierarchyWindow window, HierarchyView view)
        {
            if (buttons.ContainsKey(window)) return;
            var toolbar = window.rootVisualElement.Q<Toolbar>();
            if (toolbar == null) return;
            var button = new ToolbarButton(() => OpenMenu(window))
            {
                name = "hierarchy-toolkit-bookmarks",
                text = "★",
                tooltip = "GameObject bookmarks — reveal or manage personal bookmarks"
            };
            button.style.width = 26;
            button.style.minWidth = 26;
            button.style.flexShrink = 0;
            toolbar.Add(button);
            buttons.Add(window, button);
        }

        private void OpenMenu(HierarchyWindow window)
        {
            var menu = new GenericMenu();
            var entries = store.Entries;
            // Resolve only on explicit menu opening/clicking, never on repaint or row binding.
            for (var i = 0; i < entries.Count; ++i)
            {
                var entry = entries[i];
                var target = GameObjectBookmarkStore.Resolve(entry);
                var label = target != null ? target.name : entry.lastKnownName;
                var path = (i + 1) + ". " + (label ?? "Missing GameObject").Replace('/', '／');
                var tooltip = target != null ? target.name + " — " + target.scene.name : "Target is missing or its scene is unloaded";
                if (target != null) menu.AddItem(new GUIContent(path, tooltip), false, () => GameObjectBookmarkReveal.Reveal(entry, window));
                else menu.AddDisabledItem(new GUIContent(path + " (unavailable)", tooltip));
                menu.AddItem(new GUIContent("Remove/" + path, tooltip), false, () => store.Remove(entry.objectId));
            }
            if (entries.Count == 0) menu.AddDisabledItem(new GUIContent("No GameObject bookmarks"));
            menu.AddSeparator("");
            var selected = ManualColorOperations.ResolveTargets(null, Selection.gameObjects);
            if (selected.Length > 0)
                menu.AddItem(new GUIContent("Bookmark Selected GameObjects"), false, () => store.Add(null, selected));
            else menu.AddDisabledItem(new GUIContent("Bookmark Selected GameObjects", "Select a GameObject in a saved scene"));
            menu.DropDown(buttons[window].worldBound);
        }

        private void Unbind(HierarchyWindow window, HierarchyView view)
        {
            if (buttons.Remove(window, out var button)) button.RemoveFromHierarchy();
        }

        public void Dispose()
        {
            EditorApplication.delayCall -= BindExisting;
            HierarchyWindow.BindView -= Bind;
            HierarchyWindow.UnbindView -= Unbind;
            foreach (var button in buttons.Values) button.RemoveFromHierarchy();
            buttons.Clear();
        }
    }
}
