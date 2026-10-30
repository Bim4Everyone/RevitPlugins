using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitStairRailing.Models;

namespace RevitStairRailing.Services;

/// <summary>
/// Построение ограждения после закрытия окна: выбор линий траектории, раскладка, создание элементов и сборки.
/// </summary>
internal class RailingPlacement {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public RailingPlacement(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Строит ограждение по линиям модели: выбранным до запуска или указанным пользователем сейчас.
    /// </summary>
    /// <exception cref="Autodesk.Revit.Exceptions.OperationCanceledException">Пользователь отменил выбор линий.</exception>
    /// <exception cref="RailingException">Линии не образуют траекторию или ограждение не построить.</exception>
    public void Run(RailingParameters parameters) {
        List<Line> lines = _revitRepository.GetSelectedModelLines();
        if(lines.Count == 0) {
            lines = _revitRepository.PickModelLines(GetText("Pick.Prompt"));
        }

        RailingPath edgePath = RailingPath.FromLines(lines);
        RailingPath path = edgePath.Offset(RailingLayout.GetAxisOffsets(edgePath, parameters), parameters.Side);
        List<RailingMember> members = RailingLayout.Create(path, parameters);
        RailingResult result = new RailingBuilder(_revitRepository.Document)
            .Build(members, parameters, GetText("Build.TransactionName"));
        _revitRepository.SelectElements(result.Assembly != null
            ? new List<ElementId> { result.Assembly.Id }
            : result.ElementIds);
    }

    private string GetText(string key) {
        return _localizationService.GetLocalizedString(key);
    }
}
