using System;
using System.Collections.Generic;

namespace Meganeura.HierarchyToolkit
{
    internal static class SceneSelectorModel
    {
        internal static void Build(IReadOnlyList<SceneCatalog.Entry> source, SceneFavoriteStore favorites,
            string search, List<SceneCatalog.Entry> destination, out int favoriteCount)
        {
            destination.Clear();
            for (var favoritePass = 1; favoritePass >= 0; --favoritePass)
                for (var i = 0; i < source.Count; ++i)
                {
                    var entry = source[i];
                    if ((favorites.Contains(entry.Guid) ? 1 : 0) != favoritePass || !Matches(entry, search)) continue;
                    destination.Add(entry);
                }
            favoriteCount = 0;
            while (favoriteCount < destination.Count && favorites.Contains(destination[favoriteCount].Guid)) ++favoriteCount;
        }

        private static bool Matches(SceneCatalog.Entry entry, string search) => string.IsNullOrEmpty(search)
            || entry.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
            || entry.Path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
