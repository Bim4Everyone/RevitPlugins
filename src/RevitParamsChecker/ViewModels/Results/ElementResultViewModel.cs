using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Results;

namespace RevitParamsChecker.ViewModels.Results;

internal class ElementResultViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private string _chunkName;
    private int _itemNumber;
    private string _userMark;

    public ElementResultViewModel(ILocalizationService localization, ElementResult elementResult) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        ElementResult = elementResult ?? throw new ArgumentNullException(nameof(elementResult));
        Id = ElementResult.ElementModel.Element.Id;
        FileName = ElementResult.ElementModel.Element.Document.Title;
        FamilyTypeName = ElementResult.ElementModel.Element.Name ?? string.Empty;
        Status = _localization.GetLocalizedString($"{nameof(StatusCode)}.{ElementResult.Status}");
        Error = ElementResult.Error;
        RuleName = ElementResult.RuleName;
        CategoryName = ElementResult.ElementModel.Element.Category?.Name ?? string.Empty;
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

    public string UserMark {
        get => _userMark;
        set => RaiseAndSetIfChanged(ref _userMark, value);
    }

    public string CategoryName { get; }

    public string FileName { get; }

    public string FamilyTypeName { get; }

    public string Status { get; }

    public string Error { get; }

    public string RuleName { get; }

    public ElementResult ElementResult { get; }
}
