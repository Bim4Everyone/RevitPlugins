using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class PublicAreasParamsVM : ParametersViewModel {
    private Parameter _selectedGroupNameParam;

    public PublicAreasParamsVM(RevitRepository revitRepository, MainViewModel mainViewModel)
        : base(revitRepository, mainViewModel) { }

    public Parameter SelectedGroupNameParam {
        get => _selectedGroupNameParam;
        set => RaiseAndSetIfChanged(ref _selectedGroupNameParam, value);
    }

    public override List<Parameter> AllSelectedParameters {
        get {
            var parameters = new List<Parameter> {
                SelectedFilterRoomsParam,
                SelectedGroupingBySectionParam,
                SelectedGroupingByGroupParam,
                SelectedMultiStoreyParam,

                SelectedDepartmentParam,
                SelectedLevelParam,
                SelectedSectionParam,
                SelectedBuildingParam,

                SelectedRoomNumberParam,
                SelectedApartAreaParam,

                SelectedRoomNameParam,
                SelectedRoomAreaParam
            };

            if(AddPrefixToNumber) {
                parameters.Add(SelectedApartNumParam);
            }

            return parameters;
        }
    }

    public override void SetLastParamConfig(object obj) {
        var config = PublicAreasConfig.GetPluginConfig();
        var configSettings = config.GetSettings(_revitRepository.Document);
        SetParametersFromConfig(configSettings);
    }

    public override void SetCompanyParamConfig(object obj) {
        var companyConfigSettings = new PublicAreasConfigSettings().GetCompanyConfig();
        SetParametersFromConfig(companyConfigSettings);
    }

    public override void SetParametersFromConfig(ProjectSettings configSettings) {
        var publicAreasConfigSettings = (PublicAreasConfigSettings) configSettings;
        SelectedFilterRoomsParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.FilterRoomsParam);
        FilterRoomsValues = [.. publicAreasConfigSettings.FilterRoomsValues.Select(x => new FilterRoomValueVM(this, x))];
        SelectedGroupingBySectionParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.GroupingBySectionParam);
        SelectedGroupingByGroupParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.GroupingByGroupParam);
        SelectedMultiStoreyParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.MultiStoreyParam);

        SelectedDepartmentParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.DepartmentParam);
        SelectedLevelParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.LevelParam);
        SelectedSectionParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.SectionParam);
        SelectedBuildingParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.BuildingParam);
        SelectedApartAreaParam = DoubleParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.ApartmentAreaParam);
        ProjectName = publicAreasConfigSettings.ProjectNameID;
        SelectedApartNumParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.ApartmentNumberParam);

        SelectedRoomNumberParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.RoomNumberParam);
        SelectedRoomNameParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.RoomNameParam);
        SelectedRoomAreaParam = DoubleParameters
            .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.RoomAreaParam);

        AddPrefixToNumber = publicAreasConfigSettings.AddPrefixToNumber;
    }
}
