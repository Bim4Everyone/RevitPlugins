using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitSleeves.ViewModels.Filtration;
internal class LinkViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly RevitLinkType _revitLinkType;
    private string _futureStatus;
    private bool _isSelected;
    private readonly string _willBeLoaded;
    private readonly string _willNotChange;

    public LinkViewModel(ILocalizationService localizationService, RevitLinkType revitLinkType) {
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        _revitLinkType = revitLinkType
            ?? throw new System.ArgumentNullException(nameof(revitLinkType));

        _willBeLoaded = _localizationService.GetLocalizedString(
            "LinksSelectorWindow.LinkWillBeLoaded");
        _willNotChange = _localizationService.GetLocalizedString(
            "LinksSelectorWindow.LinkWillNotChange");

        Name = _revitLinkType.Name;
        IsCurrentlyLoaded = RevitLinkType.IsLoaded(_revitLinkType.Document, _revitLinkType.Id);
        IsSelected = IsCurrentlyLoaded;
        CurrentStatus = IsCurrentlyLoaded
            ? _localizationService.GetLocalizedString("LinksSelectorWindow.LinkIsLoaded")
            : _localizationService.GetLocalizedString("LinksSelectorWindow.LinkIsNotLoaded");
    }

    public string Name { get; }

    public bool IsCurrentlyLoaded { get; }

    public bool IsSelected {
        get => _isSelected;
        set {
            RaiseAndSetIfChanged(ref _isSelected, value);
            FutureStatus = (value && !IsCurrentlyLoaded) ? _willBeLoaded : _willNotChange;
        }
    }

    /// <summary>
    /// Статус связи на момент запуска окна
    /// </summary>
    public string CurrentStatus { get; }

    /// <summary>
    /// Статус связи, который будет задан в результате выбора пользователя
    /// </summary>
    public string FutureStatus {
        get => _futureStatus;
        set => RaiseAndSetIfChanged(ref _futureStatus, value);
    }

    public override bool Equals(object obj) {
        return Equals(obj as LinkViewModel);
    }

    public bool Equals(LinkViewModel other) {
        if(other is null) {
            return false;
        }
        if(ReferenceEquals(this, other)) {
            return true;
        }
        return _revitLinkType.Id == other._revitLinkType.Id;
    }

    public override int GetHashCode() {
        return -1154484602 + EqualityComparer<ElementId>.Default.GetHashCode(_revitLinkType?.Id);
    }

    public RevitLinkType GetLinkType() {
        return _revitLinkType;
    }
}
