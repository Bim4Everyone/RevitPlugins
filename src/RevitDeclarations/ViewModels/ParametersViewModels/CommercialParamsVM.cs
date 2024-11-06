using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class CommercialParamsVM : ParametersViewModel {
        private Parameter _selectedGroupNameParam;
        private Parameter _selectedBuildingNumberParam;
        private Parameter _selectedConstrWorksNumberParam;
        private Parameter _selectedRoomsHeightParam;

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

        public override List<Parameter> AllSelectedParameters {
            get {
                var parameters =  new List<Parameter> {
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

                    SelectedApartNumParam,
                    SelectedApartAreaParam,
                    SelectedRoomsHeightParam,

                    SelectedRoomNameParam,
                    SelectedRoomAreaParam
                };

                if(AddPrefixToNumber) {
                    parameters.Add(SelectedRoomNumberParam);
                }

                return parameters;
            }
        }


        public override void SetLastParamConfig(object obj) {
            CommercialConfig config = CommercialConfig.GetPluginConfig();
            CommercialConfigSettings configSettings = config.GetSettings(_revitRepository.Document);
            SetParametersFromConfig(configSettings);
        }

        public override void SetCompanyParamConfig(object obj) {
            CommercialConfigSettings companyConfigSettings = new CommercialConfigSettings().GetCompanyConfig();
            SetParametersFromConfig(companyConfigSettings);
        }

        public override void SetParametersFromConfig(ProjectSettings configSettings) {
            CommercialConfigSettings commercialConfigSettings = (CommercialConfigSettings) configSettings;
            SelectedFilterRoomsParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.FilterRoomsParam);
            FilterRoomsValues = new ObservableCollection<FilterRoomValueVM>(
                commercialConfigSettings.FilterRoomsValues.Select(x => new FilterRoomValueVM(this, x)));
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
}
