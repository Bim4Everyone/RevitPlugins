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

        public MepCategoryViewModel(MepCategory mepCategory) {
            Name = mepCategory.Name;
            ImageSource = mepCategory.ImageSource;
            MinSizes = new ObservableCollection<ISizeViewModel>(mepCategory.MinSizes.Select(item => new SizeViewModel(item)));
            Offsets = new ObservableCollection<IOffsetViewModel>(mepCategory.Offsets.Select(item => new OffsetViewModel(item)));
            Name = mepCategory.Name;
        }

        public MepCategoryViewModel() {
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
    }
}
