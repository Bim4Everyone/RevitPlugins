using System;
using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ReportsIntersectionResolver {
    /// <summary>
    /// Заменяет существующие отчеты левой части из пересечения правыми и возвращает объединение множеств
    /// </summary>
    /// <param name="reportsIntersection">Пересечение множеств отчетов</param>
    /// <returns>Все отчеты из правой части и отчеты левой части, которые не входили в пересечение</returns>
    public ICollection<ReportViewModel> Replace(ReportSetsIntersectionResult reportsIntersection) {
        if(reportsIntersection == null) {
            throw new ArgumentNullException(nameof(reportsIntersection));
        }

        return reportsIntersection.LeftOuterSet
            .Union(reportsIntersection.RightInnerSet, reportsIntersection.RightOuterSet)
            .Reports;
    }

    /// <summary>
    /// Переименовывает отчеты из правой части пересечения и возвращает объединение множеств
    /// </summary>
    /// <param name="reportsIntersection">Пересечение множеств отчетов</param>
    /// <returns>Все отчеты левой части, переименованные отчеты правой части из пересечения
    /// и отчеты правой части, не входившие в пересечение</returns>
    public ICollection<ReportViewModel> CopyAndRename(ReportSetsIntersectionResult reportsIntersection) {
        var renamedImported = GetRenamedReports(
            reportsIntersection.LeftInnerSet.Reports,
            reportsIntersection.RightInnerSet.Reports);
        return reportsIntersection.LeftOuterSet.Union(
                reportsIntersection.LeftInnerSet,
                reportsIntersection.RightOuterSet)
            .Reports.Union(renamedImported)
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
