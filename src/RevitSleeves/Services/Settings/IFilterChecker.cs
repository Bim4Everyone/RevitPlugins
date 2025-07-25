using RevitClashDetective.Models.FilterModel;

namespace RevitSleeves.Services.Settings;
internal interface IFilterChecker {
    /// <summary>
    /// Показывает окно проверки фильтра в немодальном режиме
    /// </summary>
    /// <param name="filter"></param>
    void ShowFilter(Filter filter);
}
