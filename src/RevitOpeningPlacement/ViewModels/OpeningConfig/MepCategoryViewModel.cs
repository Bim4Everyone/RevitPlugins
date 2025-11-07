using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.TypeNamesProviders;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class MepCategoryViewModel : BaseViewModel {
    private const string _pipeDiameterDisplayName = "Внешний диаметр";
    private readonly ILocalizationService _localization;
    private string _name;
    private ObservableCollection<SizeViewModel> _minSizes;
    private ObservableCollection<OffsetViewModel> _offsets;
    private bool _isSelected;
    private ObservableCollection<StructureCategoryViewModel> _structureCategories;
    private SetViewModel _setViewModel;
    private int _selectedRounding;
    private int _selectedElevationRounding;

    public MepCategoryViewModel(RevitRepository revitRepository,
        ILocalizationService localization,
        MepCategory mepCategory) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
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
            mepCategory.Intersections.Select(c => new StructureCategoryViewModel(revitRepository, c, _localization)));
        SelectedRounding = mepCategory.Rounding;
        SelectedElevationRounding = mepCategory.ElevationRounding;
        var categoriesInfoViewModel = GetCategoriesInfoViewModel(revitRepository, Name);
        SetViewModel = new SetViewModel(revitRepository, _localization, categoriesInfoViewModel, mepCategory.Set);
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

    public SetViewModel SetViewModel {
        get => _setViewModel;
        set => RaiseAndSetIfChanged(ref _setViewModel, value);
    }

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
        if(SetViewModel.IsEmpty()) {
            return $"Поля фильтра для ВИС категории \'{Name}\' должны быть заполнены.";
        }
        var structureEmptyFilter = StructureCategories.FirstOrDefault(item => item.SetViewModel.IsEmpty());
        return structureEmptyFilter != null
            ? $"Поля фильтра категории \'{structureEmptyFilter.Name}\' для ВИС категории \'{Name}\' должны быть заполнены."
            : !string.IsNullOrEmpty(SetViewModel.GetErrorText()) ? SetViewModel.GetErrorText() : null;
    }

    public MepCategory GetMepCategory() {
        return new MepCategory(Name, ImageSource, IsRound) {
            Offsets = Offsets.Select(item => item.GetOffset()).ToList(),
            MinSizes = new SizeCollection(MinSizes.Select(item => item.GetSize())),
            IsSelected = IsSelected,
            Intersections = StructureCategories.Select(item => new StructureCategory() {
                Name = item.Name,
                IsSelected = item.IsSelected,
                Set = item.SetViewModel.GetSet()
            })
            .ToList(),
            Rounding = SelectedRounding,
            ElevationRounding = SelectedElevationRounding,
            Set = SetViewModel.GetSet()
        };
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

    private CategoriesInfoViewModel GetCategoriesInfoViewModel(RevitRepository revitRepository, string mepCategoryName) {
        var revitCategories = revitRepository.GetCategories(revitRepository.GetMepCategoryEnum(mepCategoryName));
        return new CategoriesInfoViewModel(revitRepository, _localization, revitCategories);
    }
}
