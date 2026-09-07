using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit
{
    // Row misses only enqueue work. Component collection and icon resolution happen in delayCall.
    internal sealed class ComponentMinimapCache : IDisposable
    {
        internal sealed class Icon
        {
            internal readonly Type Type;
            internal readonly string Name;
            internal readonly Texture Texture;
            internal Icon(Type type, Texture texture) { Type = type; Name = type.Name; Texture = texture; }
        }

        private sealed class Entry
        {
            internal GameObject Target;
            internal Icon[] Icons = Array.Empty<Icon>();
            internal bool Dirty = true;
        }

        private readonly Dictionary<EntityId, Entry> entries = new();
        private readonly Dictionary<Type, Icon> icons = new();
        private readonly List<Component> components = new();
        private readonly HashSet<Type> seen = new();
        private readonly List<Icon> collected = new();
        private readonly List<EntityId> removed = new();
        private bool scheduled;
        private bool disposed;
        internal event Action Changed;
        internal int CollectionCount { get; private set; }

        internal ComponentMinimapCache()
        {
            EditorApplication.hierarchyChanged += Invalidate;
            EditorApplication.projectChanged += InvalidateIcons;
            Undo.undoRedoPerformed += Invalidate;
            ObjectChangeEvents.changesPublished += ObjectsChanged;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorSceneManager.sceneClosed += SceneClosed;
        }

        internal static bool IsSupported(GameObject target)
            => target != null && !EditorUtility.IsPersistent(target) && target.scene.IsValid()
                && target.scene.isLoaded && (target.hideFlags & HideFlags.HideInHierarchy) == 0;

        internal Icon[] GetIcons(GameObject target)
        {
            if (disposed || !IsSupported(target)) return Array.Empty<Icon>();
            var id = target.GetEntityId();
            if (!entries.TryGetValue(id, out var entry))
            {
                entries.Add(id, entry = new Entry { Target = target });
                Schedule();
            }
            return entry.Icons;
        }

        internal void Invalidate()
        {
            if (disposed) return;
            foreach (var entry in entries.Values)
            {
                entry.Dirty = true;
                entry.Icons = Array.Empty<Icon>();
            }
            Schedule();
        }

        private void InvalidateIcons() { icons.Clear(); Invalidate(); }
        private void SceneOpened(Scene scene, OpenSceneMode mode) => Invalidate();
        private void SceneClosed(Scene scene) => Invalidate();
        private void PlayModeChanged(PlayModeStateChange state) => Invalidate();

        private void ObjectsChanged(ref ObjectChangeEventStream stream)
        {
            for (var i = 0; i < stream.length; ++i)
            {
                switch (stream.GetEventType(i))
                {
                    case ObjectChangeKind.ChangeGameObjectStructure:
                    case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                    case ObjectChangeKind.UpdatePrefabInstances:
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                        Invalidate();
                        return;
                    case ObjectChangeKind.ChangeAssetObjectProperties:
                        InvalidateIcons();
                        return;
                }
            }
        }

        private void Schedule()
        {
            if (scheduled || disposed) return;
            scheduled = true;
            EditorApplication.delayCall += Rebuild;
        }

        internal void Rebuild()
        {
            EditorApplication.delayCall -= Rebuild;
            scheduled = false;
            if (disposed) return;
            foreach (var pair in entries)
            {
                var entry = pair.Value;
                if (!IsSupported(entry.Target)) { removed.Add(pair.Key); continue; }
                if (!entry.Dirty) continue;
                entry.Target.GetComponents(components);
                ++CollectionCount;
                entry.Icons = Collect(components);
                entry.Dirty = false;
            }
            for (var i = 0; i < removed.Count; ++i) entries.Remove(removed[i]);
            removed.Clear();
            components.Clear();
            Changed?.Invoke();
        }

        internal Icon[] Collect(IList<Component> source)
        {
            seen.Clear();
            collected.Clear();
            for (var i = 0; i < source.Count; ++i)
            {
                var component = source[i];
                if (component == null || component is Transform) continue;
                var type = component.GetType();
                if (!seen.Add(type)) continue;
                if (!icons.TryGetValue(type, out var icon))
                {
                    var texture = EditorGUIUtility.ObjectContent(component, type).image;
                    if (texture == null) texture = EditorGUIUtility.ObjectContent(null, typeof(MonoScript)).image;
                    icons.Add(type, icon = new Icon(type, texture));
                }
                collected.Add(icon);
            }
            return collected.Count == 0 ? Array.Empty<Icon>() : collected.ToArray();
        }

        public void Dispose()
        {
            disposed = true;
            EditorApplication.delayCall -= Rebuild;
            EditorApplication.hierarchyChanged -= Invalidate;
            EditorApplication.projectChanged -= InvalidateIcons;
            Undo.undoRedoPerformed -= Invalidate;
            ObjectChangeEvents.changesPublished -= ObjectsChanged;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.sceneClosed -= SceneClosed;
            entries.Clear();
            icons.Clear();
            components.Clear();
            collected.Clear();
            seen.Clear();
            removed.Clear();
        }
    }
}
