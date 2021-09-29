using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitGenLookupTables.Models;

namespace RevitGenLookupTables.ViewModels {
    internal class FamilyViewModel : BaseViewModel {
        private readonly Family _family;
        private readonly RevitRepository _revitRepository;

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
                .Where(item => item.StorageType == StorageType.Integer || item.StorageType == StorageType.Double || item.StorageType == StorageType.String)
                .Select(item => new FamilyParamViewModel(_revitRepository, item))
                .OrderBy(item => item.Name);
        }

        private void SaveTable(object param) {
            var familyParams = FamilyParams.Where(item => !string.IsNullOrEmpty(item.FamilyParamValues.ParamValues)).ToList();

            var builder = new StringBuilder();
            foreach(var familyParam in familyParams) {
                builder.Append(";");
                foreach(var paramValue in familyParam.FamilyParamValues.GetParamValues()) {
                    builder.Append(Environment.NewLine);
                    builder.Append(paramValue);
                }
            }


            File.WriteAllText(@"D:\Users\biseuv_o\Desktop\params.csv", builder.ToString());
            Process.Start(@"D:\Users\biseuv_o\Desktop\params.csv");
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
