using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class CommercialParamsVM : ParametersViewModel {
    private Parameter _selectedGroupNameParam;
    private Parameter _selectedBuildingNumberParam;
    private Parameter _selectedConstrWorksNumberParam;
    private Parameter _selectedRoomsHeightParam;
    private Parameter _selectedParkingSpaceClass;
    private Parameter _selectedParkingInfo;

    public CommercialParamsVM(RevitRepository revitRepository, MainViewModel mainViewModel)
        : base(revitRepository, mainViewModel) {
    }

    public Parameter SelectedGroupNameParam {
        get => _selectedGroupNameParam;
        set => RaiseAndSetIfChanged(ref _selectedGroupNameParam, value);
    }

    public Parameter SelectedBuildingNumberParam {
        get => _selectedBuildingNumberParam;
        set => RaiseAndSetIfChanged(ref _selectedBuildingNumberParam, value);
    }
    public Parameter SelectedConstrWorksNumberParam {
        get => _selectedConstrWorksNumberParam;
        set => RaiseAndSetIfChanged(ref _selectedConstrWorksNumberParam, value);
    }
    public Parameter SelectedRoomsHeightParam {
        get => _selectedRoomsHeightParam;
        set => RaiseAndSetIfChanged(ref _selectedRoomsHeightParam, value);
    }
    public Parameter SelectedParkingSpaceClass {
        get => _selectedParkingSpaceClass;
        set => RaiseAndSetIfChanged(ref _selectedParkingSpaceClass, value);
    }
    public Parameter SelectedParkingInfo {
        get => _selectedParkingInfo;
        set => RaiseAndSetIfChanged(ref _selectedParkingInfo, value);
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
                SelectedBuildingNumberParam,
                SelectedConstrWorksNumberParam,

                SelectedRoomNumberParam,
                SelectedApartAreaParam,
                SelectedRoomsHeightParam,
                SelectedParkingSpaceClass,
                SelectedParkingInfo,

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
        var config = CommercialConfig.GetPluginConfig();
        var configSettings = config.GetSettings(_revitRepository.Document);
        if(configSettings != null) {
            SetParametersFromConfig(configSettings);
        }
    }

    public override void SetCompanyParamConfig(object obj) {
        var companyConfigSettings = new CommercialConfigSettings().GetCompanyConfig();
        SetParametersFromConfig(companyConfigSettings);
    }

    public override void SetParametersFromConfig(ProjectSettings configSettings) {
        var commercialConfigSettings = (CommercialConfigSettings) configSettings;
        SelectedFilterRoomsParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.FilterRoomsParam);
        FilterRoomsValues = [.. commercialConfigSettings.FilterRoomsValues.Select(x => new FilterRoomValueVM(this, x))];
        SelectedGroupingBySectionParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.GroupingBySectionParam);
        SelectedGroupingByGroupParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.GroupingByGroupParam);
        SelectedMultiStoreyParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.MultiStoreyParam);

        SelectedDepartmentParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.DepartmentParam);
        SelectedLevelParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.LevelParam);
        SelectedSectionParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.SectionParam);
        SelectedBuildingParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.BuildingParam);
        SelectedBuildingNumberParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.BuildingNumberParam);
        SelectedConstrWorksNumberParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.ConstrWorksNumberParam);
        SelectedApartNumParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.ApartmentNumberParam);
        SelectedApartAreaParam = DoubleParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.ApartmentAreaParam);
        SelectedRoomsHeightParam = DoubleParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.RoomsHeightParam);
        SelectedParkingSpaceClass = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.ParkingSpaceClass);
        SelectedParkingInfo = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.ParkingInfo);
        ProjectName = commercialConfigSettings.ProjectNameID;

        SelectedRoomNumberParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.RoomNumberParam);
        SelectedRoomNameParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.RoomNameParam);

        SelectedRoomAreaParam = DoubleParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.RoomAreaParam);
        SelectedGroupNameParam = TextParameters
            .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.GroupNameParam);

        AddPrefixToNumber = commercialConfigSettings.AddPrefixToNumber;
    }
}
