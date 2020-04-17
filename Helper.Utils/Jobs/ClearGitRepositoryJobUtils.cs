using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Utils.Jobs
{
    public static class ClearGitRepositoryJobUtils
    {
        private static readonly string[] BranchStopWords =
        {
            "release", "production", "master", "rc", "head", "prelive"
        };

        public static bool SkipBranchByName(string branchName)
        {
            var parts = branchName.ToLowerInvariant().Replace("refs/remotes/origin/", string.Empty).Split('/');

            if (parts.Length == 1 && parts[0].StartsWith("r"))
            {
                var sb = new StringBuilder();
                foreach (var ch in parts[0])
                    if (!char.IsDigit(ch) && !char.IsPunctuation(ch))
                        sb.Append(ch);
                if (sb.ToString() == "r")
                    return true;
            }

            var words = new List<string>();
            foreach (var part in parts)
            {
                var parts2 = part.Split('-', '_', '.');
                foreach (var part2 in parts2)
                    if (part2.Length > 2)
                    {
                        if (BranchStopWords.Contains(part2))
                            if (!part.StartsWith(part2))
                                continue;

                        if (!words.Contains(part2))
                            words.Add(part2);
                    }
            }

            if (words.Any() && words.TrueForAll(w => BranchStopWords.Any(w.StartsWith)))
                return true;

            return false;
        }
    }
}
