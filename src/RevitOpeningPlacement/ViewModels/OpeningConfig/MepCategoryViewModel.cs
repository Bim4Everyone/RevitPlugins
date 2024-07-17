using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.TypeNamesProviders;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class MepCategoryViewModel : BaseViewModel {
        private string _name;
        private ObservableCollection<SizeViewModel> _minSizes;
        private ObservableCollection<OffsetViewModel> _offsets;
        private bool _isSelected;
        private List<StructureCategoryViewModel> _structureCategories;
        private SetViewModel _setViewModel;

        public MepCategoryViewModel(RevitRepository revitRepository, MepCategory mepCategory) {
            InitializeStructureCategories();

            Name = mepCategory.Name;
            ImageSource = mepCategory.ImageSource;
            MinSizes = new ObservableCollection<SizeViewModel>(mepCategory.MinSizes.Select(item => new SizeViewModel(item)));
            IsRound = mepCategory.IsRound;
            IsSelected = mepCategory.IsSelected;
            Offsets = new ObservableCollection<OffsetViewModel>(mepCategory.Offsets.Select(item => new OffsetViewModel(item, new TypeNamesProvider(mepCategory.IsRound))));
            SetSelectedCategories(mepCategory);
            SelectedRounding = mepCategory.Rounding;
            var categoriesInfoViewModel = GetCategoriesInfoViewModel(revitRepository, Name);
            SetViewModel = new SetViewModel(revitRepository, categoriesInfoViewModel, mepCategory.Set);

            AddOffsetCommand = new RelayCommand(AddOffset);
            RemoveOffsetCommand = new RelayCommand(RemoveOffset, CanRemoveOffset);
        }

        public MepCategoryViewModel(RevitRepository revitRepository, string name, Parameters[] minSizesParameters, bool isRound, string imageSource) {
            InitializeStructureCategories();

            Name = name;
            ImageSource = imageSource;
            MinSizes = GetMinSizes(minSizesParameters);
            IsRound = isRound;
            Offsets = new ObservableCollection<OffsetViewModel>() { new OffsetViewModel(new TypeNamesProvider(isRound)) };
            var categoriesInfoViewModel = GetCategoriesInfoViewModel(revitRepository, Name);
            SetViewModel = new SetViewModel(revitRepository, categoriesInfoViewModel);

            // инициализация значений по умолчанию
            IsSelected = true; // категория выбрана для создания заданий на отверстия
            SelectedRounding = Roundings.Last(); // округление 50 мм
            var firstOffset = Offsets.First();
            firstOffset.To = MepCategory.DefaultMaxSizeMm; // максимальный габарит сечения элемента для зазора в мм
            firstOffset.SelectedOpeningType = firstOffset.OpeningTypeNames.Last(); // тип задания на  отверстие по умолчанию прямоугольный
            firstOffset.Offset = MepCategory.DefaultOffsetMm; // зазор 50 мм для прямоугольных заданий на отверстия
            foreach(var structureCategory in StructureCategories) {
                structureCategory.IsSelected = true; // все категории конструкций выбраны для создания заданий на отверстия
            }

            AddOffsetCommand = new RelayCommand(AddOffset);
            RemoveOffsetCommand = new RelayCommand(RemoveOffset, CanRemoveOffset);
        }


        public bool IsRound { get; set; }

        public bool IsSelected {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public int SelectedRounding { get; set; }

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

        public List<StructureCategoryViewModel> StructureCategories {
            get => _structureCategories;
            set => RaiseAndSetIfChanged(ref _structureCategories, value);
        }

        public List<int> Roundings { get; set; } = new List<int> { 0, 10, 50 };

        public SetViewModel SetViewModel {
            get => _setViewModel;
            set => RaiseAndSetIfChanged(ref _setViewModel, value);
        }

        public ICommand AddOffsetCommand { get; }
        public ICommand RemoveOffsetCommand { get; }

        public string GetErrorText() {
            var sizeError = MinSizes.Select(item => item.GetErrorText()).FirstOrDefault(item => !string.IsNullOrEmpty(item));
            if(!string.IsNullOrEmpty(sizeError)) {
                return $"У категории \"{Name}\" {sizeError}";
            }
            var offsetError = Offsets.Select(item => item.GetErrorText()).FirstOrDefault(item => !string.IsNullOrEmpty(item));
            if(!string.IsNullOrEmpty(offsetError)) {
                return $"У категории \"{Name}\" {offsetError}";
            }
            var intersectionOffsetError = GetIntersectionOffsetError();
            if(!string.IsNullOrEmpty(intersectionOffsetError)) {
                return $"У категории \"{Name}\" {intersectionOffsetError}";
            }
            if(IsSelected && StructureCategories.All(item => !item.IsSelected)) {
                return $"Для категории \"{Name}\" выберите категории для пересечения";
            }
            if(SetViewModel.IsEmpty()) {
                return $"Все поля в критериях фильтрации категории \'{Name}\' должны быть заполнены.";
            }
            if(!string.IsNullOrEmpty(SetViewModel.GetErrorText())) {
                return SetViewModel.GetErrorText();
            }
            return null;
        }

        public MepCategory GetMepCategory() {
            return new MepCategory(Name, ImageSource, IsRound) {
                Offsets = Offsets.Select(item => item.GetOffset()).ToList(),
                MinSizes = new SizeCollection(MinSizes.Select(item => item.GetSize())),
                IsSelected = IsSelected,
                Intersections = StructureCategories.Select(item => new StructureCategory() { Name = item.Name, IsSelected = item.IsSelected }).ToList(),
                Rounding = SelectedRounding,
                Set = SetViewModel.GetSet()
            };
        }

        private void InitializeStructureCategories() {
            StructureCategories = new List<StructureCategoryViewModel>() {
                new StructureCategoryViewModel(){Name = RevitRepository.StructureCategoryNames[StructureCategoryEnum.Wall]},
                new StructureCategoryViewModel(){Name = RevitRepository.StructureCategoryNames[StructureCategoryEnum.Floor]},
            };
        }

        private void SetSelectedCategories(MepCategory mepCategory) {
            foreach(var category in StructureCategories) {
                category.IsSelected = mepCategory.Intersections.FirstOrDefault(item => item.Name.Equals(category.Name, StringComparison.CurrentCulture))?.IsSelected == true;
            }
        }

        private void AddOffset(object p) {
            Offsets.Add(new OffsetViewModel(new TypeNamesProvider(IsRound)));
        }

        private void RemoveOffset(object p) {
            Offsets.Remove(p as OffsetViewModel);
        }

        private bool CanRemoveOffset(object p) {
            return (p as OffsetViewModel) != null;
        }

        private string GetIntersectionOffsetError() {
            string error = null;
            for(int i = 0; i < Offsets.Count; i++) {
                for(int j = i + 1; j < Offsets.Count; j++) {
                    error = Offsets[i].GetIntersectText(Offsets[j]);
                    if(!string.IsNullOrEmpty(error))
                        return error;
                }
            }
            return error;
        }

        private ObservableCollection<SizeViewModel> GetMinSizes(Parameters[] minSizesParameters) {
            var collection = new ObservableCollection<SizeViewModel>();
            foreach(Parameters parameter in minSizesParameters) {
                collection.Add(new SizeViewModel() { Name = RevitRepository.ParameterNames[parameter] });
            }
            return collection;
        }

        private CategoriesInfoViewModel GetCategoriesInfoViewModel(RevitRepository revitRepository, string mepCategoryName) {
            Category[] revitCategories = revitRepository.GetCategories(revitRepository.GetMepCategoryEnum(mepCategoryName));
            return new CategoriesInfoViewModel(revitRepository, revitCategories);
        }
    }
}
