using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitOpeningPlacement.ViewModels.Links;
internal class LinkViewModel : BaseViewModel, IEquatable<LinkViewModel> {
    private readonly RevitLinkType _linkType;
    private string _futureStatus;
    private bool _isSelected;

    public LinkViewModel(RevitLinkType linkTypeModel) {
        _linkType = linkTypeModel ?? throw new System.ArgumentNullException(nameof(linkTypeModel));

        Name = linkTypeModel.Name;
        IsCurrentlyLoaded = RevitLinkType.IsLoaded(linkTypeModel.Document, linkTypeModel.Id);
        IsSelected = IsCurrentlyLoaded;
        CurrentStatus = IsCurrentlyLoaded ? "Загружено" : "Не загружено";
    }


    public string Name { get; }

    public bool IsCurrentlyLoaded { get; }

    public bool IsSelected {
        get => _isSelected;
        set {
            RaiseAndSetIfChanged(ref _isSelected, value);
            FutureStatus = (value && !IsCurrentlyLoaded) ? "Будет загружено" : "Не изменится";
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
