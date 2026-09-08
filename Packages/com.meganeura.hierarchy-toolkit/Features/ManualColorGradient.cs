using UnityEngine;
using UnityEngine.UIElements;

namespace Meganeura.HierarchyToolkit
{
    internal static class ManualColorGradient
    {
        internal static float StartAlpha => QuickStylePaletteSettings.instance.GradientStartAlpha;
        internal static float EndAlpha => QuickStylePaletteSettings.instance.GradientEndAlpha;

        internal static float Alpha(float horizontalPosition)
            => Mathf.Lerp(StartAlpha, EndAlpha, Mathf.Clamp01(horizontalPosition));

        internal static void Draw(MeshGenerationContext context, Rect rect, Color source)
        {
            if (rect.width <= 0f || rect.height <= 0f || source.a <= 0f) return;
            var left = source;
            var right = source;
            left.a *= StartAlpha;
            right.a *= EndAlpha;
            if (left.a <= 0f && right.a <= 0f) return;

            var mesh = context.Allocate(4, 6);
            var vertex = new Vertex
            {
                tint = left,
                uv = Vector2.zero
            };
            vertex.position = new Vector3(rect.xMin, rect.yMin, Vertex.nearZ);
            mesh.SetNextVertex(vertex);
            vertex.position = new Vector3(rect.xMax, rect.yMin, Vertex.nearZ);
            vertex.tint = right;
            mesh.SetNextVertex(vertex);
            vertex.position = new Vector3(rect.xMax, rect.yMax, Vertex.nearZ);
            mesh.SetNextVertex(vertex);
            vertex.position = new Vector3(rect.xMin, rect.yMax, Vertex.nearZ);
            vertex.tint = left;
            mesh.SetNextVertex(vertex);
            mesh.SetNextIndex(0);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(3);
            mesh.SetNextIndex(0);
        }
    }
}
