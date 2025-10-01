using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class StructureCategoryViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly StructureCategory _structureCategory;

    public StructureCategoryViewModel(RevitRepository revitRepository, StructureCategory structureCategory) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _structureCategory = structureCategory ?? throw new System.ArgumentNullException(nameof(structureCategory));

        _name = _structureCategory.Name;
        _isSelected = _structureCategory.IsSelected;
        var categoriesInfo = GetCategoriesInfoViewModel(_revitRepository, _name);
        _setViewModel = new SetViewModel(_revitRepository, categoriesInfo, _structureCategory.Set);
    }


    private bool _isSelected;
    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    private string _name;
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    private SetViewModel _setViewModel;
    public SetViewModel SetViewModel {
        get => _setViewModel;
        set => RaiseAndSetIfChanged(ref _setViewModel, value);
    }


    private CategoriesInfoViewModel GetCategoriesInfoViewModel(
        RevitRepository revitRepository,
        string structureCategoryName) {

        var revitCategories = revitRepository.GetCategories(
            revitRepository.GetStructureCategoryEnum(structureCategoryName));
        return new CategoriesInfoViewModel(revitRepository, revitCategories);
    }
}
