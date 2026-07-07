using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.ViewModels;

internal class ViewPlanSelectionViewModel : BaseViewModel {
    
    private readonly RevitRepository _revitRepository;
    private readonly AreaBoundarySettings _areaBoundarySettings;
    
    private bool _hasError;
    private ObservableCollection<RevitElementGroupViewModel> _viewPlanGroupViewModels;
    
    public ViewPlanSelectionViewModel(RevitRepository revitRepository, AreaBoundarySettings areaBoundarySettings) { 
        _revitRepository = revitRepository;
        _areaBoundarySettings = areaBoundarySettings;
        
        LoadView();
    }
    
    public bool HasError {
        get => _hasError;
        set => RaiseAndSetIfChanged(ref _hasError, value);
    }
    
    public ObservableCollection<RevitElementGroupViewModel> ViewPlanGroupViewModels {
        get => _viewPlanGroupViewModels;
        set => RaiseAndSetIfChanged(ref _viewPlanGroupViewModels, value);
    }

    private IEnumerable<RevitElementGroupViewModel> GetViewPlanGroupViewModels() {
        var savedViewIds = _areaBoundarySettings.Views
            .Select(view => view.Element.Id)
            .ToHashSet();

        return _revitRepository.GetViewPlans()
            .GroupBy(viewPlan => viewPlan.GroupName)
            .Select(group => {
                var viewModels = group.Select(view => new RevitElementViewModel {
                    Name = view.Name,
                    IsChecked = savedViewIds.Contains(view.Element.Id)
                });
        
                return new RevitElementGroupViewModel {
                    Name = group.Key,
                    RevitElementViewModels = new ObservableCollection<RevitElementViewModel>(viewModels)
                };
            });
        
    }
    
    private void LoadView() {
        ViewPlanGroupViewModels = new ObservableCollection<RevitElementGroupViewModel>(GetViewPlanGroupViewModels());
    }
}
