using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
            var familyParams = FamilyParams
                .Where(item => !string.IsNullOrEmpty(item.FamilyParamValues.ParamValues))
                .ToList();

            var builder = new StringBuilder();
            builder.Append(";");
            builder.AppendLine(string.Join(";", familyParams.Select(item => item.Name + item.ColumnMetaData)));

            var combinations = familyParams
                .Select(item => item.FamilyParamValues.GetParamValues())
                .Combination();
            
            foreach(var combination in combinations) {
                builder.Append(";");
                builder.AppendLine(string.Join(";", combination));
            }

            File.WriteAllText(@"D:\Users\biseuv_o\Desktop\params.csv", builder.ToString(), Encoding.UTF8);
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

    internal static class CombinationExtensions {
        public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<IEnumerable<T>> listElements) {
            int countElements = listElements.Count();
            if(countElements > 1) {
                IEnumerable<IEnumerable<T>> combinations = Combination(listElements.Skip(1));
                foreach(T element in listElements.First()) {
                    foreach(IEnumerable<T> combination in combinations) {
                        yield return new[] { element }.Concat(combination);
                    }
                }
            } else if(countElements == 1) {
                foreach(var element in listElements.First().Select(x => new[] { x })) {
                    yield return element;
                }
            }
        }
    }
}
