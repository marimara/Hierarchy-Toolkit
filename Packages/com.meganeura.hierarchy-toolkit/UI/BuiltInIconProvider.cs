using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    internal static class BuiltInIconProvider
    {
        private static IReadOnlyList<BuiltInIcon> icons = Array.Empty<BuiltInIcon>();
        private static bool loaded;

        internal static IReadOnlyList<BuiltInIcon> Icons => icons;
        internal static string FailureMessage { get; private set; }

        internal static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            try
            {
                var method = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var bundle = method?.Invoke(null, null) as AssetBundle;
                if (bundle == null)
                {
                    FailureMessage = "The built-in Unity icon catalogue is unavailable in this Editor version.";
                    return;
                }

                var discovered = bundle.LoadAllAssets<Texture2D>();
                var uniqueNames = new HashSet<string>(StringComparer.Ordinal);
                var result = new List<BuiltInIcon>(discovered.Length);
                for (var i = 0; i < discovered.Length; ++i)
                {
                    var name = discovered[i] != null ? discovered[i].name : null;
                    if (string.IsNullOrEmpty(name) || !uniqueNames.Add(name)) continue;
                    var texture = EditorGUIUtility.FindTexture(name);
                    if (texture != null) result.Add(new BuiltInIcon(name, texture));
                }

                result.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name));
                icons = result;
                if (result.Count == 0)
                    FailureMessage = "Unity returned no resolvable built-in icons for this Editor version.";
            }
            catch
            {
                icons = Array.Empty<BuiltInIcon>();
                FailureMessage = "The built-in Unity icon catalogue could not be read in this Editor version.";
            }
        }

        internal readonly struct BuiltInIcon
        {
            internal readonly string Name;
            internal readonly Texture2D Texture;
            internal string Reference => "builtin:" + Name;

            internal BuiltInIcon(string name, Texture2D texture)
            {
                Name = name;
                Texture = texture;
            }
        }
    }
}
