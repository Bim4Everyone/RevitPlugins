using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitOpeningPlacement.ViewModels.Links;
internal class LinkViewModel : BaseViewModel, IEquatable<LinkViewModel> {
    private readonly RevitLinkType _linkType;
    private readonly ILocalizationService _localization;
    private string _futureStatus;
    private bool _isSelected;
    private readonly string _willBeLoaded;
    private readonly string _willNotChange;

    public LinkViewModel(RevitLinkType linkTypeModel, ILocalizationService localization) {
        _linkType = linkTypeModel ?? throw new ArgumentNullException(nameof(linkTypeModel));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        _willBeLoaded = _localization.GetLocalizedString(
            "LinksSelectorWindow.LinkWillBeLoaded");
        _willNotChange = _localization.GetLocalizedString(
            "LinksSelectorWindow.LinkWillNotChange");

        Name = linkTypeModel.Name;
        IsCurrentlyLoaded = RevitLinkType.IsLoaded(linkTypeModel.Document, linkTypeModel.Id);
        IsSelected = IsCurrentlyLoaded;
        CurrentStatus = IsCurrentlyLoaded
            ? _localization.GetLocalizedString("LinksSelectorWindow.LinkIsLoaded")
            : _localization.GetLocalizedString("LinksSelectorWindow.LinkIsNotLoaded");
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
        private set => RaiseAndSetIfChanged(ref _futureStatus, value);
    }

    public override bool Equals(object obj) {
        return Equals(obj as LinkViewModel);
    }

    public bool Equals(LinkViewModel other) {
        return other is not null && (ReferenceEquals(this, other) || _linkType.Id == other._linkType.Id);
    }

    public override int GetHashCode() {
        return -1154484602 + EqualityComparer<ElementId>.Default.GetHashCode(_linkType?.Id);
    }

    public RevitLinkType GetLinkType() {
        return _linkType;
    }
}
