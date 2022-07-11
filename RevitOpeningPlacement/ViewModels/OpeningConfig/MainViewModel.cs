using System.Collections.ObjectModel;

using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.ViewModels.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _errorText;
        private ObservableCollection<IMepCategoryViewModel> _mepCategories;

        public MainViewModel(UIApplication uiApplication) {
            _revitRepository = new RevitRepository(uiApplication);
        }

        public ObservableCollection<IMepCategoryViewModel> MepCategories {
            get => _mepCategories;
            set => this.RaiseAndSetIfChanged(ref _mepCategories, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public void InitializeCategories() {
            MepCategories = new ObservableCollection<IMepCategoryViewModel>();
        }
    }
}