using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Filtration;

namespace RevitSleeves.ViewModels.Settings;
internal class MepCategoryViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IFilterContextParser _filterContextParser;
    private DiameterRangeViewModel _selectedDiameterRange;

    public MepCategoryViewModel(RevitRepository revitRepository,
        ILocalizationService localizationService,
        ILanguageService languageService,
        ILogicalFilterProviderFactory filterProviderFactory,
        IFilterContextParser filterContextParser,
        MepCategorySettings mepCategorySettings) {

        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }
        if(filterProviderFactory is null) {
            throw new ArgumentNullException(nameof(filterProviderFactory));
        }
        if(mepCategorySettings is null) {
            throw new ArgumentNullException(nameof(mepCategorySettings));
        }
        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        LanguageService = languageService
            ?? throw new ArgumentNullException(nameof(languageService));
        _filterContextParser = filterContextParser
            ?? throw new ArgumentNullException(nameof(filterContextParser));

        InitializeCategory(revitRepository, localizationService, filterProviderFactory, mepCategorySettings);
        AddDiameterRangeCommand = RelayCommand.Create(AddDiameterRange);
        RemoveDiameterRangeCommand = RelayCommand.Create<DiameterRangeViewModel>(
            RemoveDiameterRange, CanRemoveDiameterRange);
    }


    public ICommand AddDiameterRangeCommand { get; }

    public ICommand RemoveDiameterRangeCommand { get; }

    public DiameterRangeViewModel SelectedDiameterRange {
        get => _selectedDiameterRange;
        set => RaiseAndSetIfChanged(ref _selectedDiameterRange, value);
    }

    public string Name { get; private set; }

    public ILogicalFilterProvider MepFilterProvider { get; private set; }

    public ILanguageService LanguageService { get; }

    public ILocalizationService LocalizationService => _localizationService;

    public ObservableCollection<DiameterRangeViewModel> DiameterRanges { get; } = [];

    public ObservableCollection<OffsetViewModel> Offsets { get; } = [];

    public StructureCategoryViewModel WallSettings { get; private set; }

    public StructureCategoryViewModel FloorSettings { get; private set; }


    private void InitializeCategory(RevitRepository revitRepository,
        ILocalizationService localizationService,
        ILogicalFilterProviderFactory filterProviderFactory,
        MepCategorySettings mepCategorySettings) {

        var category = revitRepository.GetCategory(mepCategorySettings.Category);
        var dataProvider = new FilterDataProvider(category, [revitRepository.Document]).CreateDataProvider();
        MepFilterProvider = _filterContextParser.TryParse(mepCategorySettings.MepFilterContext, out var context)
            ? filterProviderFactory.Create(dataProvider, context)
            : filterProviderFactory.Create(dataProvider);
        Name = category.Name;
        foreach(var diamRange in mepCategorySettings.DiameterRanges) {
            DiameterRanges.Add(new DiameterRangeViewModel(_localizationService, diamRange));
        }
        foreach(var pair in mepCategorySettings.Offsets) {
            Offsets.Add(new OffsetViewModel(localizationService,
                new Offset() { OffsetType = pair.OffsetType, Value = pair.Value }));
        }
        WallSettings = new StructureCategoryViewModel(revitRepository,
            localizationService, LanguageService, filterProviderFactory, _filterContextParser,
            mepCategorySettings.WallSettings);
        FloorSettings = new StructureCategoryViewModel(revitRepository,
            localizationService, LanguageService, filterProviderFactory, _filterContextParser,
            mepCategorySettings.FloorSettings);
    }

    public string GetErrorText() {
        if(DiameterRanges.Count == 0) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.DiameterRangesIsEmpty");
        }
        string diameterRangeError = DiameterRanges
            .Select(r => r.GetErrorText())
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        if(!string.IsNullOrWhiteSpace(diameterRangeError)) {
            return diameterRangeError;
        }
        if(DiameterRangesOverlap()) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.DiameterRangesOverlap");
        }
        if(Offsets.Any(o => o.Value < 0)) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.OffsetsLessThanZero");
        }
        if(!WallSettings.IsEnabled && !FloorSettings.IsEnabled) {
            return _localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.StructureCategoriesNotEnabled");
        }
        string mepFilterError = GetFilterErrorText(MepFilterProvider, Name);
        if(!string.IsNullOrWhiteSpace(mepFilterError)) {
            return mepFilterError;
        }
        string wallFilterError = GetFilterErrorText(WallSettings.FilterProvider, WallSettings.Name);
        if(!string.IsNullOrWhiteSpace(wallFilterError)) {
            return wallFilterError;
        }
        string floorFilterError = GetFilterErrorText(FloorSettings.FilterProvider, FloorSettings.Name);
        if(!string.IsNullOrWhiteSpace(floorFilterError)) {
            return floorFilterError;
        }
        return string.Empty;
    }

    public T GetSettings<T>() where T : MepCategorySettings, new() {
        return new T() {
            DiameterRanges = [.. DiameterRanges.Select(item => item.GetDiameterRange())],
            Offsets = [.. Offsets.Select(item => item.GetOffset())],
            FloorSettings = FloorSettings.GetStructureSettings<FloorSettings>(),
            WallSettings = WallSettings.GetStructureSettings<WallSettings>(),
            MepFilterContext = _filterContextParser.Serialize(MepFilterProvider.GetFilter()),
        };
    }

    private string GetFilterErrorText(ILogicalFilterProvider filterProvider, string categoryName) {
        if(filterProvider.CanGetFilter(out var errors)) {
            return string.Empty;
        }
        if(errors.Length == 0) {
            return string.Format(_localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.FilterIsEmpty"), categoryName);
        }
        return string.Format(_localizationService.GetLocalizedString(
            "SleevePlacementSettings.Validation.FilterError"), categoryName, errors[0].Message);
    }

    private void AddDiameterRange() {
        DiameterRanges.Add(new DiameterRangeViewModel(_localizationService, new DiameterRange()));
    }

    private void RemoveDiameterRange(DiameterRangeViewModel diameterRange) {
        DiameterRanges.Remove(diameterRange);
        SelectedDiameterRange = DiameterRanges.FirstOrDefault();
    }

    private bool CanRemoveDiameterRange(DiameterRangeViewModel diameterRange) {
        return diameterRange is not null;
    }

    private bool DiameterRangesOverlap() {
        for(int i = 0; i < DiameterRanges.Count; i++) {
            for(int j = i + 1; j < DiameterRanges.Count; j++) {
                if(DiameterRanges[i].Overlap(DiameterRanges[j])) {
                    return true;
                }
            }
        }
        return false;
    }
}
