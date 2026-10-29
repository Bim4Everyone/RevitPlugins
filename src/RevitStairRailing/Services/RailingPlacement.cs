using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitStairRailing.Models;

namespace RevitStairRailing.Services;

/// <summary>
/// Построение ограждения после закрытия окна: выбор линий траектории, раскладка, создание элементов и сборки.
/// </summary>
internal class RailingPlacement {
    private const double MillimetersToFeet = 1 / 304.8;

    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public RailingPlacement(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Строит ограждение по линиям модели: выбранным до запуска или указанным пользователем сейчас.
    /// </summary>
    public void Run(RailingParameters parameters) {
        string title = GetText("MainWindow.Title");
        try {
            List<Line> lines = _revitRepository.GetSelectedModelLines();
            if(lines.Count == 0) {
                lines = _revitRepository.PickModelLines(GetText("Pick.Prompt"));
            }

            RailingPath path = RailingPath.FromLines(lines)
                .Offset(parameters.EdgeOffset * MillimetersToFeet, parameters.Side);
            List<RailingMember> members = RailingLayout.Create(path, parameters);
            RailingResult result = new RailingBuilder(_revitRepository.Document).Build(members, parameters, title);
            _revitRepository.SelectElements(result.Assembly != null
                ? new List<ElementId> { result.Assembly.Id }
                : result.ElementIds);
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            // выбор линий отменён — ничего не строим
        } catch(RailingException ex) {
            TaskDialog.Show(title, GetText(ex.MessageKey));
        }
    }

    private string GetText(string key) {
        return _localizationService.GetLocalizedString(key);
    }
}
