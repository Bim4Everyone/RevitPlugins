using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitCorrectNamingCheck.Models;

namespace RevitCorrectNamingCheck.ViewModels;

internal class LinkedFileViewModel : BaseViewModel {
    private bool _isPinned;
    private WorksetInfoViewModel _typeWorkset;
    private WorksetInfoViewModel _instanceWorkset;
    private NameStatus _fileNameStatus;

    public LinkedFileViewModel(LinkedFile linkedFile, ICollection<WorksetInfo> availableWorksets) {
        LinkedFile = linkedFile ?? throw new ArgumentNullException(nameof(linkedFile));
        _isPinned = LinkedFile.IsPinned;
        TypeWorkset = new WorksetInfoViewModel(LinkedFile.TypeWorkset);
        InstanceWorkset = new WorksetInfoViewModel(LinkedFile.InstanceWorkset);
        TypeWorksets = [..availableWorksets.Select(w => new WorksetInfoViewModel(w)).OrderBy(w => w.Name)];
        InstanceWorksets = [..availableWorksets.Select(w => new WorksetInfoViewModel(w)).OrderBy(w => w.Name)];
    }

    public LinkedFile LinkedFile { get; }
    public string Name => LinkedFile.Name;

    public IReadOnlyCollection<WorksetInfoViewModel> TypeWorksets { get; }
    public IReadOnlyCollection<WorksetInfoViewModel> InstanceWorksets { get; }

    public NameStatus FileNameStatus {
        get => _fileNameStatus;
        set => RaiseAndSetIfChanged(ref _fileNameStatus, value);
    }

    public bool IsPinned {
        get => _isPinned;
        set => RaiseAndSetIfChanged(ref _isPinned, value);
    }

    public WorksetInfoViewModel TypeWorkset {
        get => _typeWorkset;
        set => RaiseAndSetIfChanged(ref _typeWorkset, value);
    }

    public WorksetInfoViewModel InstanceWorkset {
        get => _instanceWorkset;
        set => RaiseAndSetIfChanged(ref _instanceWorkset, value);
    }
}
