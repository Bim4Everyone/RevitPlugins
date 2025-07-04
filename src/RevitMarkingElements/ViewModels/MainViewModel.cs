using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkingElements.Models;
using RevitMarkingElements.Views;

namespace RevitMarkingElements.ViewModels;
internal class MainViewModel : BaseViewModel {
    private const BuiltInParameter _markParam = BuiltInParameter.ALL_MODEL_MARK;

    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private bool _includeUnselected;
    private bool _renumberAll;
    private bool _isArrayNumberingSelected;
    private bool _isLineNumberingSelected;

    private int _startNumber;

    private string _errorText;
    private string _lineNumberingContent;

    private List<Category> _categories;
    private Category _selectedCategory;

    private List<CurveElement> Lines { get; set; }
    private List<Element> SelectedElements { get; set; }

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        IsLineNumberingSelected = true;
        StartNumber = 1;

        //После закрытия окна выбора линий снять выделения с линии нельзя, только выбирать линии заново, пока оставили так
        SelectLinesCommand = RelayCommand.Create<MainWindow>(SelectLines);
        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public ICommand SelectLinesCommand { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string LineNumberingContent {
        get => _lineNumberingContent;
        set => RaiseAndSetIfChanged(ref _lineNumberingContent, value);
    }

    public int StartNumber {
        get => _startNumber;
        set => RaiseAndSetIfChanged(ref _startNumber, value);
    }

    public List<Category> Categories {
        get => _categories;
        set => RaiseAndSetIfChanged(ref _categories, value);
    }

    public Category SelectedCategory {
        get => _selectedCategory; set => RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public bool IsLineNumberingSelected {
        get => _isLineNumberingSelected; set => RaiseAndSetIfChanged(ref _isLineNumberingSelected, value);
    }

    public bool IsArrayNumberingSelected {
        get => _isArrayNumberingSelected; set => RaiseAndSetIfChanged(ref _isArrayNumberingSelected, value);
    }

    public bool IncludeUnselected {
        get => _includeUnselected;
        set => RaiseAndSetIfChanged(ref _includeUnselected, value);
    }

    public bool RenumberAll {
        get => _renumberAll;
        set => RaiseAndSetIfChanged(ref _renumberAll, value);
    }

    private void LoadCategories() {
        var allCategories = _revitRepository
            .GetSelectedElements()
            .Select(item => item.Category)
            .Where(category => category != null)
            .GroupBy(category => category.Id)
            .Select(group => group.First())
            .ToList();

        Categories = allCategories;

        SelectedCategory = Categories.FirstOrDefault();
    }

    private void SelectLines(MainWindow mainWindow) {
        mainWindow.Hide();
        try {
            Lines = _revitRepository.SelectLinesOnView();

            string lineNumberingLabel = _localizationService.GetLocalizedString("MainWindow.LineNumberingLabel");
            int count = Lines?.Count ?? 0;
            LineNumberingContent = $"{lineNumberingLabel} ({count}) ";

        } finally {
            _ = mainWindow.ShowDialog();
        }
    }

    private void NumberMarkingElements() {

        var markingElements = SelectedElements.Where(item => item.Category.Id == SelectedCategory.Id)
            .ToList();

        var processedElements = RenumberAll
            ? []
            : markingElements
                .Where(e => e.IsExistsParamValue(_markParam))
                .ToList();

        var unprocessedElements = RenumberAll
            ? markingElements
            : markingElements.Except(processedElements).ToList();

        int counter = RenumberAll ? StartNumber : GetNextAvailableMarkNumber(markingElements);

        var lines = IsLineNumberingSelected ? Lines : null;
        var sortedUnprocessedElements = IsLineNumberingSelected
            ? SortElementsByLines(unprocessedElements, lines)
            : SortElementsByCoordinates(unprocessedElements);

        string transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
        using var transaction = _revitRepository.Document.StartTransaction(transactionName);

        AssignMarks(sortedUnprocessedElements, processedElements, ref counter);

        _ = transaction.Commit();
    }

    private int GetNextAvailableMarkNumber(List<Element> markingElements) {
        var usedMarks = markingElements
            .Select(markingElement => markingElement.GetParamValue<string>(_markParam))
            .Where(mark => !string.IsNullOrEmpty(mark) && int.TryParse(mark, out _))
            .Select(mark => int.Parse(mark))
            .OrderBy(mark => mark)
            .ToList();

        int nextAvailableNumber = 1;

        foreach(int mark in usedMarks) {
            if(mark == nextAvailableNumber) {
                nextAvailableNumber++;
            } else if(mark > nextAvailableNumber) {
                break;
            }
        }

        return nextAvailableNumber;
    }

    private List<Element> SortElementsByLines(List<Element> elements, List<CurveElement> lines) {
        var sortedElements = new List<Element>();

        foreach(var line in lines) {
            var lineCurve = line.GeometryCurve;
            if(lineCurve == null) {
                continue;
            }

            var elementsOnLine = _revitRepository
                .GetElementsIntersectingLine(elements, line.GeometryCurve)
                .OrderBy(element => {
                    var point = _revitRepository.GetElementCoordinates(element);
                    var projection = lineCurve.Project(point);
                    return projection?.Parameter ?? double.MaxValue;
                })
                .ToList();

            sortedElements.AddRange(elementsOnLine);
        }

        return sortedElements;
    }

    private List<Element> SortElementsByCoordinates(List<Element> elements) {
        return elements
            .OrderByDescending(element => {
                var point = _revitRepository.GetElementCoordinates(element);
                return (point.Y, point.X);
            })
            .ToList();
    }

    private void AssignMarks(List<Element> elements, List<Element> processedElements, ref int counter) {
        var existingMarks = new HashSet<string>(
            processedElements.Where(e => e.IsExistsParamValue(_markParam))
                             .Select(e => e.GetParamValueOrDefault<string>(_markParam)));

        foreach(var element in elements) {
            var markParam = element.GetParam(_markParam);
            if(markParam != null && !markParam.IsReadOnly) {
                while(existingMarks.Contains(counter.ToString())) {
                    counter++;
                }

                _ = markParam.Set($"{counter}");
                _ = existingMarks.Add(counter.ToString());
                processedElements.Add(element);
                counter++;
            }
        }
    }

    private void LoadView() {
        LoadCategories();
        LoadConfig();
        SelectedElements = _revitRepository.GetSelectedElements();
        string lineNumberingLabel = _localizationService.GetLocalizedString("MainWindow.LineNumberingLabel");
        LineNumberingContent = $"{lineNumberingLabel} (0) ";
    }

    private void AcceptView() {
        NumberMarkingElements();
        SaveConfig();
    }

    private bool CanAcceptView() {
        if(!IsLineNumberingSelected && !IsArrayNumberingSelected) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.SelectNumberingType");
            return false;
        }

        if(!IsArrayNumberingSelected && !_includeUnselected && (Lines == null || Lines.Count == 0)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoElements");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SelectedCategory = setting != null
            ? Categories.FirstOrDefault(item => item.Id == setting.SelectedCategory)
            : Categories.FirstOrDefault();
    }


    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SelectedCategory = SelectedCategory.Id;
        _pluginConfig.SaveProjectConfig();
    }
}
