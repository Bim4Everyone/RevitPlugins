using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitGenLookupTables.Models;

namespace RevitGenLookupTables.ViewModels {
    internal class FamilyParamValuesViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly StorageType _storageType;

        private string _minValue;
        private string _maxValue;
        private string _stepValue;
        private string _paramValues;
        private string _errorText;

        public FamilyParamValuesViewModel(RevitRepository revitRepository, StorageType storageType) {
            _revitRepository = revitRepository;
            _storageType = storageType;

            GenerateCommand = new RelayCommand(Generate, CanGenerate);

            MinValueEdit = "0";
            MaxValueEdit = "10";
            StepValueEdit = "1";

            CultureInfo = (CultureInfo) CultureInfo.GetCultureInfo("ru-ru").Clone();
            CultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        }

        public string MinValueEdit {
            get => _minValue;
            set => this.RaiseAndSetIfChanged(ref _minValue, value);
        }

        public string MaxValueEdit {
            get => _maxValue;
            set => this.RaiseAndSetIfChanged(ref _maxValue, value);
        }

        public string StepValueEdit {
            get => _stepValue;
            set => this.RaiseAndSetIfChanged(ref _stepValue, value);
        }
        public CultureInfo CultureInfo { get; }

        public double? MinValue => GetDouble(MinValueEdit);

        public double? MaxValue => GetDouble(MaxValueEdit);

        public double? StepValue => GetDouble(StepValueEdit);

        public string ParamValues {
            get => _paramValues;
            set => this.RaiseAndSetIfChanged(ref _paramValues, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand GenerateCommand { get; }

        public IEnumerable<string> GetParamValues() {
            return ParamValues?.Split(new[] { Environment.NewLine }, StringSplitOptions.None) ?? Enumerable.Empty<string>();
        }

        public string GetValueErrors() {
            string[] values = GetParamValues().ToArray();
            if(values.Length > 0) {
                if(_storageType == StorageType.Integer) {
                    bool result = values.All(item => int.TryParse(item, out _));
                    if(!result) {
                        return "Значение параметров должны быть целочисленными.";
                    }
                }

                if(_storageType == StorageType.Double) {
                    bool result = values.All(item => double.TryParse(item, out _) || double.TryParse(item, NumberStyles.Float, CultureInfo.InvariantCulture, out _));
                    if(!result) {
                        return "Значение параметров должны быть вещественными.";
                    }
                }
            }

            return null;
        }

        private void Generate(object param) {
            if(MinValue == null
               || MaxValue == null
               || StepValue == null) {
                return;
            }

            ParamValues = string.Join(Environment.NewLine, RangedEnumeration(MinValue.Value, MaxValue.Value, StepValue.Value)
                .Select(item => ConvertValue(item)));
        }

        private bool CanGenerate(object param) {
            if(_storageType == StorageType.String) {
                ErrorText = "Генерация значений не доступна для строковых параметров.";
                return false;
            }

            if(_storageType == StorageType.Integer
                || _storageType == StorageType.Double) {
                
                if(MinValue == null) {
                    ErrorText = "Заполните минимальное значение.";
                    return false;
                }

                if(MaxValue == null) {
                    ErrorText = "Заполните максимальное значение.";
                    return false;
                }

                if(StepValue == null) {
                    ErrorText = "Заполните значение шага.";
                    return false;
                }

                if(StepValue <= 0) {
                    ErrorText = "Значение шага должно быть неотрицательным.";
                    return false;
                }

                if(MinValue > MaxValue) {
                    ErrorText = "Минимальное значение должно быть меньше максимального.";
                    return false;
                }
            }

            if(_storageType == StorageType.Integer) {
                if(!IsWholeNumber(MinValue)) {
                    ErrorText = "Минимальное значение должно быть целым.";
                    return false;
                }

                if(!IsWholeNumber(MaxValue)) {
                    ErrorText = "Максимальное значение должно быть целым.";
                    return false;
                }

                if(!IsWholeNumber(StepValue)) {
                    ErrorText = "Значение шага должно быть целым.";
                    return false;
                }
            }

            ErrorText = null;
            return true;
        }

        private string ConvertValue(double value) {
            if(_storageType == StorageType.Integer) {
                return ((int) Math.Ceiling(value)).ToString();
            }

            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static IEnumerable<double> RangedEnumeration(double min, double max, double step) {
            double i = min;
            for(; i < max; i += step) {
                yield return i;
            }

            if(i <= max) {
                yield return max;
            }
        }

        private static bool IsWholeNumber(double? x) {
            if(x == null) {
                return false;
            }
            
            return Math.Abs(x.Value % 1) <= (double.Epsilon * 100);
        }

        private double? GetDouble(string value) {
            if(double.TryParse(value, out double result)) {
                return result;
            }
            return double.TryParse(value, NumberStyles.Number, CultureInfo, out result) ? result : (double?) null;
        }
    }
}
