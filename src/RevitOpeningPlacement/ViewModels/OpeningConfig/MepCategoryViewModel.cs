using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Bim4Everyone.RevitFiltration;
using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Filtration;
using RevitOpeningPlacement.Models.TypeNamesProviders;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class MepCategoryViewModel : BaseViewModel {
    private const string _pipeDiameterDisplayName = "Внешний диаметр";
    private readonly ILocalizationService _localization;
    private readonly IFilterContextParser _filterContextParser;
    private string _name;
    private ObservableCollection<SizeViewModel> _minSizes;
    private ObservableCollection<OffsetViewModel> _offsets;
    private bool _isSelected;
    private ObservableCollection<StructureCategoryViewModel> _structureCategories;
    private int _selectedRounding;
    private int _selectedElevationRounding;

    public MepCategoryViewModel(RevitRepository revitRepository,
        ILocalizationService localization,
        ILanguageService languageService,
        ILogicalFilterProviderFactory filterProviderFactory,
        IFilterContextParser filterContextParser,
        ILogicalFilterFactory filterFactory,
        MepCategory mepCategory) {
        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }
        if(filterProviderFactory is null) {
            throw new ArgumentNullException(nameof(filterProviderFactory));
        }
        if(filterFactory is null) {
            throw new ArgumentNullException(nameof(filterFactory));
        }
        if(mepCategory is null) {
            throw new ArgumentNullException(nameof(mepCategory));
        }
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _filterContextParser = filterContextParser ?? throw new ArgumentNullException(nameof(filterContextParser));

        Name = mepCategory.Name;
        ImageSource = Path.GetFileName(mepCategory.ImageSource);
        MinSizes = new ObservableCollection<SizeViewModel>(
            mepCategory.MinSizes.Select(item => new SizeViewModel(item)));
        IsRound = mepCategory.IsRound;
        IsSelected = mepCategory.IsSelected;
        Offsets = new ObservableCollection<OffsetViewModel>(
            mepCategory.Offsets.Select(
                item => new OffsetViewModel(item, new TypeNamesProvider(mepCategory.IsRound))));
        StructureCategories = new ObservableCollection<StructureCategoryViewModel>(
            mepCategory.Intersections.Select(c => new StructureCategoryViewModel(
                revitRepository,
                c,
                _localization,
                LanguageService,
                filterProviderFactory,
                _filterContextParser,
                filterFactory)));
        SelectedRounding = mepCategory.Rounding;
        SelectedElevationRounding = mepCategory.ElevationRounding;
        InitializeFilterProvider(revitRepository, filterProviderFactory, filterFactory, mepCategory);
        RenameDisplayParameters();
        AddOffsetCommand = RelayCommand.Create(AddOffset);
        RemoveOffsetCommand = RelayCommand.Create<OffsetViewModel>(RemoveOffset, CanRemoveOffset);
    }


    public bool IsRound { get; set; }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    /// <summary>
    /// Округление габаритов задания на отверстие в мм
    /// </summary>
    public int SelectedRounding {
        get => _selectedRounding;
        set => RaiseAndSetIfChanged(ref _selectedRounding, value);
    }

    /// <summary>
    /// Округление отметки задания на отверстие в мм
    /// </summary>
    public int SelectedElevationRounding {
        get => _selectedElevationRounding;
        set => RaiseAndSetIfChanged(ref _selectedElevationRounding, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string ImageSource { get; set; }

    public ObservableCollection<SizeViewModel> MinSizes {
        get => _minSizes;
        set => RaiseAndSetIfChanged(ref _minSizes, value);
    }

    public ObservableCollection<OffsetViewModel> Offsets {
        get => _offsets;
        set => RaiseAndSetIfChanged(ref _offsets, value);
    }

    public ObservableCollection<StructureCategoryViewModel> StructureCategories {
        get => _structureCategories;
        set => RaiseAndSetIfChanged(ref _structureCategories, value);
    }

    public IReadOnlyCollection<int> Roundings { get; } = new int[] { 1, 5, 10, 25, 50 };

    /// <summary>
    /// Фильтр элементов данной категории инженерных систем
    /// </summary>
    public ILogicalFilterProvider MepFilterProvider { get; private set; }

    public ILanguageService LanguageService { get; }

    public ILocalizationService LocalizationService => _localization;

    public ICommand AddOffsetCommand { get; }
    public ICommand RemoveOffsetCommand { get; }

    public string GetErrorText() {
        string sizeError = MinSizes.Select(item => item.GetErrorText()).FirstOrDefault(item => !string.IsNullOrEmpty(item));
        if(!string.IsNullOrEmpty(sizeError)) {
            return $"У категории \"{Name}\" {sizeError}";
        }
        string offsetError = Offsets.Select(item => item.GetErrorText()).FirstOrDefault(item => !string.IsNullOrEmpty(item));
        if(!string.IsNullOrEmpty(offsetError)) {
            return $"У категории \"{Name}\" {offsetError}";
        }
        string intersectionOffsetError = GetIntersectionOffsetError();
        if(!string.IsNullOrEmpty(intersectionOffsetError)) {
            return $"У категории \"{Name}\" {intersectionOffsetError}";
        }
        if(IsSelected && StructureCategories.All(item => !item.IsSelected)) {
            return $"Для категории \"{Name}\" выберите категории для пересечения";
        }
        string mepFilterError = GetFilterErrorText(MepFilterProvider, Name);
        if(!string.IsNullOrEmpty(mepFilterError)) {
            return mepFilterError;
        }
        return StructureCategories
            .Select(item => GetFilterErrorText(item.FilterProvider, item.Name))
            .FirstOrDefault(item => !string.IsNullOrEmpty(item));
    }

    public MepCategory GetMepCategory() {
        return new MepCategory(Name, ImageSource, IsRound) {
            Offsets = Offsets.Select(item => item.GetOffset()).ToList(),
            MinSizes = new SizeCollection(MinSizes.Select(item => item.GetSize())),
            IsSelected = IsSelected,
            Intersections = StructureCategories.Select(item => item.GetStructureCategory()).ToList(),
            Rounding = SelectedRounding,
            ElevationRounding = SelectedElevationRounding,
            MepFilterContext = _filterContextParser.Serialize(MepFilterProvider.GetFilter())
        };
    }


    private void InitializeFilterProvider(
        RevitRepository revitRepository,
        ILogicalFilterProviderFactory filterProviderFactory,
        ILogicalFilterFactory filterFactory,
        MepCategory mepCategory) {

        var categories = revitRepository.GetCategories(revitRepository.GetMepCategoryEnum(Name));
        var dataProvider = new FilterDataProvider(categories, [revitRepository.Doc]).CreateDataProvider();
        // если фильтр не задан, создается пустой фильтр по заданным категориям,
        // иначе фильтр останется в состоянии ошибки, пока пользователь не откроет его в интерфейсе
        MepFilterProvider = _filterContextParser.TryParse(mepCategory.MepFilterContext, out var context)
            ? filterProviderFactory.Create(dataProvider, context)
            : filterProviderFactory.Create(
                dataProvider,
                filterFactory.CreateAndFilter(),
                [.. categories.Select(c => c.GetBuiltInCategory())]);
    }

    /// <summary>
    /// Возвращает описание ошибки фильтра заданной категории, либо пустую строку, если ошибок нет
    /// </summary>
    /// <param name="filterProvider">Фильтр категории</param>
    /// <param name="categoryName">Название категории</param>
    private string GetFilterErrorText(ILogicalFilterProvider filterProvider, string categoryName) {
        if(filterProvider.CanGetFilter(out var errors)) {
            return string.Empty;
        }
        return errors.Length == 0
            ? _localization.GetLocalizedString("OpeningConfig.Validation.FilterIsEmpty", categoryName)
            : _localization.GetLocalizedString(
                "OpeningConfig.Validation.FilterError", categoryName, errors[0].Message);
    }

    private void RenameDisplayParameters() {
        if(Name.Equals(
            RevitRepository.MepCategoryNames[MepCategoryEnum.Pipe],
            StringComparison.InvariantCultureIgnoreCase)) {

            var diameter = MinSizes.FirstOrDefault(p => p.DisplayName.Equals(
                RevitRepository.ParameterNames[Parameters.Diameter],
                StringComparison.InvariantCultureIgnoreCase));
            if(diameter != null) {
                diameter.DisplayName = _pipeDiameterDisplayName;
            }
        }
    }

    private void AddOffset() {
        Offsets.Add(new OffsetViewModel(new TypeNamesProvider(IsRound)));
    }

    private void RemoveOffset(OffsetViewModel p) {
        Offsets.Remove(p);
    }

    private bool CanRemoveOffset(OffsetViewModel p) {
        return p != null;
    }

    private string GetIntersectionOffsetError() {
        string error = null;
        for(int i = 0; i < Offsets.Count; i++) {
            for(int j = i + 1; j < Offsets.Count; j++) {
                error = Offsets[i].GetIntersectText(Offsets[j]);
                if(!string.IsNullOrEmpty(error)) {
                    return error;
                }
            }
        }
        return error;
    }
}
