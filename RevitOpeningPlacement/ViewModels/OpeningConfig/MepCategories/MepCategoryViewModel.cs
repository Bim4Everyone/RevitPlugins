using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.Interfaces;
using RevitOpeningPlacement.ViewModels.OpeningConfig.OffsetViewModels;
using RevitOpeningPlacement.ViewModels.OpeningConfig.SizeViewModels;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig.MepCategories {
    internal class MepCategoryViewModel : BaseViewModel, IMepCategoryViewModel {
        private string _name;
        private ObservableCollection<ISizeViewModel> _minSizes;
        private ObservableCollection<IOffsetViewModel> _offsets;

        public MepCategoryViewModel(MepCategory mepCategory = null) {
            if(mepCategory != null) {
                Name = mepCategory.Name;
                ImageSource = mepCategory.ImageSource;
                MinSizes = new ObservableCollection<ISizeViewModel>(mepCategory.MinSizes.Select(item => new SizeViewModel(item)));
                Offsets = new ObservableCollection<IOffsetViewModel>(mepCategory.Offsets.Select(item => new OffsetViewModel(item)));
                Name = mepCategory.Name;
            }

            AddOffsetCommand = new RelayCommand(AddOffset);
            RemoveOffsetCommand = new RelayCommand(RemoveOffset, CanRemoveOffset);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        public string ImageSource { get; set; }

        public ObservableCollection<ISizeViewModel> MinSizes {
            get => _minSizes;
            set => this.RaiseAndSetIfChanged(ref _minSizes, value);
        }

        public ObservableCollection<IOffsetViewModel> Offsets {
            get => _offsets;
            set => this.RaiseAndSetIfChanged(ref _offsets, value);
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
            return null;
        }

        public MepCategory GetMepCategory() {
            return new MepCategory() {
                Name = Name,
                ImageSource = ImageSource,
                Offsets = Offsets.Select(item => item.GetOffset()).ToList(),
                MinSizes = MinSizes.Select(item => item.GetSize()).ToList()
            };
        }

        private void AddOffset(object p) {
            Offsets.Add(new OffsetViewModel());
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
