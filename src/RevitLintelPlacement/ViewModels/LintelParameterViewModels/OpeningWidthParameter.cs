using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels.LintelParameterViewModels;

internal class OpeningWidthParameter : ILintelParameterViewModel {
    private readonly ElementInfosViewModel _elementInfos;
    private readonly RevitRepository _revitRepository;

    public OpeningWidthParameter(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
        _revitRepository = revitRepository;
        _elementInfos = elementInfos;
    }

    public void SetTo(FamilyInstance lintel, FamilyInstance elementInWall) {
        if(lintel is null) {
            throw new ArgumentNullException(nameof(lintel));
        }

        if(elementInWall is null) {
            throw new ArgumentNullException(nameof(elementInWall));
        }

        double? elementWidth =
            elementInWall.GetParamValueOrDefault<double?>(_revitRepository.LintelsCommonConfig.OpeningWidth)
            ?? elementInWall.Symbol.GetParamValueOrDefault<double?>(
                _revitRepository.LintelsCommonConfig.OpeningWidth);

        if(elementWidth == null) {
            _elementInfos.ElementInfos.Add(
                new ElementInfoViewModel(
                    elementInWall.Id,
                    InfoElement.MissingOpeningParameter.FormatMessage(
                        _revitRepository.LintelsCommonConfig.OpeningWidth)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null
                        ? _revitRepository.GetElementById(elementInWall.LevelId)?.Name
                        : null
                });
            _elementInfos.ElementInfos.Add(
                new ElementInfoViewModel(
                    lintel.Id,
                    InfoElement.UnsetLintelParamter.FormatMessage(
                        _revitRepository.LintelsCommonConfig.LintelWidth)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null
                        ? _revitRepository.GetElementById(elementInWall.LevelId)?.Name
                        : null
                });
        } else {
            lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelWidth, (double) elementWidth);
        }
    }
}
