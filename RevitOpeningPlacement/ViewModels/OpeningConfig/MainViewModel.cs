using System.Collections.ObjectModel;

using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.ViewModels.Interfaces;
using RevitOpeningPlacement.ViewModels.OpeningConfig.MepCategories;
using RevitOpeningPlacement.ViewModels.OpeningConfig.OffsetViewModels;
using RevitOpeningPlacement.ViewModels.OpeningConfig.SizeViewModels;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;
        private ObservableCollection<IMepCategoryViewModel> _mepCategories;

        public MainViewModel(UIApplication uiApplication) {
            _revitRepository = new RevitRepository(uiApplication);
            InitializeCategories();
        }

        public ObservableCollection<IMepCategoryViewModel> MepCategories {
            get => _mepCategories;
            set => this.RaiseAndSetIfChanged(ref _mepCategories, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private void InitializeCategories() {
            MepCategories = new ObservableCollection<IMepCategoryViewModel>() {
                GetPipeCategory(),
                GetRectangleDuct(),
                GetRoundDuct(),
                GetCableTray()
            };
        }

        private MepCategoryViewModel GetPipeCategory() => new MepCategoryViewModel {
            Name = "Трубы",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Диаметр"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            }
        };

        private MepCategoryViewModel GetRectangleDuct() => new MepCategoryViewModel {
            Name = "Воздуховоды (прямоугольное сечение)",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Ширина"},
                new SizeViewModel(){Name ="Высота"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            }
        };

        private MepCategoryViewModel GetRoundDuct() => new MepCategoryViewModel {
            Name = "Воздуховоды (круглое сечение)",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Диаметр"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            }
        };

        private MepCategoryViewModel GetCableTray() => new MepCategoryViewModel {
            Name = "Лотки",
            MinSizes = new ObservableCollection<ISizeViewModel>() {
                new SizeViewModel(){Name ="Ширина"},
                new SizeViewModel(){Name ="Высота"}
            },
            Offsets = new ObservableCollection<IOffsetViewModel>() {
                new OffsetViewModel()
            }
        };
    }
}