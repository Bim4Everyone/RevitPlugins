using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreatingFiltersByValues.Models;

using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;
using Color = Autodesk.Revit.DB.Color;
using System.Xml.Linq;
using System.Windows.Media;

namespace RevitCreatingFiltersByValues.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _overrideByColor = true;
        private bool _overrideByPattern;
        private List<string> _selectedCategories = new List<string>();
        private ParametersHelper _selectedFilterableParameter;
        private List<PossibleValue> _selectedPossibleValues = new List<PossibleValue>();

        private ColorHelper _selectedColor;
        private PatternsHelper _selectedPattern;
        private List<string> patternNames = new List<string>() {
            "Алюминий",
            "Вертикальный",
            "Горизонтальный",
            "Диагональ вверх",
            "Диагональ вниз",
            "Диагональ крест-накрест",
            "Древесина - Окончательная отделка",
        };


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            
            DictCategoryElements = _revitRepository.GetDictCategoriesInView();
            CategoriesInPj = DictCategoryElements.Keys.OrderBy(s => s).ToList();
            SolidFillPattern = _revitRepository.SolidFillPattern;
            PatternsInPj = _revitRepository.GetPatternsByNames(patternNames);


            CreateCommand = new RelayCommand(Create, CanCreate);
            GetFilterableParametersCommand = new RelayCommand(GetFilterableParameters);
            GetPossibleValuesCommand = new RelayCommand(GetPossibleValues);
            SetPossibleValuesCommand = new RelayCommand(SetPossibleValues);

            ChangeColorCommand = new RelayCommand(ChangeColor, CanChangeColor);
            AddColorCommand = new RelayCommand(AddColor);
            DeleteColorCommand = new RelayCommand(DeleteColor, CanChangeColor);
            MoveColorUpCommand = new RelayCommand(MoveColorUp, CanChangeColor);
            MoveColorDownCommand = new RelayCommand(MoveColorDown, CanChangeColor);

            AddPatternCommand = new RelayCommand(AddPattern);
            DeletePatternCommand = new RelayCommand(DeletePattern, CanChangePattern);
            MovePatternUpCommand = new RelayCommand(MovePatternUp, CanChangePattern);
            MovePatternDownCommand = new RelayCommand(MovePatternDown, CanChangePattern);
        }



        public ICommand CreateCommand { get; }
        public ICommand GetFilterableParametersCommand { get; }
        public ICommand GetPossibleValuesCommand { get; }
        public ICommand SetPossibleValuesCommand { get; }

        public ICommand ChangeColorCommand { get; }
        public ICommand AddColorCommand { get; }
        public ICommand DeleteColorCommand { get; }
        public ICommand MoveColorUpCommand { get; }
        public ICommand MoveColorDownCommand { get; }

        public ICommand AddPatternCommand { get; }
        public ICommand DeletePatternCommand { get; }
        public ICommand MovePatternUpCommand { get; }
        public ICommand MovePatternDownCommand { get; }


        private List<Element> elementsInView { get; set; } = new List<Element>();
        public FillPatternElement SolidFillPattern { get; set; }
        public ObservableCollection<PatternsHelper> PatternsInPj { get; set; } = new ObservableCollection<PatternsHelper>();
        public List<ParameterFilterElement> AllFiltersInPj { get; set; } = new List<ParameterFilterElement>();
        public List<ParameterFilterElement> AllFiltersInView { get; set; } = new List<ParameterFilterElement>();
        public List<string> AllFilterNamesInPj { get; set; } = new List<string>();
        public List<string> AllFilterNamesInView { get; set; } = new List<string>();
        public Dictionary<string, CategoryElements> DictCategoryElements { get; set; } = new Dictionary<string, CategoryElements>();
        public List<string> CategoriesInPj { get; set; } = new List<string>();
        public ObservableCollection<ParametersHelper> FilterableParameters { get; set; } = new ObservableCollection<ParametersHelper>();
        public ObservableCollection<PossibleValue> PossibleValues { get; set; } = new ObservableCollection<PossibleValue>();
        public List<ElementId> SelectedCatIds { get; set; } = new List<ElementId>();


        public List<string> SelectedCategories {
            get => _selectedCategories;
            set => this.RaiseAndSetIfChanged(ref _selectedCategories, value);
        }

        public ParametersHelper SelectedFilterableParameter {
            get => _selectedFilterableParameter;
            set => this.RaiseAndSetIfChanged(ref _selectedFilterableParameter, value);
        }

        public List<PossibleValue> SelectedPossibleValues {
            get => _selectedPossibleValues;
            set => this.RaiseAndSetIfChanged(ref _selectedPossibleValues, value);
        }
        public bool OverrideByColor {
            get => _overrideByColor;
            set => this.RaiseAndSetIfChanged(ref _overrideByColor, value);
        }
        public bool OverrideByPattern {
            get => _overrideByPattern;
            set => this.RaiseAndSetIfChanged(ref _overrideByPattern, value);
        }

        public ObservableCollection<ColorHelper> Colors { get; set; } = new ObservableCollection<ColorHelper>() {
            new ColorHelper(255, 0, 0),
            new ColorHelper(0, 255, 0),
            new ColorHelper(0, 0, 255),
            new ColorHelper(255, 255, 0),
            new ColorHelper(0, 255, 255),
            new ColorHelper(255, 0, 255),
            new ColorHelper(255, 0, 128),
            new ColorHelper(128, 0, 255),
            new ColorHelper(255, 128, 0),
            new ColorHelper(64, 128, 128),
            new ColorHelper(192, 192, 192),
            new ColorHelper(250, 180, 135),
            new ColorHelper(250, 180, 135),
            new ColorHelper(240, 100, 10),
            new ColorHelper(240, 230, 115),
            new ColorHelper(0, 128, 0),
            new ColorHelper(255, 128, 255),
            new ColorHelper(128, 128, 128),
            new ColorHelper(255, 120, 150),
            new ColorHelper(110, 255, 180)
        };
        public ColorHelper SelectedColor {
            get => _selectedColor;
            set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
        }
        public PatternsHelper SelectedPattern {
            get => _selectedPattern;
            set => this.RaiseAndSetIfChanged(ref _selectedPattern, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }



        /// <summary>
        /// Получает параметры по которым доступна фильтрация из выбранных категорий.
        /// Отрабатывает при выборе категорий в списке категорий GUI
        /// </summary>
        /// <param name="p"></param>
        private void GetFilterableParameters(object p) {
            FilterableParameters.Clear();
            PossibleValues.Clear();
            SelectedCategories.Clear();
            SelectedCatIds.Clear();

            // Забираем список выбранных элементов через CommandParameter
            System.Collections.IList selectedCategories = p as System.Collections.IList;

            // Получаем ID категорий для последующего получения параметров фильтрации
            foreach(var item in selectedCategories) {
                string categoryName = item as string;
                if(categoryName == null) { continue; }

                SelectedCategories.Add(categoryName);
                SelectedCatIds.Add(DictCategoryElements[categoryName].CategoryIdInView);
            }


            // Получаем параметры для фильтров на основе ID категорий
            List<ElementId> elementIds = ParameterFilterUtilities.GetFilterableParametersInCommon(_revitRepository.Document, SelectedCatIds).ToList();

            foreach(ElementId id in elementIds) {
                // Переводим сначала в ParameterElement, если null, значит это BuiltInParameter
                ParameterElement paramAsParameterElement = _revitRepository.Document.GetElement(id) as ParameterElement;
                
                ParametersHelper parametersHelper = new ParametersHelper();

                // Если он null, значит это встроенный параметр
                if(paramAsParameterElement is null) {
                    BuiltInParameter parameterAsBuiltIn = (BuiltInParameter) Enum.ToObject(typeof(BuiltInParameter), id.IntegerValue);

                    parametersHelper.ParamName = LabelUtils.GetLabelFor(parameterAsBuiltIn);
                    parametersHelper.BInParameter = parameterAsBuiltIn;
                    parametersHelper.Id = id;
                    parametersHelper.IsBInParam = true;
                } else {
                    parametersHelper.ParamName = paramAsParameterElement.Name;
                    parametersHelper.ParamElement = paramAsParameterElement;
                    parametersHelper.Id = id;
                    parametersHelper.IsBInParam = false;
                }

                FilterableParameters.Add(parametersHelper);
            }

            FilterableParameters = new ObservableCollection<ParametersHelper>(FilterableParameters.OrderBy(i => i.ParamName));
            OnPropertyChanged(nameof(FilterableParameters));
        }



        /// <summary>
        /// Получает варианты заполнения выбранного параметра у элементов выбранных категорий
        /// Отрабатывает при выборе параметра фильтрации в списке параметров GUI
        /// </summary>
        /// <param name="p"></param>
        public void GetPossibleValues(object p) {

            if(SelectedFilterableParameter is null) { return; }
            PossibleValues.Clear();

            List<Element> elementsForWork = new List<Element>();

            // Получаем элементы, которые выбрал пользователь через категории
            foreach(var item in SelectedCategories) {
                string categoryName = item as string;
                if(categoryName == null) { continue; }

                elementsForWork.AddRange(DictCategoryElements[categoryName].ElementsInView);
            }

            // Перебираем выбранные через категории элементы и получаем их значения по выбранному параметру
            foreach(Element elem in elementsForWork) {

                PossibleValue possibleValue = new PossibleValue(elem, SelectedFilterableParameter);
                possibleValue.GetValue();

                // Записываем только уникальные
                int test = PossibleValues
                    .Where(str => str.ValueAsString.Equals(possibleValue.ValueAsString))
                    .ToList().Count;
                if(test == 0 && possibleValue.ValueAsString != null) {
                    PossibleValues.Add(possibleValue);
                }
            }

            PossibleValues = new ObservableCollection<PossibleValue>(PossibleValues.OrderBy(i => i.ValueAsString));
            OnPropertyChanged(nameof(PossibleValues));
        }



        /// <summary>
        /// Переводит выбранные значений для фильтрации в нормальный формат
        /// Отрабатывает при выборе возможных значений выбранного параметра фильтрации в списке значений GUI
        /// </summary>
        /// <param name="p"></param>
        private void SetPossibleValues(object p) {
            SelectedPossibleValues.Clear();

            // Забираем список выбранных значений через CommandParameter
            System.Collections.IList selectedPossibleValues = p as System.Collections.IList;

            // Получаем элементы, которые выбрал пользователь
            foreach(var item in selectedPossibleValues) {
                PossibleValue possibleValue = item as PossibleValue;
                if(possibleValue == null) { continue; }

                SelectedPossibleValues.Add(possibleValue);
            }
        }


        /// <summary>
        /// Создает фильтры на виде. Старые временные фильтры удаляет с вида. У существующих в проекте меняет категории
        /// Отрабатывает при нажатии кнопки "Ок" в GUI
        /// </summary>
        /// <param name="p"></param>
        private void Create(object p) {

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Добавление фильтров")) {

                int i = 0;
                int j = 0;

                AllFiltersInPj = _revitRepository.AllFilterElements;
                AllFilterNamesInPj = _revitRepository.AllFilterElementNames;
                string userName = _revitRepository.UserName;


                // Удаляем временные фильтры пользователя на виде
                _revitRepository.DeleteTempFiltersInView();

                foreach(PossibleValue pos in SelectedPossibleValues) {

                    string possibleValue = pos.ValueAsString;


                    string newFilterName = string.Format("${0}_{1}_{2}", userName, SelectedFilterableParameter.ParamName, possibleValue);

                    ParameterFilterElement parameterFilterElement = null;

                    if(AllFilterNamesInPj.Contains(newFilterName)) {

                        // Если создаваемый фильтр уже есть в проекте, то находим его
                        parameterFilterElement =  (from filter in AllFiltersInPj where filter.Name.Equals(newFilterName) select filter).FirstOrDefault();

                        // Назначаем выбранные категории
                        parameterFilterElement.SetCategories(SelectedCatIds);
                    } else {
                        // Если его нет в проекте, то создаем
                        parameterFilterElement = ParameterFilterElement
                            .Create(_revitRepository.Document, newFilterName, SelectedCatIds);

                        // Создаем правила фильтрации в зависимости от типа данных параметра
                        FilterRule filterRule = null;
                        if(pos.StorageParamType == StorageType.String) {
                            filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsString, true);
                        } else if(pos.StorageParamType == StorageType.Double) {
                            filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsDouble, 0.0000001);
                        } else if(pos.StorageParamType == StorageType.ElementId) {
                            filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsElementId);
                        } else if(pos.StorageParamType == StorageType.Integer) {
                            filterRule = ParameterFilterRuleFactory.CreateEqualsRule(SelectedFilterableParameter.Id, pos.ValueAsInteger);
                        }

                        if(filterRule is null) { continue; }

                        ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);

                        LogicalAndFilter elemFilter = new LogicalAndFilter(new List<ElementFilter>() { elemParamFilter });

                        // Задаем правила фильтрации объекту фильтра
                        parameterFilterElement.SetElementFilter(elemFilter);
                    }

                    // Применяем фильтр на вид
                    View view = _revitRepository.Document.ActiveView;
                    view.AddFilter(parameterFilterElement.Id);

                    OverrideGraphicSettings settings = GetOverrideGraphicSettings(i, j);
                    //// Если пользователь поставил галку перекрашивания
                    //if(OverrideByColor) {
                    //    settings.SetSurfaceForegroundPatternId(SolidFillPattern.Id);
                    //    settings.SetSurfaceForegroundPatternColor(Colors[i].UserColor);

                    //    settings.SetCutForegroundPatternId(SolidFillPattern.Id);
                    //    settings.SetCutForegroundPatternColor(Colors[i].UserColor);
                    //}
                    
                    //// Если пользователь поставил галку смены штриховки
                    //if(OverrideByPattern) {
                    //    settings.SetSurfaceForegroundPatternId(PatternsInPj[j].Pattern.Id);

                    //    settings.SetCutForegroundPatternId(PatternsInPj[j].Pattern.Id);
                    //}


                    view.SetFilterOverrides(parameterFilterElement.Id, settings);

                    if(++i > Colors.Count - 1) {
                        i = 0;
                    }
                    if(++j > PatternsInPj.Count - 1) {
                        j = 0;
                    }
                }

                transaction.Commit();
            }
        }

        private OverrideGraphicSettings GetOverrideGraphicSettings(int c, int p) {
            OverrideGraphicSettings settings = new OverrideGraphicSettings();
            // Если пользователь поставил галку перекрашивания
            if(OverrideByColor) {
                settings.SetSurfaceForegroundPatternId(SolidFillPattern.Id);
                settings.SetSurfaceForegroundPatternColor(Colors[c].UserColor);

                settings.SetCutForegroundPatternId(SolidFillPattern.Id);
                settings.SetCutForegroundPatternColor(Colors[c].UserColor);
            }

            // Если пользователь поставил галку смены штриховки
            if(OverrideByPattern) {
                settings.SetSurfaceForegroundPatternId(PatternsInPj[p].Pattern.Id);

                settings.SetCutForegroundPatternId(PatternsInPj[p].Pattern.Id);
            }
            return settings;
        }


        private bool CanCreate(object p) {
            return true;
        }



        private void ChangeColor(object p) {
                       
            ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();

            if(colorSelectionDialog.Show() == ItemSelectionDialogResult.Confirmed) {

                ColorHelper newColor = new ColorHelper(
                    colorSelectionDialog.SelectedColor.Red,
                    colorSelectionDialog.SelectedColor.Green,
                    colorSelectionDialog.SelectedColor.Blue);

                int index = Colors.IndexOf(SelectedColor);
                Colors[index] = newColor;
            }
        }
        private void AddColor(object p) {
            ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();

            if(colorSelectionDialog.Show() == ItemSelectionDialogResult.Confirmed) {
                Colors.Add(new ColorHelper(colorSelectionDialog.SelectedColor.Red, colorSelectionDialog.SelectedColor.Green, colorSelectionDialog.SelectedColor.Blue));
            }
        }
        private void DeleteColor(object p) {

            Colors.Remove(SelectedColor);
        }
        private void MoveColorUp(object p) {

            int index = Colors.IndexOf(SelectedColor);
            if(index != 0) {
                Colors.Move(index, index - 1);
            }
        }
        private void MoveColorDown(object p) {

            int index = Colors.IndexOf(SelectedColor);
            if(Colors.Count - 1 != index) {
                Colors.Move(index, index + 1);
            }
        }

        private bool CanChangeColor(object p) {
            if(SelectedColor is null) {
                return false;
            }
            return true;
        }



        private void AddPattern(object p) {

            List<FillPatternElement> allDraftingPatterns = _revitRepository.AllDraftingPatterns;

            if(allDraftingPatterns.Count > 0) {
                PatternsInPj.Add(new PatternsHelper(allDraftingPatterns[0], allDraftingPatterns));
            }
        }
        private void DeletePattern(object p) {

            PatternsInPj.Remove(SelectedPattern);
        }
        private void MovePatternUp(object p) {

            int index = PatternsInPj.IndexOf(SelectedPattern);
            if(index != 0) {
                PatternsInPj.Move(index, index - 1);
            }
        }
        private void MovePatternDown(object p) {

            int index = PatternsInPj.IndexOf(SelectedPattern);
            if(PatternsInPj.Count - 1 != index) {
                PatternsInPj.Move(index, index + 1);
            }
        }
        private bool CanChangePattern(object p) {
            if(SelectedPattern is null) {
                return false;
            }
            return true;
        }
    }
}