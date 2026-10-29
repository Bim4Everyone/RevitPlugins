using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Settings;

namespace RevitDocumenter.Models.RoomDimensions;

/// <summary>
/// Определяет тип помещения по точному значению параметра назначения.
/// </summary>
internal static class RoomTypeMatcher {
    /// <summary>
    /// Возвращает тип помещения из настроек или null, если значение параметра не совпало ни с одним типом.
    /// </summary>
    public static RoomTypeSettings Match(Element room, RoomDimensioningSettings settings) {
        room.ThrowIfNull();
        settings.ThrowIfNull();

        string value = GetParameterText(room, settings.PurposeParameterName);
        if(string.IsNullOrWhiteSpace(value)) {
            return null;
        }

        value = value.Trim();
        return settings.RoomTypes.FirstOrDefault(
            t => string.Equals(t.PurposeValue.Trim(), value, StringComparison.Ordinal));
    }

    /// <summary>
    /// Возвращает значение параметра элемента как текст.
    /// </summary>
    public static string GetParameterText(Element element, string parameterName) {
        var parameter = element.LookupParameter(parameterName);
        if(parameter is null || !parameter.HasValue) {
            return null;
        }
        return parameter.StorageType == StorageType.String
            ? parameter.AsString()
            : parameter.AsValueString();
    }
}
