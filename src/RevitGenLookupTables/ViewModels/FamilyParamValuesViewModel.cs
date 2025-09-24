using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitGenLookupTables.Models;

namespace RevitGenLookupTables.ViewModels;

internal class FamilyParamValuesViewModel : BaseViewModel {
    private readonly StorageType _storageType;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _maxValue;

    private string _minValue;
    private string _paramValues;
    private string _stepValue;

    public FamilyParamValuesViewModel(StorageType storageType, ILocalizationService localizationService) {
        _storageType = storageType;
        _localizationService = localizationService;

        MinValueEdit = "0";
        MaxValueEdit = "10";
        StepValueEdit = "1";

        CultureInfo = (CultureInfo) CultureInfo.GetCultureInfo("ru-RU").Clone();
        CultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        
        GenerateCommand = RelayCommand.Create(Generate, CanGenerate);
    }

    public string MinValueEdit {
        get => _minValue;
        set => RaiseAndSetIfChanged(ref _minValue, value);
    }

    public string MaxValueEdit {
        get => _maxValue;
        set => RaiseAndSetIfChanged(ref _maxValue, value);
    }

    public string StepValueEdit {
        get => _stepValue;
        set => RaiseAndSetIfChanged(ref _stepValue, value);
    }

    public CultureInfo CultureInfo { get; }

    public double? MinValue => GetDouble(MinValueEdit);

    public double? MaxValue => GetDouble(MaxValueEdit);

    public double? StepValue => GetDouble(StepValueEdit);

    public string ParamValues {
        get => _paramValues;
        set => RaiseAndSetIfChanged(ref _paramValues, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ICommand GenerateCommand { get; }

    public IEnumerable<string> GetParamValues() {
        return ParamValues?.Split([Environment.NewLine], StringSplitOptions.None) ?? Enumerable.Empty<string>();
    }

    public string GetValueErrors() {
        string[] values = GetParamValues().ToArray();
        if(values.Length == 0) {
            return null;
        }

        return _storageType switch {
            StorageType.Integer => AllValuesInt(values)
                ? null
                : "Значение параметров должны быть целочисленными.",
            StorageType.Double => AllValuesDouble(values)
                ? null
                : "Значение параметров должны быть вещественными.",
            _ => null
        };
    }

    private void Generate() {
        if(MinValue == null
           || MaxValue == null
           || StepValue == null) {
            return;
        }

        ParamValues = string.Join(
            Environment.NewLine,
            RangedEnumeration(MinValue.Value, MaxValue.Value, StepValue.Value).Select(ConvertValue));
    }

    private bool CanGenerate() {
        if(_storageType == StorageType.String) {
            ErrorText = "Генерация значений не доступна для строковых параметров.";
            return false;
        }

        if(_storageType is StorageType.Integer or StorageType.Double) {
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
        return _storageType == StorageType.Integer
            ? ((int) Math.Ceiling(value)).ToString()
            : value.ToString(CultureInfo);
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

        return Math.Abs(x.Value % 1) <= double.Epsilon * 100;
    }
    
    private bool AllValuesInt(string[] values) {
        return values.All(item => int.TryParse(item, out _));
    }

    private bool AllValuesDouble(string[] values) {
        return values.All(item => double.TryParse(item, NumberStyles.Number, CultureInfo, out _));
    }

    private double? GetDouble(string value) {
        return double.TryParse(value, NumberStyles.Number, CultureInfo, out double result) ? result : null;
    }
}
