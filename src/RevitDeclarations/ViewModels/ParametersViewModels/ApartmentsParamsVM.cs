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
    internal class ApartmentsParamsVM : ParametersViewModel {
        private Parameter _selectedFullApartNumParam;
        private Parameter _selectedBuildingNumberParam;
        private Parameter _selectedConstrWorksNumberParam;
        private Parameter _selectedApartAreaCoefParam;
        private Parameter _selectedApartAreaLivingParam;
        private Parameter _selectedRoomsAmountParam;
        private Parameter _selectedApartmentAreaNonSumParam;
        private Parameter _selectedRoomsHeightParam;
        private Parameter _selectedRoomAreaCoefParam;

        public ApartmentsParamsVM(RevitRepository revitRepository, MainViewModel mainViewModel)
            : base(revitRepository, mainViewModel) {
            
        }

        public Parameter SelectedFullApartNumParam {
            get => _selectedFullApartNumParam;
            set => RaiseAndSetIfChanged(ref _selectedFullApartNumParam, value);
        }

        public Parameter SelectedBuildingNumberParam {
            get => _selectedBuildingNumberParam;
            set => RaiseAndSetIfChanged(ref _selectedBuildingNumberParam, value);
        }
        public Parameter SelectedConstrWorksNumberParam {
            get => _selectedConstrWorksNumberParam;
            set => RaiseAndSetIfChanged(ref _selectedConstrWorksNumberParam, value);
        }

        public Parameter SelectedApartAreaCoefParam {
            get => _selectedApartAreaCoefParam;
            set => RaiseAndSetIfChanged(ref _selectedApartAreaCoefParam, value);
        }
        public Parameter SelectedApartAreaLivingParam {
            get => _selectedApartAreaLivingParam;
            set => RaiseAndSetIfChanged(ref _selectedApartAreaLivingParam, value);
        }
        public Parameter SelectedRoomsAmountParam {
            get => _selectedRoomsAmountParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomsAmountParam, value);
        }
        public Parameter SelectedApartAreaNonSumParam {
            get => _selectedApartmentAreaNonSumParam;
            set => RaiseAndSetIfChanged(ref _selectedApartmentAreaNonSumParam, value);
        }
        public Parameter SelectedRoomsHeightParam {
            get => _selectedRoomsHeightParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomsHeightParam, value);
        }
        public Parameter SelectedRoomAreaCoefParam {
            get => _selectedRoomAreaCoefParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomAreaCoefParam, value);
        }

        public override List<Parameter> GetAllParametrs() {
            return new List<Parameter> {
                SelectedFilterRoomsParam,
                SelectedGroupingBySectionParam,
                SelectedGroupingByGroupParam,
                SelectedMultiStoreyParam,
                SelectedFullApartNumParam,
                SelectedDepartmentParam,
                SelectedLevelParam,
                SelectedSectionParam,
                SelectedBuildingParam,
                SelectedBuildingNumberParam,
                SelectedConstrWorksNumberParam,
                SelectedApartNumParam,
                SelectedApartAreaParam,
                SelectedApartAreaCoefParam,
                SelectedApartAreaLivingParam,
                SelectedRoomsAmountParam,
                SelectedApartAreaNonSumParam,
                SelectedRoomsHeightParam,
                SelectedRoomAreaParam,
                SelectedRoomAreaCoefParam
            };
        }

        public override void SetLastParamConfig(object obj) {
            ApartmentsConfig config = ApartmentsConfig.GetPluginConfig();
            ApartmnetsConfigSettings configSettings = config.GetSettings(_revitRepository.Document);
            SetParametersFromConfig(configSettings);
        }

        public override void SetCompanyParamConfig(object obj) {
            ApartmnetsConfigSettings companyConfigSettings = new ApartmnetsConfigSettings().GetCompanyConfig();
            SetParametersFromConfig(companyConfigSettings);
        }

        public override void SetParametersFromConfig(ProjectSettings configSettings) {
            ApartmnetsConfigSettings apartsConfigSettings = (ApartmnetsConfigSettings) configSettings;
            SelectedFilterRoomsParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.FilterRoomsParam);
            FilterRoomsValue = apartsConfigSettings.FilterRoomsValue;
            SelectedGroupingBySectionParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.GroupingBySectionParam);
            SelectedGroupingByGroupParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.GroupingByGroupParam);
            SelectedMultiStoreyParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.MultiStoreyParam);

            SelectedFullApartNumParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.ApartmentFullNumberParam);
            SelectedDepartmentParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.DepartmentParam);
            SelectedLevelParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.LevelParam);
            SelectedSectionParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.SectionParam);
            SelectedBuildingParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.BuildingParam);
            SelectedBuildingNumberParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.BuildingNumberParam);
            SelectedConstrWorksNumberParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.ConstrWorksNumberParam);
            SelectedApartNumParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.ApartmentNumberParam);
            SelectedApartAreaParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.ApartmentAreaParam);
            SelectedApartAreaCoefParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.ApartmentAreaCoefParam);
            SelectedApartAreaLivingParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.ApartmentAreaLivingParam);
            SelectedRoomsAmountParam = IntParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.RoomsAmountParam);
            ProjectName = apartsConfigSettings.ProjectNameID;
            SelectedApartAreaNonSumParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.ApartmentAreaNonSumParam);
            SelectedRoomsHeightParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.RoomsHeightParam);

            SelectedRoomNumberParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.RoomNumberParam);
            SelectedRoomNameParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.RoomNameParam);
            SelectedRoomAreaParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.RoomAreaParam);
            SelectedRoomAreaCoefParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == apartsConfigSettings.RoomAreaCoefParam);
        }
    }
}
