using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Meganeura.HierarchyToolkit
{
    /// <summary>Stable icon identifiers: asset GUID plus local file ID, or an explicit built-in name.</summary>
    public static class HierarchyIconReference
    {
        /// <summary>Creates a stable reference to a persistent Texture2D or Sprite, including subassets.</summary>
        public static string FromAsset(UnityEngine.Object asset)
        {
            if ((asset is not Texture2D && asset is not Sprite)
                || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId)
                || string.IsNullOrEmpty(guid))
                throw new ArgumentException("Choose a project Texture2D or Sprite asset.", nameof(asset));
            return "asset:" + guid + ":" + localId.ToString(CultureInfo.InvariantCulture);
        }

        internal static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Supply an icon reference, or use ClearIcon.");
            if (value.StartsWith("asset:", StringComparison.Ordinal) || value.StartsWith("builtin:", StringComparison.Ordinal)) return value;
            if (value.StartsWith("Assets/", StringComparison.Ordinal) || value.StartsWith("Packages/", StringComparison.Ordinal))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(value) as UnityEngine.Object
                    ?? AssetDatabase.LoadAssetAtPath<Sprite>(value);
                // A previously deleted legacy asset cannot be recovered; preserve the explicit override without a path.
                return asset != null ? FromAsset(asset) : "asset:missing:0";
            }
            return "builtin:" + value;
        }

        /// <summary>Resolves outside the Hierarchy drawing loop. Missing references return null without logging.</summary>
        public static UnityEngine.Object Resolve(string reference)
        {
            if (string.IsNullOrEmpty(reference)) return null;
            if (reference.StartsWith("builtin:", StringComparison.Ordinal))
                return EditorGUIUtility.FindTexture(reference.Substring(8));
            var parts = reference.Split(':');
            if (parts.Length != 3 || parts[0] != "asset" || !long.TryParse(parts[2], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var localId)) return null;
            var path = AssetDatabase.GUIDToAssetPath(parts[1]);
            if (string.IsNullOrEmpty(path)) return null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if ((asset is Texture2D || asset is Sprite)
                    && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out long id) && id == localId)
                    return asset;
            return null;
        }
    }
}
