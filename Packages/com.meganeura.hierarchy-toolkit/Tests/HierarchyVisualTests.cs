using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Meganeura.HierarchyToolkit.Tests
{
    public sealed class HierarchyVisualTests
    {
        private static HierarchyLinesFeature.BranchCache BuildBranches(params int[] depths)
        {
            var cache = new HierarchyLinesFeature.BranchCache();
            foreach (var depth in depths) cache.Add(depth);
            cache.Complete();
            return cache;
        }

        [Test]
        public void OnlyChildEndsBranchEvenWhenItHasDescendants()
        {
            var cache = BuildBranches(0, 1, 2, 3);
            for (var i = 0; i < cache.Count; ++i)
                Assert.That(cache.HasFollowingSibling(i), Is.False);
        }

        [Test]
        public void FirstAndMiddleSiblingsContinueAcrossExpandedSubtrees()
        {
            var cache = BuildBranches(0, 1, 2, 2, 1, 2, 1);
            Assert.That(cache.HasFollowingSibling(1), Is.True);
            Assert.That(cache.HasFollowingSibling(4), Is.True);
            Assert.That(cache.HasFollowingSibling(6), Is.False);
            Assert.That(cache.HasFollowingSibling(2), Is.True);
            Assert.That(cache.HasFollowingSibling(3), Is.False);
            Assert.That(cache.HasFollowingSibling(5), Is.False);
        }

        [Test]
        public void AncestorsContinueOnlyWhereTheirOwnSiblingsFollow()
        {
            var cache = BuildBranches(0, 1, 2, 3, 4, 3, 1);
            var ancestor = cache.Parent(4);
            Assert.That(cache.HasFollowingSibling(ancestor), Is.True);
            ancestor = cache.Parent(ancestor);
            Assert.That(cache.HasFollowingSibling(ancestor), Is.False);
            ancestor = cache.Parent(ancestor);
            Assert.That(cache.HasFollowingSibling(ancestor), Is.True);
        }

        [Test]
        public void CollapsedSiblingsAndSeparateRootsHaveIndependentEndings()
        {
            var cache = BuildBranches(0, 1, 1, 2, 0, 1);
            Assert.That(cache.HasFollowingSibling(1), Is.True);
            Assert.That(cache.HasFollowingSibling(2), Is.False);
            Assert.That(cache.HasFollowingSibling(3), Is.False);
            Assert.That(cache.Parent(5), Is.EqualTo(4));
        }

        [Test]
        public void InvalidatedCacheRebuildsAfterReparentingWithUnchangedRowCount()
        {
            var cache = BuildBranches(0, 1, 1);
            Assert.That(cache.HasFollowingSibling(1), Is.True);
            cache.Invalidate();
            Assert.That(cache.Dirty, Is.True);
            cache.Clear();
            cache.Add(0);
            cache.Add(1);
            cache.Add(2);
            cache.Complete();
            Assert.That(cache.Dirty, Is.False);
            Assert.That(cache.HasFollowingSibling(1), Is.False);
            Assert.That(cache.Parent(2), Is.EqualTo(1));
        }

        [Test]
        public void BootstrapRegistersVisualFeaturesOnceInLayerOrder()
        {
            var zebra = HierarchyToolkitBootstrap.ZebraStriping;
            var lines = HierarchyToolkitBootstrap.HierarchyLines;
            var field = typeof(HierarchyDrawer).GetField("features", BindingFlags.NonPublic | BindingFlags.Static);
            var features = (IHierarchyFeature[])field.GetValue(null);
            var zebraIndex = Array.IndexOf(features, zebra);
            var linesIndex = Array.IndexOf(features, lines);
            Assert.That(zebraIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(features[zebraIndex + 1], Is.TypeOf<ManualColorFeature>());
            Assert.That(linesIndex, Is.EqualTo(zebraIndex + 2));
            Assert.That(features[linesIndex + 1], Is.TypeOf<ManualIconFeature>());
            HierarchyDrawer.Register(zebra);
            HierarchyDrawer.Register(lines);
            Assert.That(field.GetValue(null), Is.SameAs(features), "Duplicate registration must not allocate or duplicate drawing.");
        }

        [Test]
        public void ZebraUsesVisibleIndexAndPreservesSelection()
        {
            using var cache = new ManualColorCache();
            var zebra = new ZebraStripingFeature(cache);
            Assert.That(zebra.ShouldDraw(default, -1, false), Is.False);
            Assert.That(zebra.ShouldDraw(default, 0, false), Is.False);
            Assert.That(zebra.ShouldDraw(default, 1, false), Is.True);
            Assert.That(zebra.ShouldDraw(default, 2, false), Is.False);
            Assert.That(zebra.ShouldDraw(default, 101, false), Is.True);
            Assert.That(zebra.ShouldDraw(default, 1, true), Is.False);
            zebra.Enabled = false;
            Assert.That(zebra.ShouldDraw(default, 1, false), Is.False);
        }

        [Test]
        public void TogglesNotifyOnlyOnChangeAndRemainIndependent()
        {
            using var cache = new ManualColorCache();
            var zebra = new ZebraStripingFeature(cache);
            var lines = new HierarchyLinesFeature();
            var zebraChanges = 0;
            var lineChanges = 0;
            zebra.Changed += () => ++zebraChanges;
            lines.Changed += () => ++lineChanges;
            zebra.Enabled = false;
            zebra.Enabled = false;
            Assert.That(lines.Enabled, Is.True);
            lines.Enabled = false;
            lines.Enabled = true;
            Assert.That(zebraChanges, Is.EqualTo(1));
            Assert.That(lineChanges, Is.EqualTo(2));
        }

        [TestCase(1, 44f, 58f, 14f)]
        [TestCase(2, 44f, 72f, 14f)]
        [TestCase(200, 44f, 2844f, 14f)]
        [TestCase(3, 10f, 58f, 16f)]
        public void IndentationFollowsNativeSpacingAndDeepNesting(int depth, float gutter, float content, float expected)
        {
            Assert.That(HierarchyLinesFeature.TryGetIndent(depth, gutter, content, out var indent), Is.True);
            Assert.That(indent, Is.EqualTo(expected));
            var toggleCenter = content + 9f;
            Assert.That(HierarchyLinesFeature.GuideX(toggleCenter, indent, 1), Is.LessThan(content - 2f));
            Assert.That(HierarchyLinesFeature.GuideX(toggleCenter, indent, depth - 1),
                Is.EqualTo(gutter + indent + 9f).Within(0.001f));
        }

        [TestCase(0, 44f, 44f)]
        [TestCase(8, 44f, 44f)]
        [TestCase(8, 44f, 30f)]
        [TestCase(2, 44f, 200f)]
        public void RootsAndUnknownOrFlattenedLayoutsDoNotProduceGuides(int depth, float gutter, float content)
            => Assert.That(HierarchyLinesFeature.TryGetIndent(depth, gutter, content, out _), Is.False);
    }

    public sealed partial class SceneMetadataStoreTests
    {
        [Test]
        public void ManualColorSuppressesZebraAndClearingRestoresIt()
        {
            using var cache = new ManualColorCache();
            var zebra = new ZebraStripingFeature(cache);
            var id = target.GetEntityId();
            cache.Rebuild();
            Assert.That(zebra.ShouldDraw(id, 1, false), Is.True);
            store.SetColor(target, Color.red);
            cache.Rebuild();
            Assert.That(zebra.ShouldDraw(id, 1, false), Is.False);
            // A transparent explicit override still has precedence.
            store.SetColor(target, Color.clear);
            cache.Rebuild();
            Assert.That(zebra.ShouldDraw(id, 1, false), Is.False);
            store.ClearColor(target);
            cache.Rebuild();
            Assert.That(zebra.ShouldDraw(id, 1, false), Is.True);
        }
    }
}
