using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class ActivationToggleFeature : IHierarchyFeature
    {
        private bool enabled = true;
        internal event Action Changed;
        internal bool Enabled
        {
            get => enabled;
            set { if (enabled == value) return; enabled = value; Changed?.Invoke(); }
        }

        internal static bool IsSupported(GameObject target)
            => target != null && !EditorApplication.isPlayingOrWillChangePlaymode
                && !EditorUtility.IsPersistent(target) && target.scene.IsValid() && target.scene.isLoaded
                && (target.hideFlags & (HideFlags.NotEditable | HideFlags.HideInHierarchy)) == 0
                && !EditorSceneManager.IsPreviewScene(target.scene);

        internal bool TryGetState(GameObject target, out bool active)
        {
            active = target != null && target.activeSelf;
            return Enabled && IsSupported(target);
        }

        internal void Toggle(GameObject clicked)
        {
            if (!Enabled || !IsSupported(clicked)) return;
            var active = !clicked.activeSelf;
            var selected = Selection.gameObjects;
            var candidates = new[] { clicked };
            for (var i = 0; i < selected.Length; ++i)
                if (selected[i] == clicked) { candidates = selected; break; }
            var targets = new List<GameObject>();
            foreach (var target in candidates)
                if (IsSupported(target) && target.activeSelf != active && !targets.Contains(target)) targets.Add(target);
            if (targets.Count == 0) return;
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            const string label = "Toggle Hierarchy Activation";
            Undo.SetCurrentGroupName(label);
            try
            {
                Undo.RecordObjects(targets.ToArray(), label);
                foreach (var target in targets)
                {
                    if (target == null) continue;
                    target.SetActive(active);
                    if (PrefabUtility.IsPartOfPrefabInstance(target))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
                Undo.FlushUndoRecordObjects();
            }
            finally
            {
                Undo.CollapseUndoOperations(group);
                Undo.IncrementCurrentGroup();
                Changed?.Invoke();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        // Interactive controls are owned by the existing UI Toolkit row binding.
        public void Draw(in HierarchyRowContext context) { }
    }
}
