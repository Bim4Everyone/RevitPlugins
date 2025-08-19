using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.ViewModels.Filtration;

namespace RevitSleeves.ViewModels.Settings;
internal class MepCategoryViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly MepCategorySettings _mepCategorySettings;
    private DiameterRangeViewModel _selectedDiameterRange;

    public MepCategoryViewModel(RevitRepository revitRepository,
        ILocalizationService localizationService,
        MepCategorySettings mepCategorySettings) {

        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        _mepCategorySettings = mepCategorySettings
            ?? throw new System.ArgumentNullException(nameof(mepCategorySettings));

        InitializeCategory(_revitRepository, _localizationService, _mepCategorySettings);
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

    public SetViewModel MepFilterViewModel { get; private set; }

    public ObservableCollection<DiameterRangeViewModel> DiameterRanges { get; } = [];

    public ObservableCollection<OffsetViewModel> Offsets { get; } = [];

    public StructureCategoryViewModel WallSettings { get; private set; }

    public StructureCategoryViewModel FloorSettings { get; private set; }


    private void InitializeCategory(RevitRepository revitRepository,
        ILocalizationService localizationService,
        MepCategorySettings mepCategorySettings) {

        var category = revitRepository.GetCategory(mepCategorySettings.Category);
        MepFilterViewModel = new SetViewModel(
            revitRepository,
            localizationService,
            new CategoryInfoViewModel(
                revitRepository,
                localizationService,
                category),
            mepCategorySettings.MepFilterSet);
        Name = category.Name;
        foreach(var diamRange in mepCategorySettings.DiameterRanges) {
            DiameterRanges.Add(new DiameterRangeViewModel(_localizationService, diamRange));
        }
        foreach(var pair in mepCategorySettings.Offsets) {
            Offsets.Add(new OffsetViewModel(localizationService,
                new Offset() { OffsetType = pair.OffsetType, Value = pair.Value }));
        }
        WallSettings = new StructureCategoryViewModel(revitRepository,
            localizationService,
            mepCategorySettings.WallSettings);
        FloorSettings = new StructureCategoryViewModel(revitRepository,
            localizationService,
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
        if(MepFilterViewModel.IsEmpty()) {
            return string.Format(_localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.FilterIsEmpty"), Name);
        }
        if(WallSettings.StructureFilterViewModel.IsEmpty()) {
            return string.Format(_localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.FilterIsEmpty"), WallSettings.Name);
        }
        if(FloorSettings.StructureFilterViewModel.IsEmpty()) {
            return string.Format(_localizationService.GetLocalizedString(
                "SleevePlacementSettings.Validation.FilterIsEmpty"), FloorSettings.Name);
        }
        string mepFilterError = MepFilterViewModel.GetErrorText();
        if(!string.IsNullOrWhiteSpace(mepFilterError)) {
            return mepFilterError;
        }
        string wallFilterError = WallSettings.StructureFilterViewModel.GetErrorText();
        if(!string.IsNullOrWhiteSpace(wallFilterError)) {
            return wallFilterError;
        }
        string floorFilterError = FloorSettings.StructureFilterViewModel.GetErrorText();
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
            MepFilterSet = MepFilterViewModel.GetSet(),
        };
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
