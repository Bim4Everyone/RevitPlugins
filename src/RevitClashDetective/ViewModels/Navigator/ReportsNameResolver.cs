using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

namespace RevitClashDetective.ViewModels.Navigator;
internal class ReportsNameResolver {
    private readonly IEqualityComparer<ReportViewModel> _reportsComparer;
    private readonly IEqualityComparer<IClashViewModel> _clashesComparer;
    private readonly ILocalizationService _localization;

    public ReportsNameResolver(ILocalizationService localization) {

        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _reportsComparer = new ReportsNamesIgnoreCaseComparer();
        _clashesComparer = new ClashViewModelComparer();
    }


    public ICollection<ReportViewModel> GetReports(ICollection<ReportViewModel> oldReports,
        ICollection<ReportViewModel> addedReports) {
        string[] intersections = addedReports
            .Intersect(oldReports, _reportsComparer)
            .Select(r => r.Name)
            .ToArray();
        if(intersections.Length > 0) {
            switch(GetResult(string.Join(", ", intersections))) {
                case TaskDialogResult.CommandLink1: {
                    return ReplaceAndKeepStatuses(oldReports, addedReports);
                }
                case TaskDialogResult.CommandLink2: {
                    return Replace(oldReports, addedReports);
                }
                case TaskDialogResult.CommandLink3: {
                    return KeepOnlyOld(oldReports, addedReports);
                }
                case TaskDialogResult.CommandLink4: {
                    return CopyAndRename(oldReports, addedReports);
                }
                default: {
                    throw new NotSupportedException();
                }
            }
        } else {
            return oldReports.Union(addedReports).ToArray();
        }
    }

    private TaskDialogResult GetResult(string names) {
        var dialog = new TaskDialog(_localization.GetLocalizedString("ReportsResolver.Header")) {
            MainContent = _localization.GetLocalizedString("ReportsResolver.Body", names)
        };
        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
            _localization.GetLocalizedString("ReportsResolver.ReplaceAndKeepStatuses"));
        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
            _localization.GetLocalizedString("ReportsResolver.Replace"));
        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3,
            _localization.GetLocalizedString("ReportsResolver.KeepOnlyOld"));
        dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4,
            _localization.GetLocalizedString("ReportsResolver.CopyAndRename"));
        dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
        return dialog.Show();
    }

    private ICollection<ReportViewModel> Replace(
        ICollection<ReportViewModel> oldReports,
        ICollection<ReportViewModel> addedReports) {
        return oldReports
            .Except(addedReports, _reportsComparer)
            .Union(addedReports)
            .ToArray();
    }

    private ICollection<ReportViewModel> ReplaceAndKeepStatuses(
        ICollection<ReportViewModel> oldReports,
        ICollection<ReportViewModel> addedReports) {
        var intersectionOld = oldReports
            .Intersect(addedReports, _reportsComparer)
            .ToArray();
        var intersectionNew = addedReports
            .Intersect(oldReports, _reportsComparer)
            .ToArray();
        foreach(var newReport in intersectionNew) {
            var oldReport = intersectionOld.First(r => _reportsComparer.Equals(r, newReport));
            var oldClashes = oldReport.GuiClashes;
            foreach(var newClash in newReport.GuiClashes) {
                var oldClash = oldClashes.FirstOrDefault(c => _clashesComparer.Equals(c, newClash));
                if(oldClash != null) {
                    newClash.ClashStatus = oldClash.ClashStatus;
                }
            }
        }
        return Replace(oldReports, addedReports);
    }

    private ICollection<ReportViewModel> KeepOnlyOld(
        ICollection<ReportViewModel> oldReports,
        ICollection<ReportViewModel> addedReports) {
        return oldReports
            .Union(addedReports.Except(oldReports, _reportsComparer))
            .ToArray();
    }

    private ICollection<ReportViewModel> CopyAndRename(
        ICollection<ReportViewModel> oldReports,
        ICollection<ReportViewModel> addedReports) {
        var intersection = addedReports
            .Intersect(oldReports, _reportsComparer)
            .ToArray();
        var renamedIntersection = GetRenamedReports(oldReports, intersection).ToArray();
        return oldReports
            .Union(renamedIntersection)
            .Union(addedReports.Except(intersection, _reportsComparer))
            .ToArray();
    }

    private IEnumerable<ReportViewModel> GetRenamedReports(
        ICollection<ReportViewModel> oldReports,
        ICollection<ReportViewModel> intersection) {
        foreach(var element in intersection) {
            int number = oldReports.Where(item => item.Name.StartsWith(element.Name))
                .Select(item => GetNameNumber(item.Name, element.Name))
                .Max();
            element.Name += number / 10 > 0 ? $"_{number + 1}" : $"_0{number + 1}";
            yield return element;
        }
    }

    private int GetNameNumber(string name, string addedElementName) {
        return name.Length == addedElementName.Length
            ? 0
            : int.TryParse(name.Substring(addedElementName.Length + 1), out int res)
                ? res
                : 0;
    }
}
