using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using Superfilter.ViewModels.Revit;

namespace Superfilter.ViewModels {
    internal class SuperfilterViewModel : BaseViewModel {
        private RevitViewModel _revitViewModel;

        public SuperfilterViewModel() { }
        public SuperfilterViewModel(Application application, Document document) {
            RevitViewModels = new ObservableCollection<RevitViewModel> {
                new ViewRevitViewModel(application, document) { Name = "Выборка по текущему виду" },
                new ElementsRevitViewModel(application, document) { Name = "Выборка по всем элементам" },
                new SelectedRevitViewModel(application, document) { Name = "Выборка по выделенным элементам" }
            };

            RevitViewModel = RevitViewModels[0];
        }

        public RevitViewModel RevitViewModel {
            get => _revitViewModel;
            set {
                _revitViewModel = value;
                OnPropertyChanged(nameof(RevitViewModel));
            }
        }

        public ObservableCollection<RevitViewModel> RevitViewModels { get; }
    }
}
