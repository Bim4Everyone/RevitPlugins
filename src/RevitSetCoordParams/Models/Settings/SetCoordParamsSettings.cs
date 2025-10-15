using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;

namespace RevitSetCoordParams.Models.Settings;

internal class SetCoordParamsSettings {

    private readonly RevitRepository _revitRepository;
    private readonly SearchSettings _searchSettings;
    private readonly SharedParam _blockParam = SharedParamsConfig.Instance.BuildingWorksBlock;
    private readonly SharedParam _sectionParam = SharedParamsConfig.Instance.BuildingWorksSection;
    private readonly SharedParam _floorParam = SharedParamsConfig.Instance.BuildingWorksLevel;
    private readonly SharedParam _floorDEParam = SharedParamsConfig.Instance.BuildingWorksLevelCurrency;
    private readonly SharedParam _blockingParam = SharedParamsConfig.Instance.FixSolution;

    public SetCoordParamsSettings(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
        _searchSettings = new SearchSettings();
    }

    public List<ParamMap> Parameters => GetParameters();
    public List<Category> Categories => GetCategories();
    public int MaxDiameterSearchSphereMm => _searchSettings.MaxDiameterSearchSphereMm;
    public int StepDiameterSearchSphereMm => _searchSettings.StepDiameterSearchSphereMm;
    public bool Search => _searchSettings.Search;

    private List<ParamMap> GetParameters() {
        return [new ParamMap { SourceParam = _blockParam, TargetParam = _blockParam, LocalizationKey = "BlockParam"},
                new ParamMap { SourceParam = _sectionParam, TargetParam = _sectionParam, LocalizationKey = "SectionParam"},
                new ParamMap { SourceParam = _floorParam, TargetParam = _floorParam, LocalizationKey = "FloorParam"},
                new ParamMap { SourceParam = _floorDEParam, TargetParam = _floorDEParam, LocalizationKey = "FloorDEParam"},
                new ParamMap { TargetParam = _blockingParam, LocalizationKey = "BlockingParam"}];
    }

    private List<Category> GetCategories() {
        return new FixedCategories().GetCategories(_revitRepository.Document).ToList();
    }
}
