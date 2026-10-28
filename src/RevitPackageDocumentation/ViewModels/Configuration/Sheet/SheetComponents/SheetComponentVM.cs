using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

/// <summary>
/// Базовый модуль, размещаемый на листе (вид, спецификация, текст, аннотация, легенда)
/// </summary>
internal abstract class SheetComponentVM : ModuleVM {
    private readonly SheetVM _sheet;

    protected SheetComponentVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, localizationService) {
        _sheet = sheetVM;
        LocalizationService = localizationService;

        ModuleTypeName = LocalizationService.GetLocalizedString($"Type.{this.GetType().Name}");
        CreateComponentCommand = RelayCommand.Create(CreateComponent, CanCreateComponent);

        ErrorsChanged += OnErrorsChanged;
    }

    /// <summary>
    /// Команда создания только этого модуля (лист создается, если его еще нет)
    /// </summary>
    public ICommand CreateComponentCommand { get; }

    protected ILocalizationService LocalizationService { get; }

    /// <summary>
    /// Лист, на котором размещается модуль
    /// </summary>
    public SheetVM Sheet => _sheet;

    private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) {
        // Обновляем состояние команды через CommandManager
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Возвращает наибольший номер видового экрана на листе среди номеров, содержащих subStrInViewportNumber,
    /// в диапазоне (startNumber; endNumber). Если таких номеров нет, возвращает startNumber.
    /// Следующий свободный номер - результат + 1
    /// </summary>
    /// <param name="subStrInViewportNumber">
    /// Невидимый символ, которым плагин помечает номера видовых экранов своего типа модулей,
    /// чтобы номера разных типов видов на одном листе не конфликтовали
    /// </param>
    protected int GetLastViewportNumber(
        string subStrInViewportNumber,
        int startNumber = int.MinValue,
        int endNumber = int.MaxValue) {

        var viewports = Sheet.SheetInstance.GetAllViewports()
            .Select(id => Repository.Document.GetElement(id) as Viewport)
            .ToList();

        int lastViewportNumber = startNumber;
        foreach(var viewport in viewports) {
            string viewportNumberAsStr = viewport.GetParamValue<string>(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);

            if(viewportNumberAsStr is null || !viewportNumberAsStr.Contains(subStrInViewportNumber)) {
                continue;
            }

            // Если не число, то не влияет, т.к. плагин будет ставить число
            if(int.TryParse(viewportNumberAsStr.Replace(subStrInViewportNumber, ""), out int viewportNumberAsInt)) {
                if(viewportNumberAsInt > lastViewportNumber && viewportNumberAsInt < endNumber) {
                    lastViewportNumber = viewportNumberAsInt;
                }
            }
        }
        return lastViewportNumber;
    }

    /// <summary>
    /// Возвращает самый правый видовой экран листа с видом типа TView, подходящий под фильтр, или null
    /// </summary>
    /// <param name="viewportFilter">Дополнительное условие отбора. Если null - отбираются все видовые экраны</param>
    /// <param name="isCallout">Искать видовые экраны фрагментов (true) или обычных видов (false)</param>
    protected Viewport GetLastViewport<TView>(Func<Viewport, bool> viewportFilter = null, bool isCallout = false)
        where TView : View {
        return Repository.GetViewports(Sheet.SheetInstance)
            .Select(viewport => new {
                Viewport = viewport,
                View = Repository.Document.GetElement(viewport.ViewId) as View,
                Center = viewport.GetBoxCenter()
            })
            .Where(x => x.View is TView)
            .Where(x => x.View.IsCallout == isCallout)
            .Where(x => viewportFilter?.Invoke(x.Viewport) ?? true)
            .OrderByDescending(x => x.Center.X)
            .FirstOrDefault()?.Viewport;
    }

    /// <summary>
    /// Создает модуль в отдельной транзакции. Если лист еще не найден или не создан, сначала создается лист
    /// без остальных модулей
    /// </summary>
    public override void CreateComponent() {
        using var transaction = Repository.Document.StartTransaction(
            LocalizationService.GetLocalizedString("MainWindow.Title"));

        if(Sheet.SheetInstance is null) {
            Sheet.Process(false);
        }
        Process();
        transaction.Commit();
    }

    public override bool CanCreateComponent() {
        return !HasErrors;
    }
}
