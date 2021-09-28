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

        private double? _minValue;
        private double? _maxValue;
        private double? _stepValue;
        private string _paramValues;
        private string _errorText;

        public FamilyParamValuesViewModel(RevitRepository revitRepository, StorageType storageType) {
            _revitRepository = revitRepository;
            _storageType = storageType;

            GenerateCommand = new RelayCommand(Generate, CanGenerate);

            MinValue = 0;
            MaxValue = 10;
            StepValue = 1;
        }

        public double? MinValue {
            get => _minValue;
            set => this.RaiseAndSetIfChanged(ref _minValue, value);
        }

        public double? MaxValue {
            get => _maxValue;
            set => this.RaiseAndSetIfChanged(ref _maxValue, value);
        }

        public double? StepValue {
            get => _stepValue;
            set => this.RaiseAndSetIfChanged(ref _stepValue, value);
        }

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
            return ParamValues?.Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
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
            ParamValues = string.Join(Environment.NewLine, RangedEnumeration(MinValue.Value, MaxValue.Value, StepValue.Value).Select(item => ConvertValue(item)));
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
                if(!IsWholeNumber(MinValue.Value)) {
                    ErrorText = "Минимальное значение должно быть целым.";
                    return false;
                }

                if(!IsWholeNumber(MaxValue.Value)) {
                    ErrorText = "Максимальное значение должно быть целым.";
                    return false;
                }

                if(!IsWholeNumber(StepValue.Value)) {
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

            if(i != max || i == max) {
                yield return max;
            }
        }

        private static bool IsWholeNumber(double x) {
            return Math.Abs(x % 1) < double.Epsilon;
        }
    }
}
