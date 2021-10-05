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

using Microsoft.Win32;

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

            SelectedFamilyParams = new ObservableCollection<FamilyParamViewModel>();
            FamilyParams = new ObservableCollection<FamilyParamViewModel>(GetFamilyParams());

            SaveTableCommand = new RelayCommand(SaveTable, CanSaveTable);
            
            AddFamilyParamCommand = new RelayCommand(AddFamilyParam, CanAddFamilyParam);
            RemoveFamilyParamCommand = new RelayCommand(RemoveFamilyParam, CanRemoveFamilyParam);

            UpFamilyParamCommand = new RelayCommand(UpFamilyParam, CanUpFamilyParam);
            DownFamilyParamCommand = new RelayCommand(DownFamilyParam, CanDownFamilyParam);

            SelectedFamilyParams.Add(FamilyParams[0]);
            SelectedFamilyParams.Add(FamilyParams[1]);
            SelectedFamilyParams.Add(FamilyParams[2]);
            SelectedFamilyParams.Add(FamilyParams[3]);
        }

        public string Name { get; }
        public ObservableCollection<FamilyParamViewModel> FamilyParams { get; }
        public ObservableCollection<FamilyParamViewModel> SelectedFamilyParams { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand SaveTableCommand { get; }

        public ICommand AddFamilyParamCommand { get; }
        public ICommand RemoveFamilyParamCommand { get; }

        public ICommand UpFamilyParamCommand { get; }
        public ICommand DownFamilyParamCommand { get; }

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

        #region SaveTableCommand

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

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Worksheets|*.csv";
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = Path.Combine(saveFileDialog.InitialDirectory, _revitRepository.DocumentName + ".csv");
            if(saveFileDialog.ShowDialog() == true) {
                File.WriteAllText(saveFileDialog.FileName, builder.ToString(), Encoding.UTF8);
                Process.Start(saveFileDialog.FileName);
            }
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

        #endregion

        #region AddFamilyParamCommand

        private void AddFamilyParam(object param) {

        }

        private bool CanAddFamilyParam(object param) {
            return FamilyParams.Count > 0;
        }

        #endregion

        #region RemoveFamilyParamCommand

        private void RemoveFamilyParam(object param) {
            SelectedFamilyParams.Remove(SelectedFamilyParam);
        }

        private bool CanRemoveFamilyParam(object param) {
            return SelectedFamilyParams.Count > 0 && SelectedFamilyParam != null;
        }

        #endregion

        #region UpFamilyParamCommand

        private void UpFamilyParam(object param) {
            int index = SelectedFamilyParams.IndexOf(SelectedFamilyParam);
            if(index > 0) {
                SelectedFamilyParams.Move(index, index - 1);
            }
        }

        private bool CanUpFamilyParam(object param) {
            return SelectedFamilyParam != null;
        }

        #endregion

        #region DownFamilyParamCommand

        private void DownFamilyParam(object param) {
            int index = SelectedFamilyParams.IndexOf(SelectedFamilyParam);
            if(index < SelectedFamilyParams.Count - 1) {
                SelectedFamilyParams.Move(index, index + 1);
            }
        }

        private bool CanDownFamilyParam(object param) {
            return SelectedFamilyParam != null;
        }

        #endregion
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
