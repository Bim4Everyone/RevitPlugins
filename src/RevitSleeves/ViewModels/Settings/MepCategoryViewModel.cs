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
                category));
        Name = category.Name;
        foreach(var diamRange in mepCategorySettings.DiameterRanges) {
            DiameterRanges.Add(new DiameterRangeViewModel(_localizationService, diamRange));
        }
        foreach(var pair in mepCategorySettings.Offsets) {
            Offsets.Add(new OffsetViewModel(localizationService,
                new Offset() { OffsetType = pair.Key, Value = pair.Value }));
        }
        WallSettings = new StructureCategoryViewModel(revitRepository,
            localizationService,
            mepCategorySettings.WallSettings);
        FloorSettings = new StructureCategoryViewModel(revitRepository,
            localizationService,
            mepCategorySettings.FloorSettings);
    }

    public string GetErrorText() {
        // TODO
        //var sizeError = MinSizes.Select(item => item.GetErrorText()).FirstOrDefault(item => !string.IsNullOrEmpty(item));
        //if(!string.IsNullOrEmpty(sizeError)) {
        //    return $"У категории \"{Name}\" {sizeError}";
        //}
        //var offsetError = Offsets.Select(item => item.GetErrorText()).FirstOrDefault(item => !string.IsNullOrEmpty(item));
        //if(!string.IsNullOrEmpty(offsetError)) {
        //    return $"У категории \"{Name}\" {offsetError}";
        //}
        //var intersectionOffsetError = GetIntersectionOffsetError();
        //if(!string.IsNullOrEmpty(intersectionOffsetError)) {
        //    return $"У категории \"{Name}\" {intersectionOffsetError}";
        //}
        //if(IsSelected && StructureCategories.All(item => !item.IsSelected)) {
        //    return $"Для категории \"{Name}\" выберите категории для пересечения";
        //}
        //if(SetViewModel.IsEmpty()) {
        //    return $"Поля фильтра для ВИС категории \'{Name}\' должны быть заполнены.";
        //}
        //var structureEmptyFilter = StructureCategories.FirstOrDefault(item => item.SetViewModel.IsEmpty());
        //if(structureEmptyFilter != null) {
        //    return $"Поля фильтра категории \'{structureEmptyFilter.Name}\' для ВИС категории \'{Name}\' должны быть заполнены.";
        //}
        //if(!string.IsNullOrEmpty(SetViewModel.GetErrorText())) {
        //    return SetViewModel.GetErrorText();
        //}
        return null;
    }

    public T GetSettings<T>() where T : MepCategorySettings, new() {
        return new T() {
            DiameterRanges = [.. DiameterRanges.Select(item => item.GetDiameterRange())],
            Offsets = Offsets.Select(item => item.GetOffset())
                .ToDictionary(offset => offset.OffsetType, offset => offset.Value),
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
}
