using System.IO;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

namespace RevitRoomExtrusion.Models;
internal class FamilyPathFinder {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;

    private readonly string _familyTemplatePath;
    private readonly string _familyName;
    private readonly string _defaultFamilyPath;
    private readonly string _corporateFamilyPath;
    private readonly string _settingsPath =
        @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101\";

    public FamilyPathFinder(
    ILocalizationService localizationService,
    RevitRepository revitRepository) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _familyTemplatePath = _revitRepository.Application.FamilyTemplatePath;
        _familyName = _localizationService.GetLocalizedString("FamilyDocument.TemplateFamilyName");
        _defaultFamilyPath = GetDefaultFamilyTemplatePath();
        _corporateFamilyPath = GetCorporateFamilyTemplatePath();
    }

    public bool CheckFamilyTemplatePath(out string path, out string fam) {
        path = _familyTemplatePath;
        fam = _familyName;
        return File.Exists(_defaultFamilyPath) || File.Exists(_corporateFamilyPath);
    }

    public string GetFamilyTemplatePath() {
        return File.Exists(_defaultFamilyPath)
        ? _defaultFamilyPath
        : _corporateFamilyPath;
    }

    private string GetDefaultFamilyTemplatePath() {
        return Path.Combine(
            _familyTemplatePath,
            _familyName);
    }

    private string GetCorporateFamilyTemplatePath() {
        return Path.Combine(
            _settingsPath,
            ModuleEnvironment.RevitVersion,
            nameof(RevitRoomExtrusion),
            "Families",
            _familyName);
    }
}
