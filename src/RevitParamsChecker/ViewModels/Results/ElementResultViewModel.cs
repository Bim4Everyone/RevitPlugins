using System;

using dosymep.WPF.ViewModels;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.ViewModels.Results;

internal class ElementResultViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private string _chunkName;
    private int _itemNumber;

    public ElementResultViewModel(ILocalizationService localization, ElementResult elementResult) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        ElementResult = elementResult ?? throw new ArgumentNullException(nameof(elementResult));
        Id = ElementResult.ElementModel.Element.Id;
        FileName = ElementResult.ElementModel.Element.Document.Title;
        FamilyTypeName = ElementResult.ElementModel.Element.Name ?? string.Empty;
        Status = _localization.GetLocalizedString($"{nameof(StatusCode)}.{ElementResult.Status}");
        Error = ElementResult.Error;
        RuleName = ElementResult.RuleName;
    }

    public ElementId Id { get; }

    public string ChunkName {
        get => _chunkName;
        set => RaiseAndSetIfChanged(ref _chunkName, value);
    }

    public int ItemNumber {
        get => _itemNumber;
        set => RaiseAndSetIfChanged(ref _itemNumber, value);
    }

    public string FileName { get; }

    public string FamilyTypeName { get; }

    public string Status { get; }

    public string Error { get; }

    public string RuleName { get; }

    public ElementResult ElementResult { get; }
}
