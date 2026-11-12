using System;
using System.Collections.Generic;
using System.Globalization;

using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>Записывает номера помещений и номер основного машиноместа у зависимых.</summary>
internal sealed class ParamSetter(RevitRepository revitRepository) : IParamSetter {
    public void SetParams(IReadOnlyList<SpatialPointIntersection> intersections, ConfigSettings settings) {
        if(intersections is null) {
            throw new ArgumentNullException(nameof(intersections));
        }
        if(settings is null) {
            throw new ArgumentNullException(nameof(settings));
        }

        var numberParam = settings.NumberParam
            ?? throw new ArgumentException("Не выбран параметр номера помещения.", nameof(settings));
        double number = settings.StartNumber
            ?? throw new ArgumentException("Не задан стартовый номер.", nameof(settings));
        string prefix = settings.Prefix ?? string.Empty;
        string suffix = settings.Suffix ?? string.Empty;
        string searchKey = settings.SearchKey ?? string.Empty;

        using var transaction = revitRepository.Document.StartTransaction("Нумерация помещений по линии");
        string? previousNumber = null;

        foreach(var intersection in intersections) {
            if(intersection.SpatialModel.SpatialElement is not Room room) {
                continue;
            }

            bool isDependent = previousNumber is not null
                && settings.DependentSearchParam is { } searchParam
                && settings.DependentParam is not null
                && !string.IsNullOrWhiteSpace(searchKey)
                && searchParam.GetParam(room)?.AsString()?.IndexOf(
                    searchKey, StringComparison.OrdinalIgnoreCase) >= 0;

            string currentNumber = prefix + number.ToString(CultureInfo.CurrentCulture) + suffix;
            room.SetParamValue(numberParam, currentNumber);

            if(isDependent) {
                room.SetParamValue(settings.DependentParam!, previousNumber!);
            }

            previousNumber = currentNumber;
            number++;
        }

        transaction.Commit();
    }
}
