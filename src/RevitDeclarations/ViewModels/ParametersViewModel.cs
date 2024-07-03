using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using RevitDeclarations.Models;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Autodesk.Revit.DB;

namespace RevitDeclarations.ViewModels {
    internal class ParametersViewModel : BaseViewModel {
        private readonly MainViewModel _mainViewModel;
        private readonly RevitRepository _revitRepository;
        private readonly ParameterToolTip _parameterToolTip;

        private RevitDocumentViewModel _selectedDocument;

        private IReadOnlyCollection<Parameter> _textParameters;
        private IReadOnlyCollection<Parameter> _doubleParameters;
        private IReadOnlyCollection<Parameter> _intParameters;

        private Parameter _selectedFilterRoomsParam;
        private string _filterRoomsValue;
        private Parameter _selectedSectionRoomParam;
        private Parameter _selectedGroupRoomParam;
        private Parameter _selectedMultiStoreyParam;

        private Parameter _selectedFullApartNumParam;
        private Parameter _selectedDepartmentParam;
        private Parameter _selectedLevelParam;
        private Parameter _selectedSectionParam;
        private Parameter _selectedBuildingParam;
        private Parameter _selectedApartNumParam;
        private Parameter _selectedApartAreaParam;
        private Parameter _selectedApartAreaCoefParam;
        private Parameter _selectedApartAreaLivingParam;
        private Parameter _selectedRoomsAmountParam;
        private string _projectName;
        private Parameter _selectedApartmentAreaNonSumParam;
        private Parameter _selectedRoomsHeightParam;

        private Parameter _selectedRoomAreaParam;
        private Parameter _selectedRoomAreaCoefParam;

        public ParametersViewModel(RevitRepository revitRepository, MainViewModel mainViewModel) {
            _mainViewModel = mainViewModel;
            _revitRepository = revitRepository;

            SelectedDocument = RevitDocuments.FirstOrDefault();

            _textParameters = _revitRepository
                .GetRoomsParamsByStorageType(SelectedDocument, StorageType.String);
            _doubleParameters = _revitRepository
                .GetRoomsParamsByStorageType(SelectedDocument, StorageType.Double);
            _intParameters = _revitRepository
                .GetRoomsParamsByStorageType(SelectedDocument, StorageType.Integer);

            _parameterToolTip = new ParameterToolTip();

            SetLastConfigCommand = new RelayCommand(SetLastParamConfig);
            SetCompanyConfigCommand = new RelayCommand(SetCompanyParamConfig);
        }

        public ICommand SetLastConfigCommand { get; }
        public ICommand SetCompanyConfigCommand { get; }

        public IList<RevitDocumentViewModel> RevitDocuments => _mainViewModel.RevitDocuments;

        public RevitDocumentViewModel SelectedDocument {
            get => _selectedDocument;
            set {
                RaiseAndSetIfChanged(ref _selectedDocument, value);
                _textParameters = _revitRepository
                    .GetRoomsParamsByStorageType(SelectedDocument, StorageType.String);
                _doubleParameters = _revitRepository
                    .GetRoomsParamsByStorageType(SelectedDocument, StorageType.Double);
                _intParameters = _revitRepository
                    .GetRoomsParamsByStorageType(SelectedDocument, StorageType.Integer);
                OnPropertyChanged(nameof(TextParameters));
                OnPropertyChanged(nameof(DoubleParameters));
                OnPropertyChanged(nameof(IntParameters));
            }
        }
        public IReadOnlyCollection<Parameter> TextParameters => _textParameters;
        public IReadOnlyCollection<Parameter> DoubleParameters => _doubleParameters;
        public IReadOnlyCollection<Parameter> IntParameters => _intParameters;

        public Parameter SelectedFilterRoomsParam {
            get => _selectedFilterRoomsParam;
            set => RaiseAndSetIfChanged(ref _selectedFilterRoomsParam, value);
        }
        public string FilterRoomsValue {
            get => _filterRoomsValue;
            set => RaiseAndSetIfChanged(ref _filterRoomsValue, value);
        }
        public Parameter SelectedGroupingBySectionParam {
            get => _selectedSectionRoomParam;
            set => RaiseAndSetIfChanged(ref _selectedSectionRoomParam, value);
        }
        public Parameter SelectedGroupingByGroupParam {
            get => _selectedGroupRoomParam;
            set => RaiseAndSetIfChanged(ref _selectedGroupRoomParam, value);
        }
        public Parameter SelectedMultiStoreyParam {
            get => _selectedMultiStoreyParam;
            set => RaiseAndSetIfChanged(ref _selectedMultiStoreyParam, value);
        }

