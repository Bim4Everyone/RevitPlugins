using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RevitListOfSchedules.Models;
public static class PathCharValidator {
    private static readonly HashSet<char> _invalidChars = [.. Path.GetInvalidPathChars()
            .Concat(Path.GetInvalidFileNameChars())];

    public static string LegalizeString(string original) {
        return new string(original
            .Select(c => _invalidChars.Contains(c) ? '_' : c)
            .ToArray());
    }
}
