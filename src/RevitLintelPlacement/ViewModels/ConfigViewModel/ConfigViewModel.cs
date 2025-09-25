using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels;

internal class ConfigViewModel : BaseViewModel {
    private readonly bool _canChangeSelection = true;
    private readonly RevitRepository _revitRepository;
    private string _holesFilter;
    private ParameterViewModel _lintelFixation;
    private List<ParameterViewModel> _lintelFixationParameters;
    private ParameterViewModel _lintelLeftCorner;
    private List<ParameterViewModel> _lintelLeftCornerParameters;
    private ParameterViewModel _lintelLeftOffset;
    private List<ParameterViewModel> _lintelLeftOffsetParameters;
    private ParameterViewModel _lintelRightCorner;
    private List<ParameterViewModel> _lintelRightCornerParameters;
    private ParameterViewModel _lintelRightOffset;
    private List<ParameterViewModel> _lintelRightOffsetParameters;
    private string _lintelsConfigPath;
    private ParameterViewModel _lintelThickness;
    private List<ParameterViewModel> _lintelThicknessParameters;
    private List<GenericModelFamilyViewModel> _lintelTypes;
    private ParameterViewModel _lintelWidth;
    private List<ParameterViewModel> _lintelWidthParameters;
    private string _message;
    private string _openingFixation;
    private string _openingHeight;
    private string _openingWidth;
    private ObservableCollection<FilterViewModel> _reinforcedConcreteFilter;
    private GenericModelFamilyViewModel _selectedFamily;

    public ConfigViewModel() {
    }

    public ConfigViewModel(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
        LintelFamilies = new List<GenericModelFamilyViewModel>(
            revitRepository.GetGenericModelFamilies()
                .Select(f => new GenericModelFamilyViewModel { Name = f.Name })
                .OrderBy(f => f.Name));
        Initialize();
        InitializeParameters();
        SaveConfigCommand = new RelayCommand(Save, CanSave);
        AddFilterCommand = new RelayCommand(AddFilter);
        RemoveFilterCommand = new RelayCommand(RemoveFilter);
        SelectLintelsConfigCommand = new RelayCommand(SelectLintelsConfig);
        FamilySelectionChangedCommand = new RelayCommand(FamilySelectionChanged, p => _canChangeSelection);
    }

    public ParameterViewModel LintelThickness {
        get => _lintelThickness;
        set => RaiseAndSetIfChanged(ref _lintelThickness, value);
    }

    public ParameterViewModel LintelWidth {
        get => _lintelWidth;
        set => RaiseAndSetIfChanged(ref _lintelWidth, value);
    }

    public ParameterViewModel LintelRightOffset {
        get => _lintelRightOffset;
        set => RaiseAndSetIfChanged(ref _lintelRightOffset, value);
    }

    public ParameterViewModel LintelLeftOffset {
        get => _lintelLeftOffset;
        set => RaiseAndSetIfChanged(ref _lintelLeftOffset, value);
    }

    public ParameterViewModel LintelLeftCorner {
        get => _lintelLeftCorner;
        set => RaiseAndSetIfChanged(ref _lintelLeftCorner, value);
    }

    public ParameterViewModel LintelRightCorner {
        get => _lintelRightCorner;
        set => RaiseAndSetIfChanged(ref _lintelRightCorner, value);
    }

    public ParameterViewModel LintelFixation {
        get => _lintelFixation;
        set => RaiseAndSetIfChanged(ref _lintelFixation, value);
    }

    public string OpeningHeight {
        get => _openingHeight;
        set => RaiseAndSetIfChanged(ref _openingHeight, value);
    }

    public string OpeningWidth {
        get => _openingWidth;
        set => RaiseAndSetIfChanged(ref _openingWidth, value);
    }

    public string OpeningFixation {
        get => _openingFixation;
        set => RaiseAndSetIfChanged(ref _openingFixation, value);
    }

    public string Message {
        get => _message;
        set => RaiseAndSetIfChanged(ref _message, value);
    }

    public string HolesFilter {
        get => _holesFilter;
        set => RaiseAndSetIfChanged(ref _holesFilter, value);
    }

    public string LintelsConfigPath {
        get => _lintelsConfigPath;
        set => RaiseAndSetIfChanged(ref _lintelsConfigPath, value);
    }

    public GenericModelFamilyViewModel SelectedFamily {
        get => _selectedFamily;
        set => RaiseAndSetIfChanged(ref _selectedFamily, value);
    }

    public ObservableCollection<FilterViewModel> ReinforcedConcreteFilter {
        get => _reinforcedConcreteFilter;
        set => RaiseAndSetIfChanged(ref _reinforcedConcreteFilter, value);
    }

    public List<GenericModelFamilyViewModel> LintelFamilies {
        get => _lintelTypes;
        set => RaiseAndSetIfChanged(ref _lintelTypes, value);
    }

    public List<ParameterViewModel> LintelThicknessParameters {
        get => _lintelThicknessParameters;
        set => RaiseAndSetIfChanged(ref _lintelThicknessParameters, value);
    }

