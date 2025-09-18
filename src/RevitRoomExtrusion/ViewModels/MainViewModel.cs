using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomExtrusion.Models;


namespace RevitRoomExtrusion.ViewModels;
internal class MainViewModel : BaseViewModel {
    private const string _defaultHeightMm = "2200"; // Распространенная высота машино-места для одноэтажного паркинга,
                                                    // берется из задания на проектирование (ЗнП).
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly FamilyLoadOptions _familyLoadOptions;

    private string _errorText;
    private string _extrusionHeightMm;
    private string _extrusionFamilyName;
    private bool _isJoinExtrusionChecked;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        FamilyLoadOptions familyLoadOptions) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _familyLoadOptions = familyLoadOptions;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public List<Room> SelectedRooms { get; set; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    public string ExtrusionHeightMm {
        get => _extrusionHeightMm;
        set => RaiseAndSetIfChanged(ref _extrusionHeightMm, value);
    }
    public string ExtrusionFamilyName {
        get => _extrusionFamilyName;
        set => RaiseAndSetIfChanged(ref _extrusionFamilyName, value);
    }
    public bool IsJoinExtrusionChecked {
        get => _isJoinExtrusionChecked;
        set => RaiseAndSetIfChanged(ref _isJoinExtrusionChecked, value);
    }

    // Метод загрузки конфигурации
    private void LoadView() {
        LoadConfig();
    }

    // Основной метод
    private void AcceptView() {
        SaveConfig();
        SelectedRooms = _revitRepository.GetSelectedRooms();
        double extrusionHeightDouble = Convert.ToDouble(_extrusionHeightMm);
        var instanceManager = new InstanceManager(_localizationService, _revitRepository, _familyLoadOptions);
        instanceManager.CreateInstances(SelectedRooms, _extrusionFamilyName, extrusionHeightDouble, _isJoinExtrusionChecked);
    }

    // Метод проверки возможности выполнения основного метода
    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(_extrusionHeightMm)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ExtrusionHeightMm");
            return false;
        }
        if(!double.TryParse(_extrusionHeightMm, out double result)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HeightError");
            return false;
        }
        if(result < 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HeightErrorNegative");
            return false;
        } else if(result == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HeightErrorZero");
            return false;
        } else if(result > 100000) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HeightErrorBig");
            return false;
        }
        if(string.IsNullOrEmpty(_extrusionFamilyName)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.FamilyName");
            return false;
        }
        if(!NamingUtils.IsValidName(_extrusionFamilyName) ||
            _extrusionFamilyName.Any(c => Path.GetInvalidFileNameChars().Contains(c)) ||
            _extrusionFamilyName.Any(c => Path.GetInvalidPathChars().Contains(c))) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NameError");
            return false;
        }
        ErrorText = null;
        return true;
    }

    // Метод загрузки конфигурации
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        ExtrusionHeightMm = setting?.ExtrusionHeightMm ?? _defaultHeightMm;
        ExtrusionFamilyName = setting?.ExtrusionFamilyName
            ?? _localizationService.GetLocalizedString("MainViewModel.DefaultFamilyName");
        IsJoinExtrusionChecked = setting?.IsJoinExtrusionChecked ?? true;
    }

    // Метод сохранения конфигурации
    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ExtrusionHeightMm = ExtrusionHeightMm;
        setting.ExtrusionFamilyName = ExtrusionFamilyName;
        setting.IsJoinExtrusionChecked = IsJoinExtrusionChecked;
        _pluginConfig.SaveProjectConfig();
    }
}
