using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
internal abstract class SheetComponentVM : BaseParamContainerVM {
    private bool _isModuleCheck;
    private string _moduleName;
    private string _moduleComment;
    private string _moduleCode;
    private string _moduleTypeName;
    private string _moduleErrors;
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
    }

    public ICommand CreateComponentCommand { get; set; }

    protected ILocalizationService LocalizationService { get; }

    public SheetVM Sheet => _sheet;

    public bool IsModuleCheck {
        get => _isModuleCheck;
        set => RaiseAndSetIfChanged(ref _isModuleCheck, value);
    }

    public string ModuleName {
        get => _moduleName;
        set => RaiseAndSetIfChanged(ref _moduleName, value);
    }

    public string ModuleComment {
        get => _moduleComment;
        set => RaiseAndSetIfChanged(ref _moduleComment, value);
    }

    public string ModuleCode {
        get => _moduleCode;
        set => RaiseAndSetIfChanged(ref _moduleCode, value);
    }

    public string ModuleTypeName {
        get => _moduleTypeName;
        set => RaiseAndSetIfChanged(ref _moduleTypeName, value);
    }

    public string ModuleErrors {
        get => _moduleErrors;
        set => RaiseAndSetIfChanged(ref _moduleErrors, value);
    }



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

    public abstract void CreateComponent();
    public abstract bool ValidateModule();
    public abstract void Process();
}
