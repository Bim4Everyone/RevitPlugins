using System;
using System.Collections.Generic;
using System.Globalization;

using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>Записывает номера помещений и номер основного машино-места у зависимых.</summary>
internal sealed class ParamSetter(
    RevitRepository revitRepository,
    IParamService paramService,
    ILocalizationService localizationService) : IParamSetter {
    public void SetParams(IReadOnlyList<SpatialPointIntersection> intersections, ConfigSettings settings) {
        var numberParam = settings.NumberParam!;
        double number = settings.StartNumber!.Value;
        string prefix = settings.Prefix ?? string.Empty;
        string suffix = settings.Suffix ?? string.Empty;
        string searchKey = settings.SearchKey!;

        using var transaction = revitRepository.Document.StartTransaction(
            localizationService.GetLocalizedString("ParamSetter.TransactionName"));
        string? previousNumber = null;

        foreach(var intersection in intersections) {
            if(intersection.SpatialModel.SpatialElement is not Room room) {
                continue;
            }

            bool isDependent = previousNumber is not null
                && paramService.TryGetParam(settings.DependentSearchParam!, room, out var searchParam)
                && searchParam?.AsString()?.IndexOf(
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
