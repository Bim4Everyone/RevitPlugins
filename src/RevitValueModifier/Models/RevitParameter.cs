using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitValueModifier.Models;
internal class RevitParameter {
    /// <param name="parameterId">Id параметра</param>
    /// <param name="document">Документ Revit</param>
    /// <param name="sampleParameter">Параметр у одного из выбранных элементов.
    /// Нужен для определения типа данных системного параметра, может быть null</param>
    public RevitParameter(ElementId parameterId, Document document, Parameter sampleParameter) {
        if(parameterId.IsSystemId()) {
            BuiltInParameter bInParameter = (BuiltInParameter) parameterId.GetIdValue();
            ParamStorageType = document.get_TypeOfStorage(bInParameter);
            ParamName = LabelUtils.GetLabelFor(bInParameter);
            ParamTypeName = RevitParamType.SystemParameter;
            // Тип данных системного параметра можно получить только у экземпляра параметра элемента
            DataType = sampleParameter?.Definition.GetDataType() ?? new ForgeTypeId();
        } else {
            ParameterElement paramElement = (ParameterElement) document.GetElement(parameterId);
            ParamName = paramElement.Name;
            ParamStorageType = paramElement.GetStorageType();
            ParamTypeName = paramElement.IsSharedParam() ? RevitParamType.SharedParameter : RevitParamType.ProjectParameter;
            DataType = paramElement.GetDefinition().GetDataType();
        }
        Id = parameterId;
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
