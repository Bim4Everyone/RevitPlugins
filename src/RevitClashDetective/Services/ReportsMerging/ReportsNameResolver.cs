using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;
internal class ReportsNameResolver {
    private readonly IEqualityComparer<ReportViewModel> _reportsComparer;

    public ReportsNameResolver() {
        _reportsComparer = new ReportsNamesIgnoreCaseComparer();
    }

    public ICollection<ReportViewModel> Replace(
        ICollection<ReportViewModel> oldReports,
        ICollection<ReportViewModel> addedReports) {
        return oldReports
            .Except(addedReports, _reportsComparer)
            .Union(addedReports)
            .ToArray();
    }

    public ICollection<ReportViewModel> CopyAndRename(
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
