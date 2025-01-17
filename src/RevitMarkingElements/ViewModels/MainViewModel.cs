// Ignore Spelling: bbox

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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
        private string _selectedCategoryName;
        private List<Category> _categories;
        private ElementId _selectedCategoryId;
        private bool _includeUnselected;
        private bool _renumberAll;
        private bool _isArrayNumberingSelected;
        private bool _isLineNumberingSelected;
        private List<CurveElement> Lines { get; set; }

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

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

        private void SelectLines(MainWindow mainWindow) {
            mainWindow.Hide();
            try {
                var mainInstruction = _localizationService.GetLocalizedString("MainWindow.MainInstruction");
                var mainContent = _localizationService.GetLocalizedString("MainWindow.MainContent");
                var selectLinesText = _localizationService.GetLocalizedString("MainWindow.SelectLines");
                var resultMessage = _localizationService.GetLocalizedString("MainWindow.ResultMessage");
                var resultTitle = _localizationService.GetLocalizedString("MainWindow.ResultTitle");

                var taskDialog = new TaskDialog("Выбор линий") {
                    MainInstruction = mainInstruction,
                    MainContent = mainContent,
                    CommonButtons = TaskDialogCommonButtons.Ok
                };

                taskDialog.Show();
                var linesType = _localizationService.GetLocalizedString("MainWindow.LinesType");
                Lines = _revitRepository.SelectLinesOnView(linesType);
                var message = $"{resultMessage}({Lines?.Count ?? 0})";

                TaskDialog.Show(resultTitle, message);
            } finally {
                mainWindow.ShowDialog();
            }
        }

        private void LoadCategories() {
            var allCategories = _revitRepository.GetCategoriesWithMarkParam(_markParam);

            Categories = allCategories;
            SelectedCategoryName = allCategories.FirstOrDefault(x => x.Id.AsBuiltInCategory() == _structuralColumns)?.Name;
        }

        private void NumberMarkingElements() {
            var markingElements = _revitRepository.GetElements(SelectedCategoryId);
            var counter = RenumberAll ? 1 : GetNextAvailableMarkNumber(markingElements);

            var context = new MarkingContext {
                Counter = counter,
                MarkingElements = markingElements,
                Lines = Lines
            };

            var transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
            using(Transaction transaction = _revitRepository.CreateTransaction(transactionName)) {
                transaction.Start();

                if(IsArrayNumberingSelected) {
                    ProcessUnselectedElements(context);
                } else {
                    ProcessElementsByLines(context);

                    if(IncludeUnselected) {
                        ProcessUnselectedElements(context);
                    }
                }

                transaction.Commit();
            }

            ErrorText = null;
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

        private void ProcessElementsByLines(MarkingContext context) {
            foreach(var lineElement in context.Lines) {
                var line = lineElement.GeometryCurve;
                if(line == null) {
                    continue;
                }

                var markingElementsOnLine = _revitRepository
                    .GetElementsIntersectingLine(context.MarkingElements, lineElement)
                    .OrderBy(markingElement => {
                        var markingElementPoint = _revitRepository.GetElementCoordinates(markingElement);
                        return markingElementPoint.DistanceTo(line.GetEndPoint(0));
                    })
                    .ToList();

                AssignMarksToElements(markingElementsOnLine, context);
            }
        }

        private void ProcessUnselectedElements(MarkingContext context) {

            var remainingElements = context.MarkingElements.Except(context.ProcessedElements)
                .OrderByDescending(markingElement => {
                    var point = _revitRepository.GetElementCoordinates(markingElement);
                    return (point.Y, point.X);
                })
                .ToList();

            AssignMarksToElements(remainingElements, context);
        }

        private void AssignMarksToElements(List<Element> elements, MarkingContext context) {
            var existingMarks = RenumberAll
                ? new List<string>()
                : new List<string>(
                    elements
                        .Select(e => e.GetParam(_markParam)?.AsString())
                        .Where(mark => !string.IsNullOrEmpty(mark))
                );

            foreach(var markingElement in elements) {
                var markParam = markingElement.GetParam(_markParam);
                if(markParam != null && !markParam.IsReadOnly) {
                    var currentMarkValue = markParam.AsString();
                    if(!RenumberAll && !string.IsNullOrEmpty(currentMarkValue)) {
                        context.ProcessedElements.Add(markingElement);
                        continue;
                    }

                    while(existingMarks.Contains(context.Counter.ToString())) {
                        context.Counter++;
                    }

                    markParam.Set($"{context.Counter}");
                    existingMarks.Add(context.Counter.ToString());
                    context.Counter++;
                    context.ProcessedElements.Add(markingElement);
                }
            }
        }

        private void LoadView() {
            LoadConfig();
            LoadCategories();
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
