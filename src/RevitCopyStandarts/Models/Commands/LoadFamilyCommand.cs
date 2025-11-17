using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;
internal class LoadFamilyCommand : ICopyStandartsCommand {
    private readonly Document _source;
    private readonly Document _target;
    private readonly ILocalizationService _localizationService; 
    
    public LoadFamilyCommand(Document source, Document target, ILocalizationService localizationService) {
        _source = source;
        _target = target;
        _localizationService = localizationService;
    }

    public string Path { get; set; }

    public void Execute() {
        using var transaction = new Transaction(_target);
        transaction.BIMStart(_localizationService.GetLocalizedString("LoadFamilyCommandTransaction"));

        var loadOptions = new FamilyLoadOptions();
        _target.LoadFamily(Path, loadOptions, out _);
        transaction.Commit();
    }
}

internal class FamilyLoadOptions : IFamilyLoadOptions {
    public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues) {
        overwriteParameterValues = true;
        return true;
    }

    public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, 
                                    out FamilySource source, out bool overwriteParameterValues) {
        overwriteParameterValues = true;
        source = FamilySource.Family;
        return true;
    }
}
