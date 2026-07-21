using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitCorrectNamingCheck.Models;

namespace RevitCorrectNamingCheck.ViewModels;

internal class WorksetInfoViewModel : BaseViewModel, IEquatable<WorksetInfoViewModel> {
    private readonly WorksetInfo _worksetInfo;
    private readonly ILocalizationService _localization;
    private NameStatus _worksetNameStatus;

    public WorksetInfoViewModel(WorksetInfo worksetInfo, ILocalizationService localization) {
        _worksetInfo = worksetInfo ?? throw new ArgumentNullException(nameof(worksetInfo));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public WorksetId Id => _worksetInfo.Id;
    public string Name => _worksetInfo.Name;

    public NameStatus WorksetNameStatus {
        get => _worksetNameStatus;
        set {
            RaiseAndSetIfChanged(ref _worksetNameStatus, value);
            OnPropertyChanged(nameof(ToolTip));
        }
    }

    /// <summary>
    /// Локализованное описание статуса рабочего набора для всплывающей подсказки.
    /// </summary>
    public string ToolTip => _localization.GetLocalizedString(WorksetNameStatus switch {
        NameStatus.Correct => "WorksetNameStatus.WorksetCorrect",
        NameStatus.Incorrect => "WorksetNameStatus.Incorrect",
        NameStatus.PartialCorrect => "WorksetNameStatus.PartialCorrect",
        _ => "WorksetNameStatus.NoMatch"
    });

    public bool Equals(WorksetInfoViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Id.Equals(other.Id);
    }

    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        if(ReferenceEquals(this, obj)) {
            return true;
        }

        if(obj.GetType() != GetType()) {
            return false;
        }

        return Equals((WorksetInfoViewModel) obj);
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }
}
