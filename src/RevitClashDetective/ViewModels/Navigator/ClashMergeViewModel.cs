using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Services.ReportsMerging;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashMergeViewModel : BaseViewModel {
    private static readonly ClashCommentContentComparer _comparer = new();
    private string _clashName;
    private ClashStatus _clashStatus;
    private bool _existingNameSelected;
    private bool _importingNameSelected;
    private bool _existingStatusSelected;
    private bool _importingStatusSelected;
    private bool _existingCommentsSelected;
    private bool _importingCommentsSelected;
    private int _mergedCommentsCount;
    private bool _importingClashSelected;
    private bool _existingClashSelected;

    public ClashMergeViewModel(ClashViewModel existing, ClashViewModel importing) {
        ExistingClash = existing ?? throw new ArgumentNullException(nameof(existing));
        ImportingClash = importing ?? throw new ArgumentNullException(nameof(importing));

        // по умолчанию комментарии всегда объединяются
        ExistingCommentsSelected = true;
        ImportingCommentsSelected = true;
        SetMergedComments();

        SetMergedCommentsCommand = RelayCommand.Create(SetMergedComments);
    }

    public ClashViewModel ExistingClash { get; }
    public ClashViewModel ImportingClash { get; }
    public ICommand SetMergedCommentsCommand { get; }

    public string ClashName {
        get => _clashName;
        set => RaiseAndSetIfChanged(ref _clashName, value);
    }

    public ClashStatus ClashStatus {
        get => _clashStatus;
        set => RaiseAndSetIfChanged(ref _clashStatus, value);
    }

    public bool ExistingNameSelected {
        get => _existingNameSelected;
        set {
            if(value) {
                ClashName = ExistingClash.ClashName;
                ImportingNameSelected = false;
            }

            RaiseAndSetIfChanged(ref _existingNameSelected, value);
        }
    }

    public bool ImportingNameSelected {
        get => _importingNameSelected;
        set {
            if(value) {
                ClashName = ImportingClash.ClashName;
                ExistingNameSelected = false;
            }

            RaiseAndSetIfChanged(ref _importingNameSelected, value);
        }
    }

    public bool ExistingStatusSelected {
        get => _existingStatusSelected;
        set {
            if(value) {
                ClashStatus = ExistingClash.ClashStatus;
                ImportingStatusSelected = false;
            }

            RaiseAndSetIfChanged(ref _existingStatusSelected, value);
        }
    }

    public bool ImportingStatusSelected {
        get => _importingStatusSelected;
        set {
            if(value) {
                ClashStatus = ImportingClash.ClashStatus;
                ExistingStatusSelected = false;
            }

            RaiseAndSetIfChanged(ref _importingStatusSelected, value);
        }
    }

    public bool ExistingCommentsSelected {
        get => _existingCommentsSelected;
        set => RaiseAndSetIfChanged(ref _existingCommentsSelected, value);
    }

    public bool ImportingCommentsSelected {
        get => _importingCommentsSelected;
        set => RaiseAndSetIfChanged(ref _importingCommentsSelected, value);
    }

    public bool ExistingClashSelected {
        get => _existingClashSelected;
        set {
            ExistingNameSelected = value;
            ExistingStatusSelected = value;
            ExistingCommentsSelected = value;
            RaiseAndSetIfChanged(ref _existingClashSelected, value);
        }
    }

    public bool ImportingClashSelected {
        get => _importingClashSelected;
        set {
            ImportingNameSelected = value;
            ImportingStatusSelected = value;
            ImportingCommentsSelected = value;
            RaiseAndSetIfChanged(ref _importingClashSelected, value);
        }
    }

    public ObservableCollection<ClashCommentViewModel> Comments { get; }

    public int MergedCommentsCount {
        get => _mergedCommentsCount;
        private set => RaiseAndSetIfChanged(ref _mergedCommentsCount, value);
    }

    private void SetMergedComments() {
        if(ExistingCommentsSelected && ImportingCommentsSelected) {
            var comments = ExistingClash.Comments.Union(ImportingClash.Comments, _comparer).ToArray();
            SetMergedComments(comments);
            return;
        }

        if(ExistingCommentsSelected) {
            SetMergedComments(ExistingClash.Comments);
            return;
        }

        if(ImportingCommentsSelected) {
            SetMergedComments(ImportingClash.Comments);
            return;
        }
    }

    private void SetMergedComments(ICollection<ClashCommentViewModel> comments) {
        Comments.Clear();
        foreach(var c in comments) {
            Comments.Add(c);
        }

        MergedCommentsCount = comments.Count;
    }
}
