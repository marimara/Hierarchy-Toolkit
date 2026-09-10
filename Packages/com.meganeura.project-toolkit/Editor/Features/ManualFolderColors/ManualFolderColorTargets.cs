using System;
using System.Collections.Generic;

namespace Meganeura.ProjectToolkit
{
    internal static class ManualFolderColorTargets
    {
        internal static IReadOnlyList<string> Resolve(
            string clickedGuid,
            IReadOnlyList<string> selectedGuids,
            Func<string, bool> isEligibleFolder)
        {
            if (isEligibleFolder == null)
            {
                throw new ArgumentNullException(nameof(isEligibleFolder));
            }

            List<string> targets = new List<string>();
            if (string.IsNullOrEmpty(clickedGuid))
            {
                return targets;
            }

            bool clickedBelongsToSelection = false;
            if (selectedGuids != null)
            {
                for (int index = 0; index < selectedGuids.Count; index++)
                {
                    if (string.Equals(selectedGuids[index], clickedGuid, StringComparison.Ordinal))
                    {
                        clickedBelongsToSelection = true;
                        break;
                    }
                }
            }

            if (!clickedBelongsToSelection)
            {
                if (isEligibleFolder(clickedGuid))
                {
                    targets.Add(clickedGuid);
                }

                return targets;
            }

            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < selectedGuids.Count; index++)
            {
                string selectedGuid = selectedGuids[index];
                if (!string.IsNullOrEmpty(selectedGuid)
                    && visited.Add(selectedGuid)
                    && isEligibleFolder(selectedGuid))
                {
                    targets.Add(selectedGuid);
                }
            }

            return targets;
        }
    }
}
