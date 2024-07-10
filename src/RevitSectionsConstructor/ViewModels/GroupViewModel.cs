using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSectionsConstructor.Models;

namespace RevitSectionsConstructor.ViewModels {
    internal class GroupViewModel : BaseViewModel, IEquatable<GroupViewModel> {
        private readonly IReadOnlyCollection<LevelWrapper> _levelsForPlacing;


        public GroupViewModel(Group group, IReadOnlyCollection<LevelWrapper> levels) {
            Group = group ?? throw new ArgumentNullException(nameof(group));
            _levelsForPlacing = levels ?? throw new ArgumentNullException(nameof(levels));

            Id = Group.Id;
            Name = Group.Name;
            Level = new LevelWrapper(Group.Document.GetElement(Group.LevelId) as Level);
            EnabledTopLevels = new ObservableCollection<LevelWrapper>();
            EnabledBottomLevels = new ObservableCollection<LevelWrapper>(
                _levelsForPlacing.OrderBy(level => level.Elevation));

            SelectedBottomLevel = Level;
            SelectedTopLevel = Level;
        }


        public Group Group { get; }
        public ElementId Id { get; }
        public string Name { get; }
        public LevelWrapper Level { get; }
        public ObservableCollection<LevelWrapper> EnabledBottomLevels { get; }
        public ObservableCollection<LevelWrapper> EnabledTopLevels { get; }

        private bool _deleteGroup;
        public bool DeleteGroup {
            get => _deleteGroup;
            set {
                RaiseAndSetIfChanged(ref _deleteGroup, value);
                if(value) {
                    ActionOnGroup = ActionsOnGroup.Delete;
                } else {
                    ActionOnGroup = ActionsOnGroup.Nothing;
                }
            }
        }

        private LevelWrapper _selectedBottomLevel;
        public LevelWrapper SelectedBottomLevel {
            get => _selectedBottomLevel;
            set {
                RaiseAndSetIfChanged(ref _selectedBottomLevel, value);
                //верхний этаж должен быть не ниже нижнего этажа
                UpdateTopLevels(value);
            }
        }

        private LevelWrapper _selectedTopLevel;
        public LevelWrapper SelectedTopLevel {
            get => _selectedTopLevel;
            set => RaiseAndSetIfChanged(ref _selectedTopLevel, value);
        }

        private ActionsOnGroup _actionOnGroup = ActionsOnGroup.Nothing;
        public ActionsOnGroup ActionOnGroup {
            get => _actionOnGroup;
            set => RaiseAndSetIfChanged(ref _actionOnGroup, value);
        }


        public bool Equals(GroupViewModel other) {
            if(other is null) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return Id == other.Id;
        }

        public override int GetHashCode() {
            return 2108858624 + EqualityComparer<ElementId>.Default.GetHashCode(Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as GroupViewModel);
        }

        public override string ToString() {
            return Name;
        }

        public IList<LevelWrapper> GetLevelsRange() {
            return _levelsForPlacing
                .Where(level =>
                        SelectedBottomLevel?.Elevation <= level.Elevation
                                    && level.Elevation <= SelectedTopLevel?.Elevation)
                .ToList();
        }

        private void UpdateTopLevels(LevelWrapper selectedBottomLevel) {
            if(selectedBottomLevel is null) {
                EnabledTopLevels.Clear();
                return;
            }

            var selectedTopLevel = SelectedTopLevel;
            EnabledTopLevels.Clear();
            if(selectedTopLevel != null && selectedBottomLevel.Elevation <= selectedTopLevel.Elevation) {
                SelectedTopLevel = selectedTopLevel;
            } else {
                SelectedTopLevel = selectedBottomLevel;
            }
            var enabledTopLevels = _levelsForPlacing
                .Where(level => level.Elevation >= selectedBottomLevel.Elevation)
                .OrderBy(level => level.Elevation);
            foreach(var level in enabledTopLevels) {
                EnabledTopLevels.Add(level);
            }
        }
    }
}
