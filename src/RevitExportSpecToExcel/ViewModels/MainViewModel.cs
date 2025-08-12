using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
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
    private readonly ILocalizationService _localizationService;

    private IList<ScheduleViewModel> _schedules;
    private IList<ScheduleViewModel> _filteredSchedules;
    private bool _saveAsOneFile;
    private string _searchText;
    private string _errorText;
    
    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        
        _schedules = _revitRepository.GetSchedulesVM().OrderBy(x => x.OpenStatus).ToList();
        FilteredSchedules = new ObservableCollection<ScheduleViewModel>(_schedules);

        LoadConfig();
        ExportSchedulesCommand = RelayCommand.Create(ExportSchedules, CanAcceptView);
        SearchCommand = RelayCommand.Create(ApplySearch);
    }

    public ICommand ExportSchedulesCommand { get; }
    public ICommand SearchCommand { get; }


    public IList<ScheduleViewModel> Schedules {
        get => _schedules;
        set => RaiseAndSetIfChanged(ref _schedules, value);
    }

    public IList<ScheduleViewModel> FilteredSchedules {
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

    private void ExportSchedules() {
        string path = SelectFolder();

        var schedulesToExport = _schedules
            .Where(x => x.IsChecked)
            .Select(x => x.Schedule);

        ExcelExporter exporter = new ExcelExporter();
        exporter.ExportSchedulesToExcel(path, schedulesToExport, SaveAsOneFile);

        SaveConfig();
    }

    private bool CanAcceptView() {
        if(!_schedules.Where(x => x.IsChecked).Any()) {
            ErrorText = "Не выбраны спецификации";
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void ApplySearch() {
        if(string.IsNullOrEmpty(SearchText)) {
            FilteredSchedules = new ObservableCollection<ScheduleViewModel>(Schedules);
        } else {
            FilteredSchedules = new ObservableCollection<ScheduleViewModel>(
                Schedules
                    .Where(item => item.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0));
        }
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
