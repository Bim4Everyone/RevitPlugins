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

namespace RevitMarkingElements.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private const BuiltInParameter _markParam = BuiltInParameter.ALL_MODEL_MARK;
        private const BuiltInCategory _structuralColumns = BuiltInCategory.OST_StructuralColumns;

        private string _errorText;
        private string _selectedCategoryName;
        private List<string> _categoriesNames;
        private ElementId _selectedCategoryId;
        private bool _includeUnselected;
        private bool _renumberAll;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public List<string> CategoriesNames {
            get => _categoriesNames;
            set => this.RaiseAndSetIfChanged(ref _categoriesNames, value);
        }
        public string SelectedCategoryName {
            get => _selectedCategoryName;
            set {
                this.RaiseAndSetIfChanged(ref _selectedCategoryName, value);
                SelectedCategoryId = _revitRepository.GetCategoriesWithMarkParam()
                    .FirstOrDefault(category => category.Name == value)?.Id;
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

        public List<Element> MarkingElements { get; set; }
        public List<CurveElement> Lines { get; set; }

        private void LoadCategories() {
            var allCategories = _revitRepository.GetCategoriesWithMarkParam();

            SelectedCategoryName = allCategories
                .FirstOrDefault(x => x.Id.GetIdValue() == (int) _structuralColumns)?.Name;

            CategoriesNames = allCategories.Select(x => x.Name).Distinct().ToList();
        }

        private void NumberMarkingElements() {
            PrepareMarkingElements();
            var transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
            using(Transaction transaction = _revitRepository.CreateTransaction(transactionName)) {
                transaction.Start();

                int counter = StartingCounter();
                var processedElements = ProcessElementsByLines(ref counter);
                ProcessUnselectedElements(ref counter, processedElements);

                transaction.Commit();
            }
            ErrorText = null;
        }

        private void PrepareMarkingElements() {
            var category = _revitRepository.GetCategoryById(SelectedCategoryId);
            MarkingElements = _revitRepository.GetElements(category);
            Lines = _revitRepository.GetLinesAndSplines();
        }

        private int StartingCounter() {
            return RenumberAll ? 1 : GetLastMarkNumber() + 1;
        }

        private List<Element> ProcessElementsByLines(ref int counter) {
            var processedElements = new List<Element>();
            foreach(var lineElement in Lines) {
                var line = lineElement.GeometryCurve;
                if(line == null) {
                    continue;
                }

                var markingElementsOnLine = _revitRepository
                    .GetElementsIntersectingLine(MarkingElements, lineElement)
                    .OrderBy(markingElement => {
                        var markingElementPoint = _revitRepository.GetElementCoordinates(markingElement);
                        return markingElementPoint.DistanceTo(line.GetEndPoint(0));
                    })
                    .ToList();
                counter = AssignMarksToElements(markingElementsOnLine, counter, processedElements);
            }

            return processedElements;
        }

        private int AssignMarksToElements(List<Element> elements, int counter, List<Element> processedElements) {
            foreach(var markingElement in elements) {
                var markParam = markingElement.GetParam(_markParam);
                if(markParam != null && !markParam.IsReadOnly) {
                    markParam.Set($"{counter++}");
                    processedElements.Add(markingElement);
                }
            }
            return counter;
        }

        private void ProcessUnselectedElements(ref int counter, List<Element> processedElements) {
            if(!IncludeUnselected)
                return;

            var remainingElements = MarkingElements.Except(processedElements)
                .OrderByDescending(markingElement => {
                    var point = _revitRepository.GetElementCoordinates(markingElement);
                    return (point.Y, point.X);
                })
                .ToList();

            counter = AssignMarksToElements(remainingElements, counter, processedElements);
        }

        private int GetLastMarkNumber() {
            var filledMarks = MarkingElements
                .Select(markingElement => markingElement.GetParamValue<string>(_markParam))
                .Where(mark => !string.IsNullOrEmpty(mark) && int.TryParse(mark, out _))
                .Select(mark => int.Parse(mark))
                .ToList();

            return filledMarks.Any() ? filledMarks.Max() : 0;
        }

        private void LoadView() {
            LoadConfig();
            LoadCategories();
            PrepareMarkingElements();
        }

        private void AcceptView() {
            NumberMarkingElements();
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(!_includeUnselected && (Lines == null || !Lines.Any())) {
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
