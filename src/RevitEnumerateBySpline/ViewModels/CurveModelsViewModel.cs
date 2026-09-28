using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Services;

namespace RevitEnumerateBySpline.ViewModels;

internal class CurveModelsViewModel : BaseViewModel {
    private readonly SelectionService _selectionService;
    private readonly RevitRepository _revitRepository;
    private readonly WindowService _windowService;
    private IList<Curve> _selectedCurves = [];
    private ObservableCollection<CurveModelViewModel> _curveModelViewModels = [];

    public CurveModelsViewModel(SelectionService selectionService, RevitRepository revitRepository, WindowService windowService) {
        _selectionService = selectionService;
        _revitRepository = revitRepository;
        _windowService = windowService;
        
        SelectCurvesCommand = RelayCommand.Create(SelectCurves);
        
        DeleteAllCurvesCommand = RelayCommand.Create(DeleteAllCurves);
        MoveUpCurveCommand = RelayCommand.Create(MoveUpCurve);
        MoveDownCurveCommand = RelayCommand.Create(MoveDownCurve);
        DeleteCurveCommand = RelayCommand.Create(DeleteCurve);
    }

    public ICommand SelectCurvesCommand { get; }
    
    public ICommand DeleteAllCurvesCommand { get; }
    public ICommand MoveUpCurveCommand { get; }
    public ICommand MoveDownCurveCommand { get; }
    public ICommand DeleteCurveCommand { get; }
    
    public IList<Curve>SelectedCurves {
        get => _selectedCurves;
        set => RaiseAndSetIfChanged(ref _selectedCurves, value);
    }
    
    public ObservableCollection<CurveModelViewModel> CurveModelViewModels {
        get => _curveModelViewModels;
        set => RaiseAndSetIfChanged(ref _curveModelViewModels, value);
    }

    private void SelectCurves() {
        _windowService.HideMainWindow();
        var curves = _selectionService.PickCurve("Выберите линии, и нажмите Готово", _revitRepository.ActiveUiDocument);
        if(curves.Count == 0) {
            _windowService.ShowMainWindow();
            return;
        }
        foreach(var curve in curves) {
            if(curve is null || CurveModelViewModels.Any(x => x.CurveModel?.ElementId == curve.ElementId)) {
                continue;   
            }
            CurveModelViewModels.Add(new CurveModelViewModel {
                Name = $"{curve.CurveElement?.LineStyle.Name}, ID: {curve.ElementId}",
                CurveModel = curve
            });
        }
        _windowService.ShowMainWindow();
    }
    
    private void DeleteCurve() {
        throw new NotImplementedException();
    }

    private void MoveDownCurve() {
        throw new NotImplementedException();
    }

    private void MoveUpCurve() {
        throw new NotImplementedException();
    }

    private void DeleteAllCurves() {
        throw new NotImplementedException();
    }
    
    
}
