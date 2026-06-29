using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal abstract class SheetComponentVM : ModuleVM {
    private readonly SheetVM _sheet;

    protected SheetComponentVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService) : base(repository, stringParamSetService, sheetSetParams) {
        _sheet = sheetVM;
        LocalizationService = localizationService;

        ModuleTypeName = LocalizationService.GetLocalizedString($"Type.{this.GetType().Name}");
        CreateComponentCommand = RelayCommand.Create(CreateComponent, Validate);
    }

    public ICommand CreateComponentCommand { get; set; }

    protected ILocalizationService LocalizationService { get; }

    public SheetVM Sheet => _sheet;

    /// <summary>
    /// Получает следующий номер видового экрана на листе
    /// </summary>
    protected int GetLastViewportNumber(int startNumber = int.MinValue, int endNumber = int.MaxValue) {
        var viewports = Sheet.SheetInstance.GetAllViewports()
            .Select(id => Repository.Document.GetElement(id) as Viewport)
            .ToList();

        int lastViewportNumber = startNumber;
        foreach(var viewport in viewports) {
            string viewportNumberAsStr = viewport.GetParamValue<string>(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
            // Если не число, то не влияет, т.к. плагин будет ставить число
            if(int.TryParse(viewportNumberAsStr, out int viewportNumberAsInt)) {
                if(viewportNumberAsInt > lastViewportNumber && viewportNumberAsInt < endNumber) {
                    lastViewportNumber = viewportNumberAsInt;
                }
            }
        }
        return lastViewportNumber;
    }

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
            .Where(x => viewportFilter(x.Viewport))
            .OrderByDescending(x => x.Center.X)
            .FirstOrDefault()?.Viewport;
    }

    public override void CreateComponent() {
        using var transaction = Repository.Document.StartTransaction(
            LocalizationService.GetLocalizedString("MainWindow.Title"));

        if(Sheet.SheetInstance is null) {
            Sheet.Process(false);
        }
        Process();
        transaction.Commit();
    }
}
