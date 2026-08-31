using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.ViewModels;

internal class FamilySettingsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ILocalizationService _localizationService;
    
    private ObservableCollection<FamilyViewModel> _lintelFamilyViewModels;
    private FamilyViewModel _selectedLintelFamilyViewModel;
    
    private ObservableCollection<ParamViewModel> _lintelParametersDouble;
    private ObservableCollection<ParamViewModel> _lintelParametersInteger;
    private ObservableCollection<ParamViewModel> _openingParameters;
    private ParamViewModel _selectedLintelWidthParam;
    private ParamViewModel _selectedLintelThicknessParam;
    private ParamViewModel _selectedLintelRightOffsetParam;
    private ParamViewModel _selectedLintelLeftOffsetParam;
    private ParamViewModel _selectedLintelRightCornerParam;
    private ParamViewModel _selectedLintelLeftCornerParam;
    private ParamViewModel _selectedLintelRightWeldingParam;
    private ParamViewModel _selectedLintelLeftWeldingParam;
    
    private ParamViewModel _selectedOpeningHeightParam;
    private ParamViewModel _selectedOpeningWidthParam;
    
    private ObservableCollection<FamilyViewModel> _openingFamilyViewModels;
    private ObservableCollection<FamilyViewModel> _selectedOpeningFamilyViewModels;

    internal FamilySettingsViewModel(
        RevitRepository revitRepository, 
        SystemPluginConfig systemPluginConfig,
        ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _localizationService = localizationService;
        PropertyChanged += OnPropertyChanged;
        LoadView();
    }
    
    public ObservableCollection<FamilyViewModel> LintelFamilyViewModels {
        get => _lintelFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _lintelFamilyViewModels, value);
    }
    
    public FamilyViewModel SelectedLintelFamilyViewModel {
        get => _selectedLintelFamilyViewModel;
        set => RaiseAndSetIfChanged(ref _selectedLintelFamilyViewModel, value);
    }
    
    public ObservableCollection<ParamViewModel> LintelParametersDouble {
        get => _lintelParametersDouble;
        set => RaiseAndSetIfChanged(ref _lintelParametersDouble, value);
    }
    
    public ObservableCollection<ParamViewModel> LintelParametersInteger {
        get => _lintelParametersInteger;
        set => RaiseAndSetIfChanged(ref _lintelParametersInteger, value);
    }
    
    public ObservableCollection<ParamViewModel> OpeningParameters {
        get => _openingParameters;
        set => RaiseAndSetIfChanged(ref _openingParameters, value);
    }
    
    public ParamViewModel SelectedLintelWidthParam {
        get => _selectedLintelWidthParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelWidthParam, value);
    }
    
    public ParamViewModel SelectedLintelThicknessParam {
        get => _selectedLintelThicknessParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelThicknessParam, value);
    }
    
    public ParamViewModel SelectedLintelRightOffsetParam {
        get => _selectedLintelRightOffsetParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelRightOffsetParam, value);
    }
    
    public ParamViewModel SelectedLintelLeftOffsetParam {
        get => _selectedLintelLeftOffsetParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelLeftOffsetParam, value);
    }
    
    public ParamViewModel SelectedLintelRightCornerParam {
        get => _selectedLintelRightCornerParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelRightCornerParam, value);
    }
    
    public ParamViewModel SelectedLintelLeftCornerParam {
        get => _selectedLintelLeftCornerParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelLeftCornerParam, value);
    }
    
    public ParamViewModel SelectedLintelRightWeldingParam {
        get => _selectedLintelRightWeldingParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelRightWeldingParam, value);
    }
    
    public ParamViewModel SelectedLintelLeftWeldingParam {
        get => _selectedLintelLeftWeldingParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelLeftWeldingParam, value);
    }
    
    public ObservableCollection<FamilyViewModel> OpeningFamilyViewModels {
        get => _openingFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _openingFamilyViewModels, value);
    }
    
    public ObservableCollection<FamilyViewModel> SelectedOpeningFamilyViewModels {
        get => _selectedOpeningFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _selectedOpeningFamilyViewModels, value);
    }
    
    public ParamViewModel SelectedOpeningHeightParam {
        get => _selectedOpeningHeightParam;
        set => RaiseAndSetIfChanged(ref _selectedOpeningHeightParam, value);
    }
    
    public ParamViewModel SelectedOpeningWidthParam {
        get => _selectedOpeningWidthParam;
        set => RaiseAndSetIfChanged(ref _selectedOpeningWidthParam, value);
    }
    
    private IEnumerable<FamilyViewModel> GetLintelFamiliesViewModels() {
        var lintelFamilies = _revitRepository.LintelFamilies.ToArray();
        if(lintelFamilies.Length == 0) {
            return [
                new FamilyViewModel {
                    Name = _localizationService.GetLocalizedString("FamilySettingsViewModel.NoLintelFamilies"),
                    Family = null,
                    OrderedParams = null
                }];
        }
        return lintelFamilies
            .Select(revitFamily => new FamilyViewModel {
                Name = revitFamily.Family.Name,
                Family = revitFamily.Family,
                OrderedParams = revitFamily.OrderedParams
            });
    } 
    
    private IEnumerable<FamilyViewModel> GetOpeningFamiliesViewModels() {
        var openingFamilies = _revitRepository.OpeningFamilies.ToArray();
        if(openingFamilies.Length == 0) {
            return [
                new FamilyViewModel {
                Name = _localizationService.GetLocalizedString("FamilySettingsViewModel.NoOpeningFamilies"),
                Family = null,
                OrderedParams = null
            }];
        }
        return openingFamilies
            .Select(revitFamily => new FamilyViewModel {
                Name = revitFamily.Family.Name,
                Family = revitFamily.Family,
                OrderedParams = revitFamily.OrderedParams
            });
    }

    private IEnumerable<ParamViewModel> GetLintelParamViewModels(FamilyViewModel familyViewModel, StorageType storageType) {
        return new[] {
            new ParamViewModel {
                Name = _localizationService.GetLocalizedString("FamilySettingsViewModel.ParamNoSelect"),
                RevitParam = null
            }
        }.Concat(
            familyViewModel.OrderedParams
                .Where(param => param.StorageType == storageType)
                .Select(param => new ParamViewModel {
                Name = param?.Name,
                RevitParam = param
            }));
    }

    private IEnumerable<ParamViewModel> GetOpeningParamViewModels(ObservableCollection<FamilyViewModel> familyViewModels, StorageType storageType) {

        var selectedFamilies = familyViewModels
            .Where(vm => vm.IsChecked)
            .ToList();

        if(selectedFamilies.Count == 0) {
            return new[] {
                new ParamViewModel {
                    Name = _localizationService.GetLocalizedString("FamilySettingsViewModel.ParamNoSelect"),
                    RevitParam = null
                }
            };
        }

        var commonParamNames = selectedFamilies
            .Select(vm => vm.OrderedParams
                .Where(param => param.StorageType == storageType)
                .Select(param => param.Name)
                .ToHashSet())
            .Aggregate((common, current) => common.Intersect(current).ToHashSet());

        var firstFamily = selectedFamilies.First();

        return new[] {
            new ParamViewModel {
                Name = _localizationService.GetLocalizedString("FamilySettingsViewModel.ParamNoSelect"),
                RevitParam = null
            }
        }.Concat(
            firstFamily.OrderedParams
                .Where(param =>
                    param.StorageType == storageType && commonParamNames.Contains(param.Name))
                .Select(param => new ParamViewModel {
                    Name = param.Name,
                    RevitParam = param
                }));
    }

    private void OnOpeningFamilyPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not FamilyViewModel vm) {
            return;
        }

        if(e.PropertyName != nameof(vm.IsChecked)) {
            return;
        }

        if(vm.IsChecked) {
            if(SelectedOpeningFamilyViewModels.Contains(vm)) {
                return;
            }
            SelectedOpeningFamilyViewModels.Add(vm);
        } else {
            SelectedOpeningFamilyViewModels.Remove(vm);
        }

        UpdateOpeningParameters();
    }
    
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(SelectedLintelFamilyViewModel):
                UpdateLintelParameters();
                break;
            case nameof(SelectedOpeningFamilyViewModels):
                UpdateOpeningParameters();
                break;
        }
    }

    private void UpdateLintelParameters() {
        LintelParametersDouble = new ObservableCollection<ParamViewModel>(GetLintelParamViewModels(SelectedLintelFamilyViewModel, StorageType.Double));
        SelectedLintelWidthParam = LintelParametersDouble
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelWidthParamName))
                                   ?? LintelParametersDouble.FirstOrDefault();
        SelectedLintelThicknessParam = LintelParametersDouble
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelThicknessParamName))
                                       ?? LintelParametersDouble.FirstOrDefault();
        SelectedLintelRightOffsetParam = LintelParametersDouble
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelRightOffsetParamName))
                                         ?? LintelParametersDouble.FirstOrDefault();
        SelectedLintelLeftOffsetParam = LintelParametersDouble
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelLeftOffsetParamName))
                                        ?? LintelParametersDouble.FirstOrDefault();
        LintelParametersInteger = new ObservableCollection<ParamViewModel>(GetLintelParamViewModels(SelectedLintelFamilyViewModel, StorageType.Integer));
        SelectedLintelRightCornerParam = LintelParametersInteger
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelRightCornerParamName))
                                         ?? LintelParametersInteger.FirstOrDefault();
        SelectedLintelLeftCornerParam = LintelParametersInteger
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelLeftCornerParamName))
                                        ?? LintelParametersInteger.FirstOrDefault();
        SelectedLintelRightWeldingParam = LintelParametersInteger
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelRightWeldingParamName))
                                          ?? LintelParametersInteger.FirstOrDefault();
        SelectedLintelLeftWeldingParam = LintelParametersInteger
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.LintelLeftWeldingParamName))
                                         ?? LintelParametersInteger.FirstOrDefault();
    }

    private void UpdateOpeningParameters() {
        OpeningParameters = new ObservableCollection<ParamViewModel>(GetOpeningParamViewModels(SelectedOpeningFamilyViewModels, StorageType.Double));
        SelectedOpeningHeightParam = OpeningParameters
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.OpeningHeightParamName))
                                       ?? OpeningParameters.FirstOrDefault();
        SelectedOpeningWidthParam = OpeningParameters
            .FirstOrDefault(vm => vm.Name.Equals(_systemPluginConfig.OpeningWidthParamName))
                                      ?? OpeningParameters.FirstOrDefault();
    }

    private void LoadView() {
        LintelFamilyViewModels = new ObservableCollection<FamilyViewModel>(GetLintelFamiliesViewModels());
        SelectedLintelFamilyViewModel = LintelFamilyViewModels
            .FirstOrDefault(lintelFamilyViewModel => lintelFamilyViewModel.Family.Name.Equals(_systemPluginConfig.DefaultLintelFamilyName));
        
        OpeningFamilyViewModels = new ObservableCollection<FamilyViewModel>(GetOpeningFamiliesViewModels());
        SelectedOpeningFamilyViewModels = [];
        foreach(var item in OpeningFamilyViewModels) {
            item.PropertyChanged += OnOpeningFamilyPropertyChanged;
            if(item.IsChecked) {
                SelectedOpeningFamilyViewModels.Add(item);
            }
        }
        UpdateLintelParameters();
        UpdateOpeningParameters();
    }
}
