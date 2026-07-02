using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Processors;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IBoundaryProcessor _processor;

    private string _errorText;
    private string _saveProperty;
    
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IBoundaryProcessor processor) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _processor = processor;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }
    
    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }
    private void LoadView() {
        LoadConfig();
    }
    private void AcceptView() {
        SaveConfig();
        
        var view = _revitRepository.ActiveUiDocument.ActiveView;
        var boundarySettings = new AreaBoundarySettings { TargetViews = [view] };

        const string transactionName = "TransactionName";
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        
        _processor.DrawBoundaries(boundarySettings);
        
        t.Commit();
    }
    
    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(SaveProperty)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
            return false;
        }

        ErrorText = null;
        return true;
    }
    
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }
    
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        _pluginConfig.SaveProjectConfig();
    }
}
