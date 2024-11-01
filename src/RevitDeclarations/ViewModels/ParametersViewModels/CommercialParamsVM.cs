using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;

using RevitDeclarations.Models;
using RevitDeclarations.Models.Configs;

namespace RevitDeclarations.ViewModels {
    internal class CommercialParamsVM : ParametersViewModel {
        private Parameter _selectedGroupNameParam;
        private bool _addPostfixToNumber;
        private Parameter _selectedBuildingNumberParam;
        private Parameter _selectedConstrWorksNumberParam;
        private Parameter _selectedRoomsHeightParam;

        public CommercialParamsVM(RevitRepository revitRepository, MainViewModel mainViewModel)
            : base(revitRepository, mainViewModel) {
            
        }

        public bool AddPostfixToNumber {
            get => _addPostfixToNumber;
            set => RaiseAndSetIfChanged(ref _addPostfixToNumber, value);
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
            FilterRoomsValue = commercialConfigSettings.FilterRoomsValue;
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

            SelectedRoomAreaParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.RoomAreaParam);
            SelectedGroupNameParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == commercialConfigSettings.GroupNameParam);

            AddPostfixToNumber = commercialConfigSettings.AddPostfixToNumber;
        }
    }
}