    public List<ParameterViewModel> LintelWidthParameters {
        get => _lintelWidthParameters;
        set => RaiseAndSetIfChanged(ref _lintelWidthParameters, value);
    }

    public List<ParameterViewModel> LintelRightOffsetParameters {
        get => _lintelRightOffsetParameters;
        set => RaiseAndSetIfChanged(ref _lintelRightOffsetParameters, value);
    }

    public List<ParameterViewModel> LintelLeftOffsetParameters {
        get => _lintelLeftOffsetParameters;
        set => RaiseAndSetIfChanged(ref _lintelLeftOffsetParameters, value);
    }

    public List<ParameterViewModel> LintelLeftCornerParameters {
        get => _lintelLeftCornerParameters;
        set => RaiseAndSetIfChanged(ref _lintelLeftCornerParameters, value);
    }

    public List<ParameterViewModel> LintelRightCornerParameters {
        get => _lintelRightCornerParameters;
        set => RaiseAndSetIfChanged(ref _lintelRightCornerParameters, value);
    }

    public List<ParameterViewModel> LintelFixationParameters {
        get => _lintelFixationParameters;
        set => RaiseAndSetIfChanged(ref _lintelFixationParameters, value);
    }

    public ICommand SaveConfigCommand { get; set; }
    public ICommand AddFilterCommand { get; set; }
    public ICommand RemoveFilterCommand { get; set; }
    public ICommand AddRulePathCommand { get; set; }
    public ICommand RemoveRulePathCommand { get; set; }
    public ICommand SelectLintelsConfigCommand { get; set; }
    public ICommand FamilySelectionChangedCommand { get; set; }

    private void Initialize() {
        OpeningHeight = _revitRepository.LintelsCommonConfig.OpeningHeight;
        OpeningWidth = _revitRepository.LintelsCommonConfig.OpeningWidth;
        OpeningFixation = _revitRepository.LintelsCommonConfig.OpeningFixation;
        if(_revitRepository.LintelsCommonConfig.ReinforcedConcreteFilter == null
           || _revitRepository.LintelsCommonConfig.ReinforcedConcreteFilter.Count == 0) {
            ReinforcedConcreteFilter = new ObservableCollection<FilterViewModel>(
                new List<FilterViewModel> { new() { Name = "Железобетон" } });
        } else {
            ReinforcedConcreteFilter = new ObservableCollection<FilterViewModel>(
                new List<FilterViewModel>(
                    _revitRepository.LintelsCommonConfig.ReinforcedConcreteFilter
                        .Select(e => new FilterViewModel { Name = e })));
        }

        HolesFilter = _revitRepository.LintelsCommonConfig.HolesFilter;
        if(!string.IsNullOrEmpty(_revitRepository.LintelsCommonConfig.LintelFamily)) {
            SelectedFamily = LintelFamilies
                .FirstOrDefault(item => item.Name.Equals(
                    _revitRepository.LintelsCommonConfig.LintelFamily,
                    StringComparison.CurrentCultureIgnoreCase));
        }
    }

