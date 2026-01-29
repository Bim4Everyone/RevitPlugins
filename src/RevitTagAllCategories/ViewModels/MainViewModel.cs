using System.Windows.Input;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitTagAllCategories.Models;
using RevitTagAllCategories.Models.Filtration;

namespace RevitTagAllCategories.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;

    public MainViewModel(PluginConfig pluginConfig,
                         RevitRepository revitRepository,
                         ILocalizationService localizationService, 
                         ILogicalFilterProviderFactory filterProviderFactory, 
                         IDataProvider dataProvider) {        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        FilterProvider = filterProviderFactory.Create(dataProvider);

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public ILogicalFilterProvider FilterProvider { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        LoadConfig();
    }

    private void AcceptView() {
        var filter = FilterProvider.GetFilter().GetFilter()
            .Build(_revitRepository.Document, new FilterOptions() { Tolerance = 0 });

        var elements = new FilteredElementCollector(_revitRepository.Document)
            .WherePasses(filter)
            .ToElementIds();

        _revitRepository.ActiveUIDocument.Selection.SetElementIds(elements);

        SaveConfig();
    }

    private bool CanAcceptView() {
        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        _pluginConfig.SaveProjectConfig();
    }
}
