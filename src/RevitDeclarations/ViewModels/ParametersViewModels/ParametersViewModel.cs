using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using RevitDeclarations.Models;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Autodesk.Revit.DB;
using dosymep.Bim4Everyone.ProjectConfigs;
using System.Collections.ObjectModel;

namespace RevitDeclarations.ViewModels {
    internal abstract class ParametersViewModel : BaseViewModel {
        private readonly MainViewModel _mainViewModel;
        private protected readonly RevitRepository _revitRepository;

        private RevitDocumentViewModel _selectedDocument;

        private IReadOnlyCollection<Parameter> _textParameters;
        private IReadOnlyCollection<Parameter> _doubleParameters;
        private IReadOnlyCollection<Parameter> _intParameters;

        private Parameter _selectedFilterRoomsParam;
        private string _filterRoomsValue;
        private ObservableCollection<FilterRoomValueVM> _filterRoomsValues;
        private Parameter _selectedSectionRoomParam;
        private Parameter _selectedGroupRoomParam;
        private Parameter _selectedMultiStoreyParam;
        private Parameter _selectedDepartmentParam;
        private Parameter _selectedLevelParam;
        private Parameter _selectedSectionParam;
        private Parameter _selectedBuildingParam;
        private Parameter _selectedApartAreaParam;
        private Parameter _selectedApartNumParam;
        private string _projectName;
        private Parameter _selectedRoomAreaParam;
        private Parameter _selectedRoomNameParam;
        private Parameter _selectedRoomNumberParam;
        private bool _addPrefixToNumber;

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

            _filterRoomsValues = new ObservableCollection<FilterRoomValueVM>();

            AddFilterCommand = new RelayCommand(AddFilter);
            SetLastConfigCommand = new RelayCommand(SetLastParamConfig);
            SetCompanyConfigCommand = new RelayCommand(SetCompanyParamConfig);
        }

        public ICommand AddFilterCommand { get; }
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
        public ObservableCollection<FilterRoomValueVM> FilterRoomsValues {
            get => _filterRoomsValues;
            set => RaiseAndSetIfChanged(ref _filterRoomsValues, value);
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

        public Parameter SelectedDepartmentParam {
            get => _selectedDepartmentParam;
            set => RaiseAndSetIfChanged(ref _selectedDepartmentParam, value);
        }
        public Parameter SelectedApartNumParam {
            get => _selectedApartNumParam;
            set => RaiseAndSetIfChanged(ref _selectedApartNumParam, value);
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

        public Parameter SelectedApartAreaParam {
            get => _selectedApartAreaParam;
            set => RaiseAndSetIfChanged(ref _selectedApartAreaParam, value);
        }

        public string ProjectName {
            get => _projectName;
            set => RaiseAndSetIfChanged(ref _projectName, value);
        }

        public Parameter SelectedRoomAreaParam {
            get => _selectedRoomAreaParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomAreaParam, value);
        }

        public Parameter SelectedRoomNameParam {
            get => _selectedRoomNameParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomNameParam, value);
        }

        public Parameter SelectedRoomNumberParam {
            get => _selectedRoomNumberParam;
            set => RaiseAndSetIfChanged(ref _selectedRoomNumberParam, value);
        }

        public bool AddPrefixToNumber {
            get => _addPrefixToNumber;
            set => RaiseAndSetIfChanged(ref _addPrefixToNumber, value);
        }

        /// <summary>
        /// Список параметров для проверки их заполненности в View
        /// </summary>
        public abstract List<Parameter> AllSelectedParameters { get; }


        public void AddFilter(object obj) {
            if(!string.IsNullOrEmpty(_filterRoomsValue)) {
                _filterRoomsValues.Add(new FilterRoomValueVM(this, _filterRoomsValue));
            }
            FilterRoomsValue = "";
        }

        public void RemoveFilter(FilterRoomValueVM filterValue) {
            _filterRoomsValues.Remove(filterValue);
        }

        public virtual void SetLastParamConfig(object obj) { }

        public virtual void SetCompanyParamConfig(object obj) { }

        public virtual void SetParametersFromConfig(ProjectSettings configSettings) { }
    }
}
