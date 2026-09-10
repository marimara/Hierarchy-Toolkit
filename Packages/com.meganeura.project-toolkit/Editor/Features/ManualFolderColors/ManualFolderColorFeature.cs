using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal sealed class ManualFolderColorFeature : IProjectFeature
    {
        private const string SetColorMenuPath = "Assets/Meganeura Project Toolkit/Set Color...";
        private const string ClearColorMenuPath = "Assets/Meganeura Project Toolkit/Clear Color";
        private static readonly Color DefaultPickerColor = new Color(0.25f, 0.58f, 0.82f, 1f);
        private static ManualFolderColorFeature activeFeature;

        private readonly FolderMetadataRepository repository;
        private readonly Action repaintProjectWindow;
        private readonly Action<Undo.UndoRedoCallback> subscribeUndoRedo;
        private readonly Action<Undo.UndoRedoCallback> unsubscribeUndoRedo;
        private Texture folderTexture;
        private ContextTarget contextTarget;
        private bool isInitialized;
        private bool isDisposed;
        private bool ownsMenuActions;

        internal ManualFolderColorFeature(FolderMetadataRepository repository)
            : this(
                repository,
                EditorApplication.RepaintProjectWindow,
                callback => Undo.undoRedoPerformed += callback,
                callback => Undo.undoRedoPerformed -= callback)
        {
        }

        internal ManualFolderColorFeature(
            FolderMetadataRepository repository,
            Action repaintProjectWindow,
            Action<Undo.UndoRedoCallback> subscribeUndoRedo,
            Action<Undo.UndoRedoCallback> unsubscribeUndoRedo)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.repaintProjectWindow = repaintProjectWindow ?? throw new ArgumentNullException(nameof(repaintProjectWindow));
            this.subscribeUndoRedo = subscribeUndoRedo ?? throw new ArgumentNullException(nameof(subscribeUndoRedo));
            this.unsubscribeUndoRedo = unsubscribeUndoRedo ?? throw new ArgumentNullException(nameof(unsubscribeUndoRedo));
        }

        public bool IsEnabled => true;

        public void Initialize()
        {
            if (isInitialized || isDisposed)
            {
                return;
            }

            isInitialized = true;
            folderTexture = EditorGUIUtility.IconContent("Folder Icon").image;
            subscribeUndoRedo(OnUndoRedo);
            if (activeFeature == null)
            {
                activeFeature = this;
                ownsMenuActions = true;
            }
        }

        public void OnProjectItemGUI(ProjectItemContext item)
        {
            CaptureContextTarget(item);

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            ManualFolderColorDrawDecision decision = GetDrawDecision(item, EditorGUIUtility.singleLineHeight);
            if (!decision.ShouldDraw || folderTexture == null)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = decision.Color;
            GUI.DrawTexture(decision.IconRect, folderTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            unsubscribeUndoRedo(OnUndoRedo);
            if (ownsMenuActions && ReferenceEquals(activeFeature, this))
            {
                activeFeature = null;
            }

            ownsMenuActions = false;
            contextTarget = default;
            folderTexture = null;
        }

        internal ManualFolderColorDrawDecision GetDrawDecision(ProjectItemContext item, float singleLineHeight)
        {
            if (!item.IsFolder
                || !repository.TryGet(item.AssetGuid, out FolderMetadataSnapshot metadata)
                || !metadata.HasColorOverride
                || !ManualFolderColorLayout.TryGetIconRect(
                    item.Rect,
                    singleLineHeight,
                    out ProjectItemPresentation presentation,
                    out Rect iconRect))
            {
                return default;
            }

            return new ManualFolderColorDrawDecision(presentation, iconRect, metadata.ColorOverride);
        }

        internal bool ApplyColor(string clickedGuid, IReadOnlyList<string> selectedGuids, Color color)
        {
            IReadOnlyList<string> targets = ManualFolderColorTargets.Resolve(
                clickedGuid,
                selectedGuids,
                repository.CanEditFolder);
            bool applied = repository.TrySetColors(targets, color);
            if (applied)
            {
                repaintProjectWindow();
            }

            return applied;
        }

        internal bool ClearColor(string clickedGuid, IReadOnlyList<string> selectedGuids)
        {
            IReadOnlyList<string> targets = ManualFolderColorTargets.Resolve(
                clickedGuid,
                selectedGuids,
                repository.CanEditFolder);
            bool cleared = repository.TryClearColors(targets);
            if (cleared)
            {
                repaintProjectWindow();
            }

            return cleared;
        }

        [MenuItem(SetColorMenuPath, false, 2000)]
        private static void SetColorFromMenu()
        {
            activeFeature?.OpenColorPicker();
        }

        [MenuItem(SetColorMenuPath, true)]
        private static bool ValidateSetColorFromMenu()
        {
            return activeFeature != null && activeFeature.ResolveCurrentTargets().Count > 0;
        }

        [MenuItem(ClearColorMenuPath, false, 2001)]
        private static void ClearColorFromMenu()
        {
            activeFeature?.ClearCurrentTargets();
        }

        [MenuItem(ClearColorMenuPath, true)]
        private static bool ValidateClearColorFromMenu()
        {
            return activeFeature != null && activeFeature.AnyCurrentTargetHasColor();
        }

        private void CaptureContextTarget(ProjectItemContext item)
        {
            Event current = Event.current;
            if (!item.IsFolder
                || current.type != EventType.ContextClick
                || !item.Rect.Contains(current.mousePosition))
            {
                return;
            }

            contextTarget = new ContextTarget(
                item.AssetGuid,
                Selection.assetGUIDs,
                GUIUtility.GUIToScreenPoint(current.mousePosition));
        }

        private void OpenColorPicker()
        {
            ContextTarget target = GetCurrentTarget();
            IReadOnlyList<string> targets = ResolveTargets(target);
            if (targets.Count == 0)
            {
                return;
            }

            Color initialColor = DefaultPickerColor;
            if (repository.TryGet(target.ClickedGuid, out FolderMetadataSnapshot metadata)
                && metadata.HasColorOverride)
            {
                initialColor = metadata.ColorOverride;
            }

            contextTarget = default;
            ManualFolderColorPickerWindow.Show(
                target.ScreenPosition,
                initialColor,
                color =>
                {
                    if (repository.TrySetColors(targets, color))
                    {
                        repaintProjectWindow();
                    }
                });
        }

        private void ClearCurrentTargets()
        {
            IReadOnlyList<string> targets = ResolveCurrentTargets();
            contextTarget = default;
            if (repository.TryClearColors(targets))
            {
                repaintProjectWindow();
            }
        }

        private bool AnyCurrentTargetHasColor()
        {
            IReadOnlyList<string> targets = ResolveCurrentTargets();
            for (int index = 0; index < targets.Count; index++)
            {
                if (repository.TryGet(targets[index], out FolderMetadataSnapshot metadata)
                    && metadata.HasColorOverride)
                {
                    return true;
                }
            }

            return false;
        }

        private IReadOnlyList<string> ResolveCurrentTargets()
        {
            return ResolveTargets(GetCurrentTarget());
        }

        private IReadOnlyList<string> ResolveTargets(ContextTarget target)
        {
            return ManualFolderColorTargets.Resolve(
                target.ClickedGuid,
                target.SelectedGuids,
                repository.CanEditFolder);
        }

        private ContextTarget GetCurrentTarget()
        {
            if (!string.IsNullOrEmpty(contextTarget.ClickedGuid))
            {
                return contextTarget;
            }

            string activeGuid = Selection.activeObject == null
                ? string.Empty
                : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));
            Vector2 screenPosition = GUIUtility.GUIToScreenPoint(Event.current?.mousePosition ?? Vector2.zero);
            return new ContextTarget(activeGuid, Selection.assetGUIDs, screenPosition);
        }

        private void OnUndoRedo()
        {
            repaintProjectWindow();
        }

        private readonly struct ContextTarget
        {
            internal ContextTarget(string clickedGuid, string[] selectedGuids, Vector2 screenPosition)
            {
                ClickedGuid = clickedGuid;
                SelectedGuids = selectedGuids ?? Array.Empty<string>();
                ScreenPosition = screenPosition;
            }

            internal string ClickedGuid { get; }

            internal string[] SelectedGuids { get; }

            internal Vector2 ScreenPosition { get; }
        }
    }
}
