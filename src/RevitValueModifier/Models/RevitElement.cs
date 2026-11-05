using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitValueModifier.Models;
internal class RevitElement {
    // Имена параметров в маске указываются в фигурных скобках:
    // префикс_{ФОП_Блок СМР}_суффикс1_{ФОП_Секция СМР}_суффикс2
    private static readonly Regex _paramNameInMaskRegex = new(@"{([^\}]+)}");

    private readonly ILocalizationService _localizationService;
    private readonly List<Parameter> _instanceParameters;
    private readonly List<Parameter> _typeParameters = [];

    // Значение, подготовленное для записи: string, int или double (во внутренних единицах Revit)
    private object _valueToWrite;

    public RevitElement(Element element, ILocalizationService localizationService) {
        _localizationService = localizationService;
        Elem = element;
        ElemId = element.Id;
        ElemName = Elem.Name;

        _instanceParameters = Elem.Parameters.Cast<Parameter>().ToList();
        // Пытаемся получить параметры на уровне типа элемента. Но не у всех выбранных элементов может быть тип
        // Поэтому в случае, если типа у элемента нет, то параметры у него не запрашиваем
        var elemTypeId = element.GetTypeId();
        if(elemTypeId != ElementId.InvalidElementId) {
            var elemType = element.Document.GetElement(elemTypeId);
            if(elemType is not null) {
                _typeParameters.AddRange(elemType.Parameters.Cast<Parameter>());
            }
        }
    }

    public Element Elem { get; set; }

    public ElementId ElemId { get; set; }

    public string ElemName { get; set; }

    /// <summary>
    /// Значение для отображения в окне: результат подстановки в маску
    /// или, для нетекстовых параметров, значение в том виде, в каком его покажет Revit после записи
    /// </summary>
    public string ParamValue { get; private set; } = string.Empty;

    /// <summary>
    /// Значение подготовлено и может быть записано в выбранный параметр
    /// </summary>
    public bool IsParamValueValid { get; private set; } = true;

    /// <summary>
    /// Причина, по которой значение нельзя записать
    /// </summary>
    public string ParamValueError { get; private set; } = string.Empty;

    /// <summary>
    /// Ищет параметр по имени. Параметры экземпляра имеют приоритет над параметрами типа
    /// </summary>
    public Parameter FindParameter(string paramName) {
        return _instanceParameters.FirstOrDefault(p => p.Definition.Name == paramName)
            ?? _typeParameters.FirstOrDefault(p => p.Definition.Name == paramName);
    }

    /// <summary>
    /// Ищет параметр по Id. Параметры экземпляра имеют приоритет над параметрами типа
    /// </summary>
    public Parameter FindParameter(ElementId paramId) {
        return _instanceParameters.FirstOrDefault(p => p.Id == paramId)
            ?? _typeParameters.FirstOrDefault(p => p.Id == paramId);
    }

    public void WriteParamValue(RevitParameter revitParameter) {
        var parameter = FindParameter(revitParameter.Id)
            ?? throw new Exception($"{_localizationService.GetLocalizedString("MainWindow.ParamByIdError")}" + Elem.Id.ToString());

        bool isSet;
        try {
            isSet = IsParamValueValid && (_valueToWrite switch {
                string text => parameter.Set(text),
                int integer => parameter.Set(integer),
                double number => parameter.Set(number),
                _ => false
            });
        } catch(Exception) {
            isSet = false;
        }

        if(!isSet) {
            throw new Exception($"{_localizationService.GetLocalizedString("MainWindow.SettingValueError")}" + Elem.Id.ToString());
        }
    }

    /// <summary>
    /// Подставляет значения параметров в маску и готовит значение для записи в выбранный параметр
    /// </summary>
    /// <param name="paramValueMask">Маска значения</param>
    /// <param name="targetParameter">Параметр для записи. Если null, выполняется только подстановка в маску</param>
    /// <param name="paramValueParser">Преобразователь строки в значение нужного типа</param>
    public void SetParamValue(string paramValueMask, RevitParameter targetParameter, ParamValueParser paramValueParser) {
        // Подстановка через MatchEvaluator: имя параметра не интерпретируется как регулярное выражение,
        // а подставленные значения повторно не обрабатываются
        ParamValue = _paramNameInMaskRegex.Replace(
            paramValueMask,
            match => GetValueByParameter(FindParameter(match.Groups[1].Value)));
        _valueToWrite = null;
        IsParamValueValid = true;
        ParamValueError = string.Empty;

        if(targetParameter is null) {
            return;
        }

        var parameter = FindParameter(targetParameter.Id);
        if(parameter is null) {
            SetError("MainWindow.ParamNotFound");
            return;
        }
        if(parameter.IsReadOnly) {
            SetError("MainWindow.ParamIsReadOnly");
            return;
        }

        var sourceParameter = GetSingleSourceParameter(paramValueMask);
        if(sourceParameter is not null
            && paramValueParser.TryGetExactValue(sourceParameter, targetParameter, out object exactValue)) {
            _valueToWrite = exactValue;
        } else if(paramValueParser.TryParse(ParamValue, targetParameter, out object parsedValue)) {
            _valueToWrite = parsedValue;
        } else {
            SetError("MainWindow.ValueParseError");
            return;
        }

        if(targetParameter.ValueKind != ParamValueKind.Text) {
            ParamValue = paramValueParser.Format(_valueToWrite, targetParameter);
        }
    }

    private string GetValueByParameter(Parameter parameter) {
        if(parameter is null || !parameter.HasValue) {
            return string.Empty;
        }
        string value = parameter.StorageType == StorageType.String
            ? parameter.AsString()
            : parameter.AsValueString();
        return string.IsNullOrEmpty(value) ? string.Empty : value;
    }

    /// <summary>
    /// Возвращает параметр, если маска состоит ровно из одного параметра, например "{ФОП_Цена}"
    /// </summary>
    private Parameter GetSingleSourceParameter(string paramValueMask) {
        string mask = paramValueMask.Trim();
        var match = _paramNameInMaskRegex.Match(mask);
        return match.Success && match.Length == mask.Length
            ? FindParameter(match.Groups[1].Value)
            : null;
    }

    private void SetError(string resourceKey) {
        IsParamValueValid = false;
        ParamValueError = _localizationService.GetLocalizedString(resourceKey);
    }
}
