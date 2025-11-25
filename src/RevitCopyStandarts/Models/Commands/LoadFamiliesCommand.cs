using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;
internal class LoadFamiliesCommand : ICopyStandartsCommand {
    private readonly List<string> _paths;
    private readonly Document _target;
    private readonly ILocalizationService _localizationService; 
    
    public LoadFamiliesCommand(List<string> paths, Document target, ILocalizationService localizationService) {
        _paths = paths;
        _target = target;
        _localizationService = localizationService;
    }

    public void Execute() {
        using var transaction = new Transaction(_target);
        transaction.BIMStart(_localizationService.GetLocalizedString("LoadFamilyCommandTransaction"));

        var loadOptions = new FamilyLoadOptions();
        foreach(var path in _paths) {
            _target.LoadFamily(path, loadOptions, out _);
        }
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
