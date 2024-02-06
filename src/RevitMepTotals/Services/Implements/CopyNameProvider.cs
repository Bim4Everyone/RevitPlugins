using System.Linq;
using System.Text.RegularExpressions;

namespace RevitMepTotals.Services.Implements {
    internal class CopyNameProvider : ICopyNameProvider {
        public string CreateCopyName(string name, string[] existingNames) {
            string suffixStart = " - копия (";
            string suffixEnd = ")";

            string newNameStart = name + suffixStart;

            var regex = new Regex($@"^{Regex.Escape(newNameStart)}(\d+){Regex.Escape(suffixEnd)}$");
            var lastCopyNumber = existingNames
                .Select(item => regex.Match(item))
                .Where(match => match.Success)
                .Select(match => match.Groups[1].Value)
                .Select(int.Parse)
                .OrderByDescending(e => e)
                .FirstOrDefault();

            return $"{newNameStart}{++lastCopyNumber}{suffixEnd}";
        }
    }
}
