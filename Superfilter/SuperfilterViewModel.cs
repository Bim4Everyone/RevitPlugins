using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Superfilter {
    internal class SuperfilterViewModel : BaseViewModel {
        private RevitViewModel _revitViewModel;

        public SuperfilterViewModel() { }
        public SuperfilterViewModel(Application application, Document document) {
            RevitViewModels = new ObservableCollection<RevitViewModel> {
                new RevitViewModel(application, document, repository => repository.GetElements()) { Name = "Выборка по текущему виду" },
                new RevitViewModel(application, document, repository => repository.GetAllElements()) { Name = "Выборка по всем элементам" },
                new RevitViewModel(application, document, repository => repository.GetSelectedElements()) { Name = "Выборка по выделенным элементам" }
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
