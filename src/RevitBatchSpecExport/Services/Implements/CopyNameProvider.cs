using System.Linq;
using System.Text.RegularExpressions;

using dosymep.SimpleServices;

namespace RevitBatchSpecExport.Services.Implements;
internal class CopyNameProvider : ICopyNameProvider {
    private readonly ILocalizationService _localizationService;

    public CopyNameProvider(ILocalizationService localizationService) {
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
    }

    public string CreateCopyName(string name, string[] existingNames) {
        string suffixStart = $" - {_localizationService.GetLocalizedString("Copy")} (";
        const string suffixEnd = ")";

        string newNameStart = name + suffixStart;

        var regex = new Regex($@"^{Regex.Escape(newNameStart)}(\d+){Regex.Escape(suffixEnd)}$");
        int lastCopyNumber = existingNames
            .Select(item => regex.Match(item))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .Select(int.Parse)
            .OrderByDescending(e => e)
            .FirstOrDefault();

        return $"{newNameStart}{++lastCopyNumber}{suffixEnd}";
    }
}
