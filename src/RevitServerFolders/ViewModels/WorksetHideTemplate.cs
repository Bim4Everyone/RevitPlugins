using System;

using dosymep.WPF.ViewModels;

namespace RevitServerFolders.ViewModels;
internal class WorksetHideTemplate : BaseViewModel, IEquatable<WorksetHideTemplate> {
    private string _template;

    public WorksetHideTemplate() {
        Guid = Guid.NewGuid();
    }

    public Guid Guid { get; }

    public string Template {
        get => _template;
        set => RaiseAndSetIfChanged(ref _template, value);
    }

    public bool Equals(WorksetHideTemplate other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) {
            return true;
        }
        return Guid == other.Guid;
    }

    public override int GetHashCode() {
        return -737073652 + Guid.GetHashCode();
    }

    public override bool Equals(object obj) {
        return Equals(obj as WorksetHideTemplate);
    }
}
