using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.Win32;

using RevitGenLookupTables.Models;
using RevitGenLookupTables.Views;

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

        public ObservableCollection<FamilyParamViewModel> GridSelectedFamilyParams { get; set; } = new ObservableCollection<FamilyParamViewModel>();

        private IEnumerable<FamilyParamViewModel> GetFamilyParams() {
            return _revitRepository.GetFamilyParams()
                .Where(item => item.StorageType == StorageType.Integer || item.StorageType == StorageType.Double || item.StorageType == StorageType.String)
                .Select(item => new FamilyParamViewModel(_revitRepository, item))
                .OrderBy(item => item.Name);
        }

        #region SaveTableCommand

        private void SaveTable(object param) {
            var familyParams = SelectedFamilyParams
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
                File.WriteAllText(saveFileDialog.FileName, builder.ToString(), Encoding.GetEncoding(1251));
                Process.Start(saveFileDialog.FileName);
            }
        }

        private bool CanSaveTable(object param) {
            FamilyParamViewModel familyParam = SelectedFamilyParams.FirstOrDefault(item => !string.IsNullOrEmpty(item.FamilyParamValues.GetValueErrors()));
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
            var window = new FamilyParamsView();
            window.Owner = (Window) param;

            window.DataContext = new SelectFamilyParamsViewModel() { FamilyParams = FamilyParams };
            if(window.ShowDialog() == true) {
                var selectedParams = ((SelectFamilyParamsViewModel) window.DataContext).SelectedFamilyParams;
                foreach(var selectedParam in selectedParams.ToList()) {
                    SelectedFamilyParams.Add(selectedParam);
                    FamilyParams.Remove(selectedParam);
                }
            }
        }

        private bool CanAddFamilyParam(object param) {
            return FamilyParams.Count > 0;
        }

        #endregion

        #region RemoveFamilyParamCommand

        private void RemoveFamilyParam(object param) {
            foreach(var selectedParam in GridSelectedFamilyParams.ToList()) {
                FamilyParams.Add(selectedParam);
                SelectedFamilyParams.Remove(selectedParam);
            }
        }

        private bool CanRemoveFamilyParam(object param) {
            return SelectedFamilyParams.Count > 0 && SelectedFamilyParam != null;
        }

        #endregion

        #region UpFamilyParamCommand

        private void UpFamilyParam(object param) {
            var sortedParams = GridSelectedFamilyParams
                .Select(item => new { Param = item, Index = SelectedFamilyParams.IndexOf(item) })
                .OrderBy(item => item.Index);

            foreach(var sortedParam in sortedParams) {
                if(sortedParam.Index == 0) {
                    break;
                }
                SelectedFamilyParams.Move(sortedParam.Index, sortedParam.Index - 1);
            }
        }

        private bool CanUpFamilyParam(object param) {
            return SelectedFamilyParam != null;
        }

        #endregion

        #region DownFamilyParamCommand

        private void DownFamilyParam(object param) {
            var sortedParams = GridSelectedFamilyParams
                .Select(item => new { Param = item, Index = SelectedFamilyParams.IndexOf(item) })
                .OrderByDescending(item => item.Index);

            foreach(var sortedParam in sortedParams) {
                if(sortedParam.Index == SelectedFamilyParams.Count - 1) {
                    break;
                }
                SelectedFamilyParams.Move(sortedParam.Index, sortedParam.Index + 1);
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
