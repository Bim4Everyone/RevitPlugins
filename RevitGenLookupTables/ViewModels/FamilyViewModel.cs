using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitGenLookupTables.Models;

namespace RevitGenLookupTables.ViewModels {
    internal class FamilyViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly Autodesk.Revit.DB.Family _family;
        private FamilyParamViewModel _selectedFamilyParam;

        private string _errorText;

        public FamilyViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _family = _revitRepository.GetMainFamily();

            Name = _revitRepository.DocumentName;
            FamilyParams = new ObservableCollection<FamilyParamViewModel>(GetFamilyParams());

            SaveTableCommand = new RelayCommand(SaveTable, CanSaveTable);
        }

        public string Name { get; }
        public ObservableCollection<FamilyParamViewModel> FamilyParams { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand SaveTableCommand { get; }

        public FamilyParamViewModel SelectedFamilyParam {
            get => _selectedFamilyParam;
            set => this.RaiseAndSetIfChanged(ref _selectedFamilyParam, value);
        }

        private IEnumerable<FamilyParamViewModel> GetFamilyParams() {
            return _revitRepository.GetFamilyParams()
                .Select(item => new FamilyParamViewModel(_revitRepository, item))
                .OrderBy(item => item.Name);
        }

        private void SaveTable(object param) {

        }

        private bool CanSaveTable(object param) {
            FamilyParamViewModel familyParam = FamilyParams.FirstOrDefault(item => !string.IsNullOrEmpty(item.FamilyParamValues.GetValueErrors()));
            if(familyParam != null) {
                ErrorText = familyParam.Name + ": " + familyParam.FamilyParamValues.GetValueErrors();
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
