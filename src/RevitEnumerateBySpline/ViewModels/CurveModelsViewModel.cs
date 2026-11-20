using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Services;

namespace RevitEnumerateBySpline.ViewModels;

/// <summary>Управляет списком кривых в порядке нумерации.</summary>
internal class CurveModelsViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly SelectionService _selectionService;
    private readonly RevitRepository _revitRepository;
    private readonly IWindowService _windowService;
    private readonly CurvePreviewService _previewService;
    private readonly SystemPluginConfig _systemPluginConfig;
    private int _nextColor;
    private CurveModelViewModel? _selectedCurveModelViewModel;
    private ObservableCollection<CurveModelViewModel> _curveModelViewModels = [];

    /// <summary>Создаёт модель списка выбранных кривых.</summary>
    public CurveModelsViewModel(
        IWindowService windowService,
        ILocalizationService localizationService,
        SelectionService selectionService,
        RevitRepository revitRepository,
        CurvePreviewService previewService,
        SystemPluginConfig systemPluginConfig) {
        _systemPluginConfig = systemPluginConfig;
        _previewService = previewService;
        _windowService = windowService;
        _localizationService = localizationService;
        _selectionService = selectionService;
        _revitRepository = revitRepository;

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
    
    public CurveModelViewModel? SelectedCurveModelViewModel {
        get => _selectedCurveModelViewModel;
        set => RaiseAndSetIfChanged(ref _selectedCurveModelViewModel, value);
    }
    
    public ObservableCollection<CurveModelViewModel> CurveModelViewModels {
        get => _curveModelViewModels;
        set => RaiseAndSetIfChanged(ref _curveModelViewModels, value);
    }

    // Запрашивает выбор линий.
    private void SelectCurves() {
        _windowService.RequestSelection(PickCurves);
    }

    /// <summary>Добавляет выбранные линии к существующему списку, сохраняя его при отмене.</summary>
    private void PickCurves() {
        string prompt = _localizationService.GetLocalizedString("CurveModelsViewModel.SelectCurves.Prompt");
        try {
            var curves = _selectionService.PickCurve(prompt, _revitRepository.ActiveUiDocument, pending => {
                // До «Готово» изменяется только графика, а не список GUI и счётчик цветов.
                int colorIndex = _nextColor;
                var preview = CurveModelViewModels.ToList();
                foreach(var curve in pending) {
                    if(preview.Any(x => x.CurveModel?.ElementId == curve.ElementId)) {
                        continue;
                    }
                    preview.Add(CreateCurveViewModel(curve, colorIndex++));
                }
                _previewService.Update(preview);
            });
            if(curves.Count > 0) {
                AddCurves(curves);
            }
        } finally {
            // При отмене или ошибке остаются только ранее подтверждённые метки.
            _previewService.Update(CurveModelViewModels);
        }
    }

    /// <summary>Добавляет выбранные линии без повторов.</summary>
    /// <param name="curves">Линии в порядке, полученном от Revit.</param>
    public void AddCurves(IEnumerable<CurveModel> curves) {
        // Сохраняем порядок выбора: первая выбранная линия добавляется первой.
        foreach(var curve in curves) {
            if(curve is null || CurveModelViewModels.Any(x => x.CurveModel?.ElementId == curve.ElementId)) {
                continue;   
            }
            CurveModelViewModels.Add(CreateCurveViewModel(curve, _nextColor++));
        }
        _previewService.Update(CurveModelViewModels);
    }
    
    /// <summary>Создаёт представление линии и назначает цвет из циклической палитры.</summary>
    /// <param name="curve">Выбранная линия Revit.</param>
    /// <param name="colorIndex">Индекс цвета в последовательности назначения, начиная с нуля.</param>
    /// <returns>Представление линии для списка и предпросмотра.</returns>
    private CurveModelViewModel CreateCurveViewModel(CurveModel curve, int colorIndex) {
        var palette = _systemPluginConfig.CurveMarkerPalette;
        return new CurveModelViewModel {
            Name = $"{curve.CurveElement?.LineStyle.Name}, ID: {curve.ElementId}",
            MarkerColor = palette[colorIndex % palette.Count],
            CurveModel = curve
        };
    }

    // Удаляет выбранную линию.
    private void DeleteCurve() {
        if(SelectedCurveModelViewModel is null) {
            return;
        }
        int index = CurveModelViewModels.IndexOf(SelectedCurveModelViewModel);
        if(index == -1) {
            return;
        }
        CurveModelViewModels.RemoveAt(index);
        RefreshPreview();
        SelectedCurveModelViewModel = CurveModelViewModels.Count == 0 
            ? null 
            : CurveModelViewModels[Math.Min(index, CurveModelViewModels.Count - 1)];
    }

    // Сдвигает линию вниз.
    private void MoveDownCurve() {
        if(SelectedCurveModelViewModel is null) {
            return;
        }
        int index = CurveModelViewModels.IndexOf(SelectedCurveModelViewModel);
        if(index == -1 || index >= CurveModelViewModels.Count - 1) {
            return;
        }
        CurveModelViewModels.Move(index, index + 1);
        RefreshPreview();
    }

    // Сдвигает линию вверх.
    private void MoveUpCurve() {
        if(SelectedCurveModelViewModel is null) {
            return;
        }
        int index = CurveModelViewModels.IndexOf(SelectedCurveModelViewModel);
        if(index <= 0) {
            return;
        }
        CurveModelViewModels.Move(index, index - 1);
        RefreshPreview();
    }

    // Удаляет все линии.
    private void DeleteAllCurves() {
        CurveModelViewModels.Clear();
        SelectedCurveModelViewModel = null;
        RefreshPreview();
    }

    /// <summary>Запрашивает обновление предпросмотра после выхода из модального диалога.</summary>
    private void RefreshPreview() {
        _windowService.RequestSelection(() => _previewService.Update(CurveModelViewModels));
    }
}
