using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

namespace RevitSectionsConstructor.Models {
    internal class GroupWithAction {
        public GroupWithAction(
            Group group,
            LevelWrapper currentLevel,
            ActionsOnGroup action,
            IList<LevelWrapper> levelsForPlacing = null) {

            Group = group ?? throw new ArgumentNullException(nameof(group));
            CurrentLevel = currentLevel ?? throw new ArgumentNullException(nameof(currentLevel));
            ActionOnGroup = action;

            if(action == ActionsOnGroup.Copy && (levelsForPlacing is null || levelsForPlacing.Count == 0)) {
                throw new ArgumentException(nameof(levelsForPlacing));
            }
            LevelsForPlacing = new ReadOnlyCollection<LevelWrapper>(levelsForPlacing ?? Array.Empty<LevelWrapper>());
        }


        public Group Group { get; }
        public LevelWrapper CurrentLevel { get; }
        public ActionsOnGroup ActionOnGroup { get; }
        public IReadOnlyCollection<LevelWrapper> LevelsForPlacing { get; }
    }
}
