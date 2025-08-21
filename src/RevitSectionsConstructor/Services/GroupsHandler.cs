using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSectionsConstructor.Models;

namespace RevitSectionsConstructor.Services;
internal class GroupsHandler {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;

    public GroupsHandler(RevitRepository revitRepository, ILocalizationService localization) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }


    public void ProcessGroups(ICollection<GroupWithAction> groups) {
        string msg = _localization.GetLocalizedString("TransTitle");
        using var trans = _revitRepository.Document.StartTransaction(msg);
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
                    throw new NotSupportedException(_localization.GetLocalizedString("Errors.UnsupportedGroupAction",
                        $"{group.ActionOnGroup}={(int) group.ActionOnGroup}"));
            }
        }
        trans.Commit();
    }
}
