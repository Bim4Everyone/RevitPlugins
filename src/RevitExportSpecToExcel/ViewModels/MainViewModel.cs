using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.Dialogs;

using RevitExportSpecToExcel.Models;

namespace RevitExportSpecToExcel.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ExcelExporter _excelExporter;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<ScheduleViewModel> _schedules = new();
    private ObservableCollection<ScheduleViewModel> _filteredSchedules = new();
    private bool _saveAsOneFile;
    private string _searchText;
    private string _errorText;
    
    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(PluginConfig pluginConfig,
                         RevitRepository revitRepository,
                         ExcelExporter excelExporter,
                         ILocalizationService localizationService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _excelExporter = excelExporter;
        _localizationService = localizationService;
        
        LoadViewCommand = RelayCommand.Create(LoadView);
        SearchCommand = RelayCommand.Create(ApplySearch);
        ExportSchedulesCommand = RelayCommand.Create(ExportSchedules, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand ExportSchedulesCommand { get; }
    public ICommand SearchCommand { get; }


    public ObservableCollection<ScheduleViewModel> Schedules {
        get => _schedules;
        set => RaiseAndSetIfChanged(ref _schedules, value);
    }

    public ObservableCollection<ScheduleViewModel> FilteredSchedules {
        get => _filteredSchedules;
        set => RaiseAndSetIfChanged(ref _filteredSchedules, value);
    }
    
    public bool SaveAsOneFile {
        get => _saveAsOneFile;
        set => RaiseAndSetIfChanged(ref _saveAsOneFile, value);
    }

    public string SearchText {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string SelectFolder() {
        CommonOpenFileDialog dialog = new CommonOpenFileDialog() {
            IsFolderPicker = true
        };

        if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            return dialog.FileName;
        }

        return string.Empty;
    }

    private void LoadView() {
        Schedules = new(_revitRepository
            .GetSchedulesVM(_localizationService)
            .ToList());
        FilteredSchedules = new ObservableCollection<ScheduleViewModel>(Schedules.OrderBy(x => x.OpenStatus));

        if(_revitRepository.Document.ActiveView is not ViewSchedule) {
            LoadConfig();
        }
    }

    private void ExportSchedules() {
        string path = SelectFolder();

        var schedulesToExport = _schedules
            .Where(x => x.IsChecked)
            .Select(x => x.Schedule)
            .ToList();

        string documentName = _revitRepository.Document.Title;
        _excelExporter.ExportSchedules(path, documentName, schedulesToExport, SaveAsOneFile);

        SaveConfig();
    }

    private bool CanAcceptView() {
        if(!_schedules.Any(x => x.IsChecked)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NoSelection");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void ApplySearch() {
        if(string.IsNullOrEmpty(SearchText)) {
            FilteredSchedules = new ObservableCollection<ScheduleViewModel>(_schedules.OrderBy(x => x.OpenStatus));
        } else {
            FilteredSchedules = new ObservableCollection<ScheduleViewModel>(
                Schedules.Where(item => item.Name
                        .IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(x => x.OpenStatus));
        }
    }

    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        if(setting is null) {
            setting = _pluginConfig.AddSettings(_revitRepository.Document);
        }

        var schedules = _schedules
            .Where(x => setting.SelectedSchedules.Contains(x.Name));

        foreach(var schedule in schedules) {
            schedule.IsChecked = true;
        }
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SelectedSchedules = _schedules
            .Where(x => x.IsChecked)
            .Select(x => x.Name)
            .ToList();

        _pluginConfig.SaveProjectConfig();
    }
}
