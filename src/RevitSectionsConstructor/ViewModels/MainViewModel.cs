using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using DevExpress.Xpf.Core;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSectionsConstructor.Models;
using RevitSectionsConstructor.Services;

namespace RevitSectionsConstructor.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly GroupsHandler _groupsHandler;
        private readonly DocumentSaver _documentSaver;
        private readonly ISaveFileDialogService _saveFileDialogService;
        private readonly IMessageBoxService _messageBoxService;

        public MainViewModel(
            RevitRepository revitRepository,
            GroupsHandler groupsHandler,
            DocumentSaver documentSaver,
            ISaveFileDialogService saveFileDialogService,
            IMessageBoxService messageBoxService) {

            _revitRepository = revitRepository
                ?? throw new System.ArgumentNullException(nameof(revitRepository));
            _groupsHandler = groupsHandler
                ?? throw new System.ArgumentNullException(nameof(groupsHandler));
            _documentSaver = documentSaver
                ?? throw new ArgumentNullException(nameof(documentSaver));
            _saveFileDialogService = saveFileDialogService
                ?? throw new ArgumentNullException(nameof(saveFileDialogService));
            _messageBoxService = messageBoxService
                ?? throw new ArgumentNullException(nameof(messageBoxService));
            GroupsNotForCopy = new ObservableCollection<GroupViewModel>(InitializeGroupViewModels(_revitRepository));
            GroupsForCopy = new ObservableCollection<GroupViewModel>();

            AcceptViewCommand
                = RelayCommand.Create(AcceptView, CanAcceptView);
            SelectPathCommand
                = RelayCommand.Create(SelectPath);
            MoveGroupsToCopyCommand
                = RelayCommand.Create<ObservableCollectionCore<object>>(MoveGroupsToCopy, CanMoveGroups);
            MoveGroupsFromCopyCommand
                = RelayCommand.Create<ObservableCollectionCore<object>>(MoveGroupsFromCopy, CanMoveGroups);
        }


        public IMessageBoxService MessageBoxService => _messageBoxService;
        public ISaveFileDialogService SaveFileDialogService => _saveFileDialogService;
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
                ErrorText = "Укажите путь для сохранения модели";
                return false;
            }

            ErrorText = string.Empty;
            return true;
        }

        private void SelectPath() {
            _saveFileDialogService.Title = "Выберите место для сохранения";
            _saveFileDialogService.AddExtension = true;
            _saveFileDialogService.Filter = "Revit projects | *.rvt";
            _saveFileDialogService.DefaultExt = "rvt";
            if(_saveFileDialogService.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "")) {
                Path = _saveFileDialogService.File.FullName;
            }
        }

        private void MoveGroupsToCopy(ObservableCollectionCore<object> selectedGroupViewModels) {
            MoveGroupsFromTo(selectedGroupViewModels, GroupsNotForCopy, GroupsForCopy);
            UpdateActionsOnGroups();
        }

        private void MoveGroupsFromCopy(ObservableCollectionCore<object> selectedGroupViewModels) {
            MoveGroupsFromTo(selectedGroupViewModels, GroupsForCopy, GroupsNotForCopy);
            UpdateActionsOnGroups();
        }

        private void MoveGroupsFromTo(
            ObservableCollectionCore<object> selectedGroupViewModels,
            ObservableCollection<GroupViewModel> from,
            ObservableCollection<GroupViewModel> to) {

            var selectedItems = selectedGroupViewModels
                .Where(item => item is GroupViewModel)
                .Cast<GroupViewModel>()
                .ToArray();
            foreach(GroupViewModel item in selectedItems) {
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

        private bool CanMoveGroups(ObservableCollectionCore<object> selectedGroupViewModels) {
            return selectedGroupViewModels != null
                && selectedGroupViewModels.Any(item => item is GroupViewModel);
        }

        private IOrderedEnumerable<GroupViewModel> InitializeGroupViewModels(RevitRepository revitRepository) {
            IReadOnlyCollection<LevelWrapper> levels = revitRepository.GetLevelWrappers();

            return revitRepository
                .GetParentGroups()
                .Select(group => new GroupViewModel(group, levels))
                .OrderBy(group => group.Level.Elevation);
        }

        private IList<GroupWithAction> GetGroupWithActions() {
            List<GroupWithAction> list = new List<GroupWithAction>();
            foreach(var item in GroupsForCopy) {
                list.Add(new GroupWithAction(item.Group, item.Level, item.ActionOnGroup, item.GetLevelsRange()));
            }
            foreach(var item in GroupsNotForCopy) {
                list.Add(new GroupWithAction(item.Group, item.Level, item.ActionOnGroup, Array.Empty<LevelWrapper>()));
            }
            return list;
        }
    }
}
