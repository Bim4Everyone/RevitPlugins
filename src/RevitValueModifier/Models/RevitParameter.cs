using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitValueModifier.Models;
internal class RevitParameter {
    /// <param name="elementParameters">Один и тот же параметр (с одинаковым Id) у выбранных элементов и их типов</param>
    /// <param name="document">Документ Revit</param>
    public RevitParameter(IReadOnlyCollection<Parameter> elementParameters, Document document) {
        var parameter = elementParameters.First();
        Id = parameter.Id;
        // Имя берется из определения, как и при поиске параметра по имени из маски
        ParamName = parameter.Definition.Name;
        ParamStorageType = parameter.StorageType;
        ParamTypeName = GetParamTypeName(parameter, document);
        DataType = parameter.Definition.GetDataType();
        IsReadOnly = elementParameters.All(elementParameter => elementParameter.IsReadOnly);
        ParamBinding = GetParamBinding(elementParameters);
        ValueKind = GetValueKind(ParamStorageType, DataType);
        DataTypeName = GetDataTypeName(DataType)
            // У части системных текстовых параметров тип данных не определяется, подписываем их как текст
            ?? (ValueKind == ParamValueKind.Text ? GetDataTypeName(SpecTypeId.String.Text) : null);
    }

    public string ParamName { get; }
    public ElementId Id { get; }
    public RevitParamType ParamTypeName { get; }
    public StorageType ParamStorageType { get; }

    /// <summary>
    /// Параметр экземпляра или типа
    /// </summary>
    public RevitParamBinding ParamBinding { get; }

    /// <summary>
    /// Параметр доступен только для чтения у всех выбранных элементов
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Тип данных параметра (SpecTypeId). Пустой, если определить не удалось
    /// </summary>
    public ForgeTypeId DataType { get; }

    /// <summary>
    /// Вид значения, определяющий способ записи в параметр
    /// </summary>
    public ParamValueKind ValueKind { get; }

    /// <summary>
    /// Название типа данных в интерфейсе Revit (например, "Длина", "Денежная единица").
    /// Null, если определить не удалось
    /// </summary>
    public string DataTypeName { get; }

    public override string ToString() => $"{ParamName}";

    private static RevitParamBinding GetParamBinding(IReadOnlyCollection<Parameter> elementParameters) {
        bool hasTypeParameter = elementParameters.Any(IsTypeParameter);
        bool hasInstanceParameter = elementParameters.Any(elementParameter => !IsTypeParameter(elementParameter));
        return hasTypeParameter && hasInstanceParameter
            ? RevitParamBinding.InstanceAndType
            : hasTypeParameter ? RevitParamBinding.Type : RevitParamBinding.Instance;
    }

    private static bool IsTypeParameter(Parameter parameter) {
        return parameter.Element is ElementType;
    }

    private static RevitParamType GetParamTypeName(Parameter parameter, Document document) {
        if(parameter.Id.IsSystemId()) {
            return RevitParamType.SystemParameter;
        }
        if(parameter.IsShared) {
            return RevitParamType.SharedParameter;
        }
        // Необщие параметры семейства не имеют элемента ParameterElement в документе проекта
        return document.GetElement(parameter.Id) is ParameterElement
            ? RevitParamType.ProjectParameter
            : RevitParamType.FamilyParameter;
    }

    private static ParamValueKind GetValueKind(StorageType storageType, ForgeTypeId dataType) {
        return storageType switch {
            StorageType.String => ParamValueKind.Text,
            StorageType.Integer when dataType == SpecTypeId.Boolean.YesNo => ParamValueKind.YesNo,
            StorageType.Integer when dataType == SpecTypeId.Int.Integer => ParamValueKind.Integer,
            StorageType.Double when IsMeasurableSpec(dataType) => ParamValueKind.Double,
            _ => ParamValueKind.Unsupported
        };
    }

    private static bool IsMeasurableSpec(ForgeTypeId dataType) {
        if(dataType.Empty()) {
            return false;
        }
        try {
            return UnitUtils.IsMeasurableSpec(dataType);
        } catch(Autodesk.Revit.Exceptions.ArgumentException) {
            return false;
        }
    }

    private static string GetDataTypeName(ForgeTypeId dataType) {
        if(dataType.Empty()) {
            return null;
        }
        try {
            return LabelUtils.GetLabelForSpec(dataType);
        } catch(Autodesk.Revit.Exceptions.ArgumentException) {
            return null;
        }
    }
}
