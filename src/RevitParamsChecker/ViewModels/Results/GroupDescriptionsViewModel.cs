using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Results;

internal class GroupDescriptionsViewModel : BaseViewModel {
    private GroupDescriptionViewModel _selectedGroupDescription;
    private bool _isGroupingEnabled = true;

    public GroupDescriptionsViewModel(
        ICollection<PropertyViewModel> availableProperties,
        ICollection<PropertyViewModel> selectedProperties) {
        if(availableProperties == null) {
            throw new ArgumentNullException(nameof(availableProperties));
        }

        if(selectedProperties == null) {
            throw new ArgumentNullException(nameof(selectedProperties));
        }

        if(availableProperties.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(availableProperties));
        }

        AvailableProperties = new ReadOnlyCollection<PropertyViewModel>(availableProperties.ToArray());
        GroupDescriptions = [
            ..selectedProperties.Intersect(availableProperties)
                .Select(p => new GroupDescriptionViewModel(p))
        ];

        AddGroupCommand = RelayCommand.Create(AddGroup, CanAddGroup);
        RemoveGroupCommand = RelayCommand.Create<GroupDescriptionViewModel>(RemoveGroup, CanRemoveGroup);
    }

    public IReadOnlyCollection<PropertyViewModel> AvailableProperties { get; }

    public ObservableCollection<GroupDescriptionViewModel> GroupDescriptions { get; }

    public bool IsGroupingEnabled {
        get => _isGroupingEnabled;
        set {
            RaiseAndSetIfChanged(ref _isGroupingEnabled, value);
            if(!value) {
                SelectedGroupDescription = null;
                GroupDescriptions.Clear();
            }
        }
    }

    public GroupDescriptionViewModel SelectedGroupDescription {
        get => _selectedGroupDescription;
        set => RaiseAndSetIfChanged(ref _selectedGroupDescription, value);
    }

    public ICommand AddGroupCommand { get; }

    public ICommand RemoveGroupCommand { get; }

    private void AddGroup() {
        GroupDescriptions.Add(new GroupDescriptionViewModel(AvailableProperties.First()));
    }

    private bool CanAddGroup() {
        return GroupDescriptions.Count < AvailableProperties.Count;
    }

    private void RemoveGroup(GroupDescriptionViewModel group) {
        GroupDescriptions.Remove(group);
    }

    private bool CanRemoveGroup(GroupDescriptionViewModel group) {
        return GroupDescriptions.Count > 0 && group != null;
    }
}
