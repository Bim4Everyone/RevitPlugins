// Ignore Spelling: bbox

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

namespace RevitMarkingElements.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private const BuiltInParameter _markParam = BuiltInParameter.ALL_MODEL_MARK;
        private const BuiltInCategory _structuralColumns = BuiltInCategory.OST_StructuralColumns;

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _lineNumberingContent;
        private string _selectedCategoryName;
        private List<Category> _categories;
        private ElementId _selectedCategoryId;
        private bool _includeUnselected;
        private bool _renumberAll;
        private bool _isArrayNumberingSelected;
        private bool _isLineNumberingSelected;
        private int _startNumber;
        private List<CurveElement> Lines { get; set; }
        private List<ElementId> SelectedElementsIds { get; set; }

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

            IsLineNumberingSelected = true;
            StartNumber = 1;
            SelectedElementsIds = _revitRepository.GetSelectedElementsIds();

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
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string LineNumberingContent {
            get => _lineNumberingContent;
            set => this.RaiseAndSetIfChanged(ref _lineNumberingContent, value);
        }

        public int StartNumber {
            get => _startNumber;
            set => this.RaiseAndSetIfChanged(ref _startNumber, value);
        }

        public List<Category> Categories {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        public string SelectedCategoryName {
            get => _selectedCategoryName;
            set {
                this.RaiseAndSetIfChanged(ref _selectedCategoryName, value);

                SelectedCategoryId = _categories
                    .FirstOrDefault(category => category.Name == value)?.Id;
            }
        }

        public bool IsLineNumberingSelected {
            get => _isLineNumberingSelected;
            set {
                this.RaiseAndSetIfChanged(ref _isLineNumberingSelected, value);
            }
        }

        public bool IsArrayNumberingSelected {
            get => _isArrayNumberingSelected;
            set {
                this.RaiseAndSetIfChanged(ref _isArrayNumberingSelected, value);
            }
        }

        public bool IncludeUnselected {
            get => _includeUnselected;
            set => this.RaiseAndSetIfChanged(ref _includeUnselected, value);
        }

        public ElementId SelectedCategoryId {
            get => _selectedCategoryId;
            set => this.RaiseAndSetIfChanged(ref _selectedCategoryId, value);
        }

        public bool RenumberAll {
            get => _renumberAll;
            set => this.RaiseAndSetIfChanged(ref _renumberAll, value);
        }

        private void LoadCategories() {
            var allCategories = _revitRepository.GetCategoriesWithMarkParam(_markParam);

            Categories = allCategories;
            SelectedCategoryName = allCategories.FirstOrDefault(x => x.Id.AsBuiltInCategory() == _structuralColumns)?.Name;
        }

        private void SelectLines(MainWindow mainWindow) {
            mainWindow.Hide();
            try {
                Lines = _revitRepository.SelectLinesOnView();

                var lineNumberingLabel = _localizationService.GetLocalizedString("MainWindow.LineNumberingLabel");
                var count = Lines?.Count ?? 0;
                LineNumberingContent = $"{lineNumberingLabel} ({count}) ";

            } finally {
                mainWindow.ShowDialog();
            }
        }

        private void NumberMarkingElements() {
            var markingElements = _revitRepository.GetElements(SelectedCategoryId)
                .Where(element => SelectedElementsIds.Contains(element.Id)).ToList();


            var processedElements = RenumberAll
                ? new List<Element>()
                : markingElements
                    .Where(e => e.IsExistsParamValue(_markParam))
                    .ToList();

            var unprocessedElements = RenumberAll
                ? markingElements
                : markingElements.Except(processedElements).ToList();

            var counter = RenumberAll ? StartNumber : GetNextAvailableMarkNumber(markingElements);

            var lines = IsLineNumberingSelected ? Lines : null;
            var sortedUnprocessedElements = IsLineNumberingSelected
                ? SortElementsByLines(unprocessedElements, lines)
                : SortElementsByCoordinates(unprocessedElements);

            var transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
            using(Transaction transaction = _revitRepository.CreateTransaction(transactionName)) {
                transaction.Start();

                AssignMarks(sortedUnprocessedElements, processedElements, ref counter);

                transaction.Commit();
            }
        }

        private int GetNextAvailableMarkNumber(List<Element> markingElements) {
            var usedMarks = markingElements
                .Select(markingElement => markingElement.GetParamValue<string>(_markParam))
                .Where(mark => !string.IsNullOrEmpty(mark) && int.TryParse(mark, out _))
                .Select(mark => int.Parse(mark))
                .OrderBy(mark => mark)
                .ToList();

            int nextAvailableNumber = 1;
            foreach(var mark in usedMarks) {
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
                if(lineCurve == null)
                    continue;

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

                    markParam.Set($"{counter}");
                    existingMarks.Add(counter.ToString());
                    processedElements.Add(element);
                    counter++;
                }
            }
        }

        private void LoadView() {
            LoadConfig();
            LoadCategories();

            var lineNumberingLabel = _localizationService.GetLocalizedString("MainWindow.LineNumberingLabel");
            LineNumberingContent = $"{lineNumberingLabel} (0) ";
        }

        private void AcceptView() {
            NumberMarkingElements();
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(SelectedElementsIds.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoSelectedElements");
                return false;
            }

            if(!IsLineNumberingSelected && !IsArrayNumberingSelected) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.SelectNumberingType");
                return false;
            }

            if(!IsArrayNumberingSelected && !_includeUnselected && (Lines == null || !Lines.Any())) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoElements");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            SelectedCategoryId = setting?.SelectedCategoryId ?? new ElementId(_structuralColumns);
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SelectedCategoryId = SelectedCategoryId;
            _pluginConfig.SaveProjectConfig();
        }

    }
}
