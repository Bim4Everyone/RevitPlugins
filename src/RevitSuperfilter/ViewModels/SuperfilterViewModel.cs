using System.Collections.ObjectModel;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSuperfilter.ViewModels.Revit;

namespace RevitSuperfilter.ViewModels;

internal class SuperfilterViewModel : BaseViewModel {
    private RevitViewModel _revitViewModel;

    public SuperfilterViewModel() {
    }

    public SuperfilterViewModel(Application application, Document document, bool hasSelectedElement) {
        RevitViewModels = new ObservableCollection<RevitViewModel> {
            new ViewRevitViewModel(application, document) { DisplayData = "Выборка по текущему виду" },
            new ElementsRevitViewModel(application, document) { DisplayData = "Выборка по всем элементам" },
            new SelectedRevitViewModel(application, document) { DisplayData = "Выборка по выделенным элементам" }
        };

        RevitViewModel = hasSelectedElement
            ? RevitViewModels[2]
            : RevitViewModels[0];
    }

    public RevitViewModel RevitViewModel {
        get => _revitViewModel;
        set => RaiseAndSetIfChanged(ref _revitViewModel, value);
    }

    public ObservableCollection<RevitViewModel> RevitViewModels { get; }
}
