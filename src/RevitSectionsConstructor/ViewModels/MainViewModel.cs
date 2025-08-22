using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSectionsConstructor.Models;
using RevitSectionsConstructor.Services;

namespace RevitSectionsConstructor.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly GroupsHandler _groupsHandler;
    private readonly DocumentSaver _documentSaver;
    private readonly ILocalizationService _localization;

    public MainViewModel(
        RevitRepository revitRepository,
        GroupsHandler groupsHandler,
        DocumentSaver documentSaver,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        ILocalizationService localization) {

        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _groupsHandler = groupsHandler
            ?? throw new System.ArgumentNullException(nameof(groupsHandler));
        _documentSaver = documentSaver
            ?? throw new ArgumentNullException(nameof(documentSaver));
        SaveFileDialogService = saveFileDialogService
            ?? throw new ArgumentNullException(nameof(saveFileDialogService));
        MessageBoxService = messageBoxService
            ?? throw new ArgumentNullException(nameof(messageBoxService));
        _localization = localization
            ?? throw new ArgumentNullException(nameof(localization));
        GroupsNotForCopy = new ObservableCollection<GroupViewModel>(InitializeGroupViewModels(_revitRepository));
        GroupsForCopy = [];

        AcceptViewCommand
            = RelayCommand.Create(AcceptView, CanAcceptView);
        SelectPathCommand
            = RelayCommand.Create(SelectPath);
        MoveGroupsToCopyCommand
            = RelayCommand.Create<IList>(MoveGroupsToCopy, CanMoveGroups);
        MoveGroupsFromCopyCommand
            = RelayCommand.Create<IList>(MoveGroupsFromCopy, CanMoveGroups);
    }


    public IMessageBoxService MessageBoxService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public ICommand AcceptViewCommand { get; }
    public ICommand SelectPathCommand { get; }
    public ICommand MoveGroupsToCopyCommand { get; }
    public ICommand MoveGroupsFromCopyCommand { get; }
    /// <summary>
    /// Список для левой половины окна с группами, с которыми либо ничего не делать, либо удалить их
    /// </summary>
    public ObservableCollection<GroupViewModel> GroupsNotForCopy { get; }
    /// <summary>
    /// Список для правой половины окна с группами, которые надо скопировать на выбранные этажи
    /// </summary>
    public ObservableCollection<GroupViewModel> GroupsForCopy { get; }

    private string _errorText;
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private string _path;
    public string Path {
        get => _path;
        set => RaiseAndSetIfChanged(ref _path, value);
    }


    private void AcceptView() {
        _groupsHandler.ProcessGroups(GetGroupWithActions());
        _documentSaver.SaveDocument(Path);
    }

    private bool CanAcceptView() {
        if(string.IsNullOrWhiteSpace(Path)) {
            ErrorText = _localization.GetLocalizedString("Validation.EmptyPath");
            return false;
        }

        ErrorText = string.Empty;
        return true;
    }

    private void SelectPath() {
        if(SaveFileDialogService.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), string.Empty)) {
            Path = SaveFileDialogService.File.FullName;
        }
    }

    private void MoveGroupsToCopy(IList selectedGroupViewModels) {
        MoveGroupsFromTo(selectedGroupViewModels, GroupsNotForCopy, GroupsForCopy);
        UpdateActionsOnGroups();
    }

    private void MoveGroupsFromCopy(IList selectedGroupViewModels) {
        MoveGroupsFromTo(selectedGroupViewModels, GroupsForCopy, GroupsNotForCopy);
        UpdateActionsOnGroups();
    }

    private void MoveGroupsFromTo(
        IList selectedGroupViewModels,
        ObservableCollection<GroupViewModel> from,
        ObservableCollection<GroupViewModel> to) {

        var selectedItems = selectedGroupViewModels
            .OfType<GroupViewModel>()
            .ToArray();
        foreach(var item in selectedItems) {
            to.Add(item);
            from.Remove(item);
        }
    }

    private void UpdateActionsOnGroups() {
        foreach(var item in GroupsForCopy) {
            item.DeleteGroup = false;
            item.ActionOnGroup = ActionsOnGroup.Copy;
        }
        foreach(var item in GroupsNotForCopy) {
            if(item.ActionOnGroup == ActionsOnGroup.Copy) {
                item.ActionOnGroup = ActionsOnGroup.Nothing;
            }
        }
    }

    private bool CanMoveGroups(IList selectedGroupViewModels) {
        return selectedGroupViewModels != null
            && selectedGroupViewModels.OfType<GroupViewModel>().Any();
    }

    private IOrderedEnumerable<GroupViewModel> InitializeGroupViewModels(RevitRepository revitRepository) {
        var levels = revitRepository.GetLevelWrappers();

        return revitRepository
            .GetParentGroups()
            .Select(group => new GroupViewModel(group, levels))
            .OrderBy(group => group.Level.Elevation);
    }

    private IList<GroupWithAction> GetGroupWithActions() {
        List<GroupWithAction> list = [];
        foreach(var item in GroupsForCopy) {
            list.Add(new GroupWithAction(item.Group, item.Level, item.ActionOnGroup, item.GetLevelsRange()));
        }
        foreach(var item in GroupsNotForCopy) {
            list.Add(new GroupWithAction(item.Group, item.Level, item.ActionOnGroup, Array.Empty<LevelWrapper>()));
        }
        return list;
    }
}
