using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDocumenter.Models;
using RevitDocumenter.Models.RoomDimensions;
using RevitDocumenter.Models.RoomDimensions.Settings;

namespace RevitDocumenter.ViewModels;

/// <summary>
/// ViewModel окна образмеривания помещений.
/// </summary>
internal class RoomDimensioningViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly RoomDimensioningSettingsReader _settingsReader;
    private readonly RoomDimensioningService _dimensioningService;

    private RoomDimensioningSettings _settings;
    private string _settingsPath;
    private string _settingsErrorText;
    private string _activeViewName;
    private List<RoomTypeSummaryViewModel> _roomTypeSummaries = [];
    private int _totalRoomCount;
    private int _unmatchedRoomCount;
    private string _errorText;
    private bool _debugMode;
    private bool _isSettingsLoaded;

    public RoomDimensioningViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        RoomDimensioningSettingsReader settingsReader,
        RoomDimensioningService dimensioningService,
        IOpenFileDialogService openFileDialogService) {
        _pluginConfig = pluginConfig.ThrowIfNull();
        _revitRepository = revitRepository.ThrowIfNull();
        _localizationService = localizationService.ThrowIfNull();
        _settingsReader = settingsReader.ThrowIfNull();
        _dimensioningService = dimensioningService.ThrowIfNull();
        OpenFileDialogService = openFileDialogService.ThrowIfNull();

        LoadViewCommand = RelayCommand.Create(LoadView);
        BrowseSettingsCommand = RelayCommand.Create(BrowseSettings);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand BrowseSettingsCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }

    /// <summary>
    /// Путь к JSON-файлу настроек.
    /// </summary>
    public string SettingsPath {
        get => _settingsPath;
        set => RaiseAndSetIfChanged(ref _settingsPath, value);
    }

    /// <summary>
    /// Ошибка чтения файла настроек.
    /// </summary>
    public string SettingsErrorText {
        get => _settingsErrorText;
        set => RaiseAndSetIfChanged(ref _settingsErrorText, value);
    }

    public string ActiveViewName {
        get => _activeViewName;
        set => RaiseAndSetIfChanged(ref _activeViewName, value);
    }

    /// <summary>
    /// Сводка по типам помещений из настроек.
    /// </summary>
    public List<RoomTypeSummaryViewModel> RoomTypeSummaries {
        get => _roomTypeSummaries;
        set => RaiseAndSetIfChanged(ref _roomTypeSummaries, value);
    }

    /// <summary>
    /// Количество размещенных помещений на активном виде.
    /// </summary>
    public int TotalRoomCount {
        get => _totalRoomCount;
        set => RaiseAndSetIfChanged(ref _totalRoomCount, value);
    }

    /// <summary>
    /// Количество помещений, тип которых не найден в настройках. Они будут пропущены.
    /// </summary>
    public int UnmatchedRoomCount {
        get => _unmatchedRoomCount;
        set => RaiseAndSetIfChanged(ref _unmatchedRoomCount, value);
    }

    /// <summary>
    /// Режим отладки: контур в Revit, журнал и изображение вида. Сохраняется в файл настроек (Debug.Enabled).
    /// </summary>
    public bool DebugMode {
        get => _debugMode;
        set => RaiseAndSetIfChanged(ref _debugMode, value);
    }

    /// <summary>
    /// Файл настроек прочитан без ошибок.
    /// </summary>
    public bool IsSettingsLoaded {
        get => _isSettingsLoaded;
        set => RaiseAndSetIfChanged(ref _isSettingsLoaded, value);
    }

    /// <summary>
    /// Текст ошибки, из-за которой запуск недоступен.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        ActiveViewName = _revitRepository.ActiveView.Name;

        string path = _pluginConfig.GetSettings(_revitRepository.Document)?.RoomDimensioningSettingsPath;
        // Если файл еще не выбран или пропал по сохраненному пути, сразу предлагаем выбрать
        if(string.IsNullOrWhiteSpace(path) || !File.Exists(path)) {
            if(OpenFileDialogService.ShowDialog()) {
                path = OpenFileDialogService.File.FullName;
            }
        }
        LoadSettings(path);
    }

    private void BrowseSettings() {
        if(OpenFileDialogService.ShowDialog()) {
            LoadSettings(OpenFileDialogService.File.FullName);
        }
    }

    private void LoadSettings(string path) {
        SettingsPath = path;
        _settings = null;
        IsSettingsLoaded = false;
        RoomTypeSummaries = [];
        TotalRoomCount = 0;
        UnmatchedRoomCount = 0;

        if(string.IsNullOrWhiteSpace(path)) {
            SettingsErrorText = Localize("RoomDimensioningWindow.SettingsFileNotSelected");
            return;
        }

        try {
            _settings = _settingsReader.Read(path);
            SettingsErrorText = string.Empty;
            DebugMode = _settings.Debug.Enabled;
            IsSettingsLoaded = true;
        } catch(InvalidOperationException ex) {
            SettingsErrorText = ex.Message;
            return;
        }

        BuildSummary();
    }

    private void BuildSummary() {
        var rooms = _revitRepository.GetRoomsOnActiveView();
        var dimensionTypeNames = new HashSet<string>(_revitRepository.DimensionTypes.Select(d => d.Name));

        var roomCountByType = new Dictionary<RoomTypeSettings, int>();
        int unmatchedCount = 0;
        foreach(var room in rooms) {
            var roomType = RoomTypeMatcher.Match(room, _settings);
            if(roomType is null) {
                unmatchedCount++;
                continue;
            }
            roomCountByType.TryGetValue(roomType, out int count);
            roomCountByType[roomType] = count + 1;
        }

        RoomTypeSummaries = _settings.RoomTypes
            .Select(t => new RoomTypeSummaryViewModel(
                t.Name,
                t.PurposeValue,
                roomCountByType.TryGetValue(t, out int count) ? count : 0,
                t.DimensionTypeName,
                dimensionTypeNames.Contains(t.DimensionTypeName)))
            .ToList();
        TotalRoomCount = rooms.Count;
        UnmatchedRoomCount = unmatchedCount;
    }

    private void AcceptView() {
        SaveConfig();
        string settingsSaveError = SaveDebugMode();
        var result = _dimensioningService.Run(_settings, SettingsPath);

        string message = BuildResultMessage(result);
        if(settingsSaveError != null) {
            message += Environment.NewLine + settingsSaveError;
        }
        TaskDialog.Show(Localize("RoomDimensioningWindow.Title"), message);
    }

    /// <summary>
    /// Записывает режим отладки в файл настроек, если пользователь его изменил.
    /// </summary>
    /// <returns>Текст ошибки записи или null.</returns>
    private string SaveDebugMode() {
        if(_settings.Debug.Enabled == DebugMode) {
            return null;
        }
        _settings.Debug.Enabled = DebugMode;
        try {
            _settingsReader.Save(_settings, SettingsPath);
            return null;
        } catch(InvalidOperationException ex) {
            return ex.Message;
        }
    }

    private string BuildResultMessage(RoomDimensioningResult result) {
        var lines = new List<string> {
            $"{Localize("RoomDimensioningResult.Rooms")} {result.Rooms.Count}",
            $"{Localize("RoomDimensioningResult.Unmatched")} {result.UnmatchedRoomCount}",
            $"{Localize("RoomDimensioningResult.RoomErrors")} {result.RoomErrorCount}",
            $"{Localize("RoomDimensioningResult.Lines")} {result.LineCount}",
            $"{Localize("RoomDimensioningResult.LinesWithoutReference")} {result.LinesWithoutReferenceCount}",
            $"{Localize("RoomDimensioningResult.Dimensions")} {result.DimensionOkCount} / {result.DimensionCount}",
            $"{Localize("RoomDimensioningResult.Deleted")} {result.DeletedDimensionCount}",
            $"{Localize("RoomDimensioningResult.Warnings")} {result.Warnings.Count}"
        };
        if(result.DebugLineCount > 0) {
            lines.Add($"{Localize("RoomDimensioningResult.DebugLines")} {result.DebugLineCount}");
        }
        if(result.ImagePath != null) {
            lines.Add($"{Localize("RoomDimensioningResult.Image")} {result.ImagePath}");
        }
        if(result.ImageError != null) {
            lines.Add($"{Localize("RoomDimensioningResult.ImageError")} {result.ImageError}");
        }
        if(result.DumpPath != null) {
            lines.Add($"{Localize("RoomDimensioningResult.Dump")} {result.DumpPath}");
        }
        if(result.DumpError != null) {
            lines.Add($"{Localize("RoomDimensioningResult.DumpError")} {result.DumpError}");
        }
        return string.Join(Environment.NewLine, lines);
    }

    /// <remarks>
    /// Проверки идут в порядке элементов окна сверху вниз.
    /// </remarks>
    private bool CanAcceptView() {
        if(_revitRepository.ActiveView is not ViewPlan) {
            ErrorText = Localize("RoomDimensioningWindow.ViewNotViewPlan");
            return false;
        }
        if(!string.IsNullOrEmpty(SettingsErrorText)) {
            ErrorText = SettingsErrorText;
            return false;
        }
        if(_settings is null) {
            ErrorText = Localize("RoomDimensioningWindow.SettingsFileNotSelected");
            return false;
        }
        // Проверяем типоразмеры только у типов, помещения которых есть на виде
        var missingDimensionType = RoomTypeSummaries
            .FirstOrDefault(s => s.RoomCount > 0 && !s.DimensionTypeFound);
        if(missingDimensionType != null) {
            ErrorText = $"{Localize("RoomDimensioningWindow.DimensionTypeNotFound")} " +
                        $"{missingDimensionType.DimensionTypeName}";
            return false;
        }
        if(RoomTypeSummaries.All(s => s.RoomCount == 0)) {
            ErrorText = Localize("RoomDimensioningWindow.NoMatchedRooms");
            return false;
        }
        ErrorText = string.Empty;
        return true;
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.RoomDimensioningSettingsPath = SettingsPath;
        _pluginConfig.SaveProjectConfig();
    }

    private string Localize(string key) {
        return _localizationService.GetLocalizedString(key);
    }
}
