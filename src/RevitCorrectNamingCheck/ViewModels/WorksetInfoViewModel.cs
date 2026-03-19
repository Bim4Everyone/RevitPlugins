using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitCorrectNamingCheck.Models;

namespace RevitCorrectNamingCheck.ViewModels;

internal class WorksetInfoViewModel : BaseViewModel, IEquatable<WorksetInfoViewModel> {
    private readonly WorksetInfo _worksetInfo;
    private NameStatus _worksetNameStatus;

    public WorksetInfoViewModel(WorksetInfo worksetInfo) {
        _worksetInfo = worksetInfo ?? throw new ArgumentNullException(nameof(worksetInfo));
    }

    public WorksetId Id => _worksetInfo.Id;
    public string Name => _worksetInfo.Name;

    public NameStatus WorksetNameStatus {
        get => _worksetNameStatus;
        set => RaiseAndSetIfChanged(ref _worksetNameStatus, value);
    }

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