    private void InitializeParameters() {
        var allParameters = _revitRepository
            .GetParametersFromFamilies(SelectedFamily?.Name)
            .OrderBy(p => p.Name)
            .ToList();
        LintelThicknessParameters = allParameters
            .Where(p => p.StorageType == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelThickness)])
            .ToList();
        LintelWidthParameters = allParameters
            .Where(p => p.StorageType == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelWidth)])
            .ToList();
        LintelRightCornerParameters = allParameters
            .Where(p => p.StorageType
                        == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelRightCorner)])
            .ToList();
        LintelRightOffsetParameters = allParameters
            .Where(p => p.StorageType
                        == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelRightOffset)])
            .ToList();
        LintelLeftCornerParameters = allParameters
            .Where(p => p.StorageType
                        == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelLeftCorner)])
            .ToList();
        LintelLeftOffsetParameters = allParameters
            .Where(p => p.StorageType
                        == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelLeftOffset)])
            .ToList();
        LintelFixationParameters = allParameters
            .Where(p => p.StorageType == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelFixation)])
            .ToList();

        LintelThickness = LintelThicknessParameters
            .FirstOrDefault(item => item.Name == _revitRepository.LintelsCommonConfig.LintelThickness
                                    && item.StorageType
                                    == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelThickness)]);
        LintelWidth = LintelWidthParameters
            .FirstOrDefault(item => item.Name == _revitRepository.LintelsCommonConfig.LintelWidth
                                    && item.StorageType
                                    == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelWidth)]);
        LintelRightCorner = LintelRightCornerParameters
            .FirstOrDefault(item => item.Name == _revitRepository.LintelsCommonConfig.LintelRightCorner
                                    && item.StorageType
                                    == _revitRepository.LintelsCommonConfig
                                        .ParamterType[nameof(LintelRightCorner)]);
        LintelRightOffset = LintelRightOffsetParameters
            .FirstOrDefault(item => item.Name == _revitRepository.LintelsCommonConfig.LintelRightOffset
                                    && item.StorageType
                                    == _revitRepository.LintelsCommonConfig
                                        .ParamterType[nameof(LintelRightOffset)]);
        LintelLeftCorner = LintelLeftCornerParameters
            .FirstOrDefault(item => item.Name == _revitRepository.LintelsCommonConfig.LintelLeftCorner
                                    && item.StorageType
                                    == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelLeftCorner)]);
        LintelLeftOffset = LintelLeftOffsetParameters
            .FirstOrDefault(item => item.Name == _revitRepository.LintelsCommonConfig.LintelLeftOffset
                                    && item.StorageType
                                    == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelLeftOffset)]);
        LintelFixation = LintelFixationParameters
            .FirstOrDefault(item => item.Name == _revitRepository.LintelsCommonConfig.LintelFixation
                                    && item.StorageType
                                    == _revitRepository.LintelsCommonConfig.ParamterType[nameof(LintelFixation)]);
    }

    private void Save(object p) {
        _revitRepository.LintelsCommonConfig.LintelThickness = LintelThickness.Name;
        _revitRepository.LintelsCommonConfig.LintelWidth = LintelWidth.Name;
        _revitRepository.LintelsCommonConfig.LintelRightCorner = LintelRightCorner.Name;
        _revitRepository.LintelsCommonConfig.LintelRightOffset = LintelRightOffset.Name;
        _revitRepository.LintelsCommonConfig.LintelLeftCorner = LintelLeftCorner.Name;
        _revitRepository.LintelsCommonConfig.LintelLeftOffset = LintelLeftOffset.Name;
        _revitRepository.LintelsCommonConfig.LintelFixation = LintelFixation.Name;
        _revitRepository.LintelsCommonConfig.OpeningFixation = OpeningFixation;
        _revitRepository.LintelsCommonConfig.ReinforcedConcreteFilter =
            ReinforcedConcreteFilter.Select(e => e.Name).ToList();
        _revitRepository.LintelsCommonConfig.HolesFilter = HolesFilter;
        _revitRepository.LintelsCommonConfig.LintelFamily = SelectedFamily?.Name ?? string.Empty;
        _revitRepository.LintelsCommonConfig.Save(_revitRepository.GetDocumentName());
        _revitRepository.LintelsConfig.SaveProjectConfig();
        Message = "Настройки успешно сохранены";
        if(p is Window window) {
            window.Close();
        }
    }

    private bool CanSave(object p) {
        if(!string.IsNullOrEmpty(LintelThickness?.Name)
           && !string.IsNullOrEmpty(LintelWidth?.Name)
           && !string.IsNullOrEmpty(LintelRightCorner?.Name)
           && !string.IsNullOrEmpty(LintelRightOffset?.Name)
           && !string.IsNullOrEmpty(LintelLeftCorner?.Name)
           && !string.IsNullOrEmpty(LintelLeftOffset?.Name)
           && !string.IsNullOrEmpty(LintelFixation?.Name)
           && !string.IsNullOrEmpty(OpeningHeight)
           && !string.IsNullOrEmpty(OpeningWidth)
           && !string.IsNullOrEmpty(OpeningFixation)
           && !string.IsNullOrEmpty(HolesFilter)
           && !string.IsNullOrEmpty(SelectedFamily?.Name)
           && ReinforcedConcreteFilter.All(item => !string.IsNullOrEmpty(item.Name))) {
            Message = string.Empty;
            return true;
        }

        Message = "Все настройки должны быть заполнены";
        return false;
    }

    private void AddFilter(object p) {
        ReinforcedConcreteFilter.Add(new FilterViewModel());
    }

    private void RemoveFilter(object p) {
        if(ReinforcedConcreteFilter.Count > 0) {
            ReinforcedConcreteFilter.Remove(ReinforcedConcreteFilter.Last());
        }
    }

    private void SelectLintelsConfig(object p) {
        using(var dialog = new FolderBrowserDialog()) {
            dialog.SelectedPath = Path.GetDirectoryName(LintelsConfigPath);
            if(dialog.ShowDialog() == DialogResult.OK) {
                if(dialog.SelectedPath != LintelsConfigPath) {
                    LintelsConfigPath = dialog.SelectedPath;
                    _revitRepository.LintelsCommonConfig =
                        LintelsCommonConfig.GetLintelsCommonConfig(LintelsConfigPath);
                    Initialize();
                }
            }
        }
    }

    private void FamilySelectionChanged(object p) {
        InitializeParameters();
    }
}

internal class FilterViewModel : BaseViewModel {
    private string _name;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}

internal class ParameterViewModel : IEquatable<ParameterViewModel> {
    public StorageType StorageType { get; set; }
    public string Name { get; set; }

    public bool Equals(ParameterViewModel other) {
        return other != null && StorageType == other.StorageType && Name == other.Name;
    }

    public override int GetHashCode() {
        int hashCode = 897683154;
        hashCode = hashCode * -1521134295 + StorageType.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        return hashCode;
    }
}
