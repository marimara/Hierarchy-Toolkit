using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal sealed class HierarchyLinesFeature : IHierarchyFeature
    {
        private const float DarkLineOpacity = 0.13f;
        private const float LightLineOpacity = 0.16f;
        private const float LineThickness = 1f;
        private const float MaximumBranchLength = 8f;
        private const float NativeControlGap = 2f;
        private const float GuideOffset = 0f;
        private const float MinimumIndent = 8f;
        private const float MaximumIndent = 32f;

        private bool enabled = true;
        internal event Action Changed;
        internal bool Enabled
        {
            get => enabled;
            set { if (enabled == value) return; enabled = value; Changed?.Invoke(); }
        }

        // Derive indentation from Unity's actual layout, including its override gutter.
        // Unknown/flattened layouts fail closed instead of drawing across native controls.
        internal static bool TryGetIndent(int depth, float gutterEnd, float contentStart, out float indent)
        {
            indent = depth > 0 ? (contentStart - gutterEnd) / depth : 0f;
            return depth > 0 && !float.IsNaN(indent) && !float.IsInfinity(indent) && indent >= MinimumIndent && indent <= MaximumIndent;
        }

        internal static float GuideX(float toggleCenter, float indent, int ancestorDistance)
            => toggleCenter - indent * ancestorDistance + GuideOffset;

        // Reused per native view. A single linear pass records visible sibling relationships;
        // descendants never have to scan forward to find the end of their parent's subtree.
        internal sealed class BranchCache
        {
            private struct Branch
            {
                internal int Parent;
                internal bool HasFollowingSibling;
            }
            private readonly List<Branch> branches = new();
            private readonly List<int> path = new();
            internal bool Dirty { get; private set; } = true;
            internal int Count => branches.Count;
            internal void Invalidate() => Dirty = true;
            internal void Clear() { branches.Clear(); path.Clear(); Dirty = true; }
            internal void Complete() => Dirty = false;
            internal int Parent(int index) => branches[index].Parent;
            internal bool HasFollowingSibling(int index) => branches[index].HasFollowingSibling;

            internal void Add(int depth)
            {
                while (path.Count <= depth) path.Add(-1);
                var parent = depth > 0 ? path[depth - 1] : -1;
                var previous = path[depth];
                if (previous >= 0 && branches[previous].Parent == parent)
                {
                    var branch = branches[previous];
                    branch.HasFollowingSibling = true;
                    branches[previous] = branch;
                }
                path[depth] = branches.Count;
                if (path.Count > depth + 1) path.RemoveRange(depth + 1, path.Count - depth - 1);
                branches.Add(new Branch { Parent = parent });
            }
        }

        internal void Draw(MeshGenerationContext context, Rect rect, int depth, BranchCache branches, int index,
            float gutterEnd, float contentStart, Rect toggle, bool filtering)
        {
            if (!Enabled || filtering || rect.height <= 0f || rect.width <= 0f
                || !TryGetIndent(depth, gutterEnd, contentStart, out var indent)) return;
            var nearest = GuideX(toggle.center.x, indent, 1);
            // Reserve the entire native control area, even for leaves with an invisible toggle.
            var end = Mathf.Min(nearest + MaximumBranchLength,
                Mathf.Min(Mathf.Min(toggle.xMin, contentStart) - NativeControlGap, rect.xMax));
            if (nearest >= end || nearest < Mathf.Max(rect.xMin, gutterEnd)) return;
            var painter = context.painter2D;
            painter.strokeColor = EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, DarkLineOpacity) : new Color(0f, 0f, 0f, LightLineOpacity);
            painter.lineWidth = LineThickness;
            painter.lineCap = LineCap.Butt;
            painter.lineJoin = LineJoin.Miter;
            painter.BeginPath();
            // Only the visible indentation is visited; no Transform or ancestor traversal.
            var branchIndex = index;
            for (var distance = 1; distance <= depth && branchIndex >= 0; ++distance)
            {
                var continues = branches.HasFollowingSibling(branchIndex);
                branchIndex = branches.Parent(branchIndex);
                if (distance > 1 && !continues) continue;
                var x = GuideX(toggle.center.x, indent, distance);
                if (x < Mathf.Max(rect.xMin, gutterEnd)) break;
                if (x > rect.xMax) continue;
                var bottom = distance == 1 && !continues ? rect.center.y : rect.yMax;
                painter.MoveTo(new Vector2(x, rect.yMin));
                painter.LineTo(new Vector2(x, bottom));
                if (distance == 1)
                {
                    // A last sibling forms one joined L; intermediate siblings form a T.
                    if (continues) painter.MoveTo(new Vector2(x, rect.center.y));
                    painter.LineTo(new Vector2(end, rect.center.y));
                }
            }
            painter.Stroke();
        }

        // The legacy context has no reliable depth/foldout bounds. Native rows supply them
        // through VisualHierarchyBinding, just as manual icons use their retained binding.
        public void Draw(in HierarchyRowContext context) { }
    }
}
