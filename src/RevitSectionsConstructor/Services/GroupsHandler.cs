using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.Revit;

using RevitSectionsConstructor.Models;

namespace RevitSectionsConstructor.Services {
    internal class GroupsHandler {
        private readonly RevitRepository _revitRepository;

        public GroupsHandler(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        }


        public void ProcessGroups(ICollection<GroupWithAction> groups) {
            using(var trans = _revitRepository.Document.StartTransaction("Обработка групп")) {
                foreach(var group in groups) {
                    switch(group.ActionOnGroup) {
                        case ActionsOnGroup.Copy:
                            _revitRepository.CopyGroup(group);
                            if(!group.LevelsForPlacing.Contains(group.CurrentLevel)) {
                                _revitRepository.RemoveElement(group.Group.Id);
                            }
                            break;
                        case ActionsOnGroup.Delete:
                            _revitRepository.RemoveElement(group.Group.Id);
                            break;
                        case ActionsOnGroup.Nothing:
                            break;
                        default:
                            throw new NotSupportedException(
                                $"Не поддерживаемое действие над группами: " +
                                $"{group.ActionOnGroup}={(int) group.ActionOnGroup}");
                    }
                }
                trans.Commit();
            }
        }
    }
}
