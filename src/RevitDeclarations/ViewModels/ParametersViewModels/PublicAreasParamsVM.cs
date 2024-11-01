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
    internal class PublicAreasParamsVM : ParametersViewModel {
        private Parameter _selectedGroupNameParam;
        private bool _addPostfixToNumber;

        public PublicAreasParamsVM(RevitRepository revitRepository, MainViewModel mainViewModel)
            : base(revitRepository, mainViewModel) { }
        

        public bool AddPostfixToNumber {
            get => _addPostfixToNumber;
            set => RaiseAndSetIfChanged(ref _addPostfixToNumber, value);
        }

        public Parameter SelectedGroupNameParam {
            get => _selectedGroupNameParam;
            set => RaiseAndSetIfChanged(ref _selectedGroupNameParam, value);
        }

        public override void SetLastParamConfig(object obj) {
            PublicAreasConfig config = PublicAreasConfig.GetPluginConfig();
            PublicAreasConfigSettings configSettings = config.GetSettings(_revitRepository.Document);
            SetParametersFromConfig(configSettings);
        }

        public override void SetCompanyParamConfig(object obj) {
            PublicAreasConfigSettings companyConfigSettings = new PublicAreasConfigSettings().GetCompanyConfig();
            SetParametersFromConfig(companyConfigSettings);
        }

        public override void SetParametersFromConfig(ProjectSettings configSettings) {
            PublicAreasConfigSettings publicAreasConfigSettings = (PublicAreasConfigSettings) configSettings;
            SelectedFilterRoomsParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.FilterRoomsParam);
            FilterRoomsValue = publicAreasConfigSettings.FilterRoomsValue;
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

            SelectedRoomAreaParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == publicAreasConfigSettings.RoomAreaParam);

            AddPostfixToNumber = publicAreasConfigSettings.AddPostfixToNumber;
        }
    }
}
