using Bim4Everyone.RevitFiltration.Controls;

namespace RevitSleeves.Services.Settings;
internal interface IFilterChecker {
    /// <summary>
    /// Показывает окно проверки фильтра в немодальном режиме
    /// </summary>
    void ShowFilter(ILogicalFilterContext filterContext);
}
