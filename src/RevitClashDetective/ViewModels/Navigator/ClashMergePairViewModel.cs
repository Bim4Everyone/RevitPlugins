using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Services.ReportsMerging;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashMergePairViewModel : BaseViewModel, ICommentable {
    private static readonly ClashCommentContentComparer _commentsComparer = new();
    private static readonly ClashIdDocComparer _clashComparer = new();
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

    public ClashMergePairViewModel(
        ILocalizationService localizationService,
        ClashViewModel existing,
        ClashViewModel importing) {
        Existing = existing ?? throw new ArgumentNullException(nameof(existing));
        Importing = importing ?? throw new ArgumentNullException(nameof(importing));

        if(!_clashComparer.Equals(Existing, Importing)) {
            throw new ArgumentException("Коллизии не соответствуют друг другу", nameof(importing));
        }

        ClashStatuses = new ReadOnlyCollection<ClashStatusViewModel>(
        [
            new ClashStatusViewModel(localizationService, ClashStatus.Active),
            new ClashStatusViewModel(localizationService, ClashStatus.Analized),
            new ClashStatusViewModel(localizationService, ClashStatus.Solved),
        ]);

        SetMergedCommentsCommand = RelayCommand.Create(SetMergedComments);
        AddCommentCommand = RelayCommand.Create(() => { }, () => false);
        RemoveCommentCommand = RelayCommand.Create(() => { }, () => false);

        PropertyChanged += OnSelectedCommentsChanged;
    }

    public string Name => Existing.ClashName;

    public IReadOnlyCollection<ClashStatusViewModel> ClashStatuses { get; }

    public ClashViewModel Existing { get; }
    public ClashViewModel Importing { get; }
    public ICommand SetMergedCommentsCommand { get; }
    public ICommand AddCommentCommand { get; }
    public ICommand RemoveCommentCommand { get; }

    public string CommentsTitle => ClashName;

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
                ClashName = Existing.ClashName;
                ImportingNameSelected = false;
            }

            RaiseAndSetIfChanged(ref _existingNameSelected, value);
        }
    }

    public bool ImportingNameSelected {
        get => _importingNameSelected;
        set {
            if(value) {
                ClashName = Importing.ClashName;
                ExistingNameSelected = false;
            }

            RaiseAndSetIfChanged(ref _importingNameSelected, value);
        }
    }

    public bool ExistingStatusSelected {
        get => _existingStatusSelected;
        set {
            if(value) {
                ClashStatus = Existing.ClashStatus;
                ImportingStatusSelected = false;
            }

            RaiseAndSetIfChanged(ref _existingStatusSelected, value);
        }
    }

    public bool ImportingStatusSelected {
        get => _importingStatusSelected;
        set {
            if(value) {
                ClashStatus = Importing.ClashStatus;
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
            if(value) {
                ImportingClashSelected = false;
            }
            RaiseAndSetIfChanged(ref _existingClashSelected, value);
        }
    }

    public bool ImportingClashSelected {
        get => _importingClashSelected;
        set {
            ImportingNameSelected = value;
            ImportingStatusSelected = value;
            ImportingCommentsSelected = value;
            if(value) {
                ExistingClashSelected = false;
            }
            RaiseAndSetIfChanged(ref _importingClashSelected, value);
        }
    }

    public ClashCommentViewModel SelectedComment { get; set; }

    public bool CanEditComments {
        get => false;
        set { return; }
    }

    public ObservableCollection<ClashCommentViewModel> Comments { get; } = [];

    public int MergedCommentsCount {
        get => _mergedCommentsCount;
        private set => RaiseAndSetIfChanged(ref _mergedCommentsCount, value);
    }

    public ClashViewModel GetResultClash() {
        Existing.ClashName = ClashName;
        Existing.ClashStatus = ClashStatus;
        Existing.ResetComments(Comments);

        return Existing;
    }

    private void SetMergedComments() {
        if(ExistingCommentsSelected && ImportingCommentsSelected) {
            var comments = Existing.Comments.Union(Importing.Comments, _commentsComparer).ToArray();
            SetMergedComments(comments);
            return;
        }

        if(ExistingCommentsSelected) {
            SetMergedComments(Existing.Comments);
            return;
        }

        if(ImportingCommentsSelected) {
            SetMergedComments(Importing.Comments);
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

    private void OnSelectedCommentsChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(ImportingCommentsSelected)
           || e.PropertyName == nameof(ExistingCommentsSelected)) {
            SetMergedCommentsCommand.Execute(null);
        }
    }
}