        public Parameter SelectedFullApartNumParam {
            get => _selectedFullApartNumParam;
            set => RaiseAndSetIfChanged(ref _selectedFullApartNumParam, value);
        }
        public Parameter SelectedDepartmentParam {
            get => _selectedDepartmentParam;
            set => RaiseAndSetIfChanged(ref _selectedDepartmentParam, value);
        }
        public Parameter SelectedLevelParam {
            get => _selectedLevelParam;
            set => RaiseAndSetIfChanged(ref _selectedLevelParam, value);
        }
        public Parameter SelectedSectionParam {
            get => _selectedSectionParam;
            set => RaiseAndSetIfChanged(ref _selectedSectionParam, value);
        }
        public Parameter SelectedBuildingParam {
            get => _selectedBuildingParam;
            set => RaiseAndSetIfChanged(ref _selectedBuildingParam, value);
        }
        public Parameter SelectedApartNumParam {
            get => _selectedApartNumParam;
            set => RaiseAndSetIfChanged(ref _selectedApartNumParam, value);
        }
        public Parameter SelectedApartAreaParam {
            get => _selectedApartAreaParam;
            set => RaiseAndSetIfChanged(ref _selectedApartAreaParam, value);
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
        public string ProjectName {
            get => _projectName;
            set => RaiseAndSetIfChanged(ref _projectName, value);
        }
        public Parameter SelectedApartAreaNonSumParam {
            get => _selectedApartmentAreaNonSumParam;
            set => RaiseAndSetIfChanged(ref _selectedApartmentAreaNonSumParam, value);
        }
        public Parameter SelectedRoomsHeightParam {
            get => _selectedRoomsHeightParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomsHeightParam, value);
        }

        public Parameter SelectedRoomAreaParam {
            get => _selectedRoomAreaParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomAreaParam, value);
        }
        public Parameter SelectedRoomAreaCoefParam {
            get => _selectedRoomAreaCoefParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomAreaCoefParam, value);
        }

        public ParameterToolTip ParameterToolTip => _parameterToolTip;

        public List<Parameter> GetAllParametrs() {
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

        public void SetLastParamConfig(object obj) {
            PluginConfig config = PluginConfig.GetPluginConfig();
            RevitSettings configSettings = config.GetSettings(_revitRepository.Document);
            SetParametersFromConfig(configSettings);
        }

        public void SetCompanyParamConfig(object obj) {
            RevitSettings companyConfigSettings = new RevitSettings().GetCompanyConfig();
            SetParametersFromConfig(companyConfigSettings);
        }

        public void SetParametersFromConfig(RevitSettings configSettings) {
            SelectedFilterRoomsParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.FilterRoomsParam);
            FilterRoomsValue = configSettings.FilterRoomsValue;
            SelectedGroupingBySectionParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.GroupingBySectionParam);
            SelectedGroupingByGroupParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.GroupingByGroupParam);
            SelectedMultiStoreyParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.MultiStoreyParam);

            SelectedFullApartNumParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.ApartmentFullNumberParam);
            SelectedDepartmentParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.DepartmentParam);
            SelectedLevelParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.LevelParam);
            SelectedSectionParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.SectionParam);
            SelectedBuildingParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.BuildingParam);
            SelectedApartNumParam = TextParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.ApartmentNumberParam);
            SelectedApartAreaParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.ApartmentAreaParam);
            SelectedApartAreaCoefParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.ApartmentAreaCoefParam);
            SelectedApartAreaLivingParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.ApartmentAreaLivingParam);
            SelectedRoomsAmountParam = IntParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.RoomsAmountParam);
            ProjectName = configSettings.ProjectNameID;
            SelectedApartAreaNonSumParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.ApartmentAreaNonSumParam);
            SelectedRoomsHeightParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.RoomsHeightParam);

            SelectedRoomAreaParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.RoomAreaParam);
            SelectedRoomAreaCoefParam = DoubleParameters
                .FirstOrDefault(x => x.Definition.Name == configSettings.RoomAreaCoefParam);
        }
    }
}
