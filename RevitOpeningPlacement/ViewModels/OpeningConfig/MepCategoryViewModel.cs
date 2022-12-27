using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public MepCategoryViewModel(MepCategory mepCategory = null) {
            InitializeStructureCategories();

            if(mepCategory != null) {
                Name = mepCategory.Name;
                ImageSource = mepCategory.ImageSource;
                MinSizes = new ObservableCollection<SizeViewModel>(mepCategory.MinSizes.Select(item => new SizeViewModel(item)));
                IsRound = mepCategory.IsRound;
                IsSelected = mepCategory.IsSelected;
                Offsets = new ObservableCollection<OffsetViewModel>(mepCategory.Offsets.Select(item => new OffsetViewModel(item, new TypeNamesProvider(mepCategory.IsRound))));
                SetSelectedCategories(mepCategory);
            }

            AddOffsetCommand = new RelayCommand(AddOffset);
            RemoveOffsetCommand = new RelayCommand(RemoveOffset, CanRemoveOffset);
        }

        public bool IsRound { get; set; }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string ImageSource { get; set; }

        public ObservableCollection<SizeViewModel> MinSizes {
            get => _minSizes;
            set => this.RaiseAndSetIfChanged(ref _minSizes, value);
        }

        public ObservableCollection<OffsetViewModel> Offsets {
            get => _offsets;
            set => this.RaiseAndSetIfChanged(ref _offsets, value);
        }

        public List<StructureCategoryViewModel> StructureCategories {
            get => _structureCategories;
            set => this.RaiseAndSetIfChanged(ref _structureCategories, value);
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
            return null;
        }

        public MepCategory GetMepCategory() {
            return new MepCategory() {
                Name = Name,
                ImageSource = ImageSource,
                Offsets = Offsets.Select(item => item.GetOffset()).ToList(),
                MinSizes = new SizeCollection(MinSizes.Select(item => item.GetSize())),
                IsRound = IsRound,
                IsSelected = IsSelected,
                Intersections = StructureCategories.Select(item => new StructureCategory() { Name = item.Name, IsSelected = item.IsSelected }).ToList()
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
    }
}
