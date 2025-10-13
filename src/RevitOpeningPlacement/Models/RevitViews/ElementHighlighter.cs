using System;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitOpeningPlacement.Models.RevitViews;
/// <summary>
/// Класс для графического выделения заданного элемента (стены или перекрытия) на 3D виде
/// </summary>
internal class ElementHighlighter {
    private readonly Element _elementToHighlight;
    private readonly RevitRepository _revitRepository;
    private readonly View3D _view3D;
    private readonly IMessageBoxService _messageBoxService;

    /// <summary>
    /// Конструктор класса для графического выделения заданного элемента (стены или перекрытия) на 3D виде
    /// <para>
    /// Графическое выделение происходит при помощи генерации фильтров вида, 
    /// в которые попадают все конструкции (стены и перекрытия), за исключением заданного элемента, 
    /// с последующим графики
    /// </para>
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа, в котором нужно выделить элемент на 3D виде</param>
    /// <param name="view3D">3D вид, на котором нужно выделить элемент</param>
    /// <param name="elementToHighlight">Элемент, который нужно выделить. Это должна быть стена или перекрытие</param>
    /// <exception cref="ArgumentNullException">Элемент должен быть стеной или перекрытием</exception>
    /// <exception cref="ArgumentException">Исключение, если класс элемента не поддерживается</exception>
    public ElementHighlighter(RevitRepository revitRepository,
        View3D view3D,
        Element elementToHighlight,
        IMessageBoxService messageBoxService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _view3D = view3D ?? throw new ArgumentNullException(nameof(view3D));
        _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        var element = elementToHighlight ?? throw new ArgumentNullException(nameof(elementToHighlight));
        _elementToHighlight = element is Wall or Floor
            ? element
            : throw new ArgumentException(nameof(elementToHighlight));

    }

    public void HighlightElement() {
        try {
            _revitRepository.DoAction(ApplyGraphicSettings);

        } catch(AccessViolationException) {
            _messageBoxService.Show(
                $"Окно плагина было открыто в другом документе Revit, который был закрыт, нельзя показать элемент.",
                $"BIM",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            _messageBoxService.Show(
                $"Окно плагина было открыто в другом документе Revit, который сейчас не активен, нельзя показать элемент.",
                $"BIM",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }
    }

    /// <summary>
    /// Устанавливает настройки графики для фильтров, в которые попадают все конструкции, за исключением заданного элемента
    /// </summary>
    private void ApplyGraphicSettings() {
        var doc = _view3D.Document;
        var filters = ParameterFilterInitializer.GetHighlightFilters(doc, _elementToHighlight);
        var graphicsSettings = GraphicSettingsInitializer.GetNotInterestingConstructionsGraphicSettings();
        using var t = doc.StartTransaction("Выделение хоста отверстия");

        foreach(var filter in filters) {
            if(_view3D.GetFilters().Contains(filter.Id)) {
                _view3D.RemoveFilter(filter.Id);
            }
            _view3D.AddFilter(filter.Id);
            _view3D.SetFilterOverrides(filter.Id, graphicsSettings);
            _view3D.SetFilterVisibility(filter.Id, true);
        }

        t.Commit();
    }
}
