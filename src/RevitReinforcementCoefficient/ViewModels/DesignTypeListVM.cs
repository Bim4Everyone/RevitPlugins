using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitReinforcementCoefficient.Models.Analyzers;
using RevitReinforcementCoefficient.Models.Report;
using RevitReinforcementCoefficient.Views;

namespace RevitReinforcementCoefficient.ViewModels;
internal class DesignTypeListVM : BaseViewModel {
    public delegate void Notify();
    public event Notify UpdateReportData;

    private readonly DesignTypeListAnalyzer _designTypeListAnalyzer;
    private readonly DesignTypeAnalyzer _designTypeAnalyzer;
    private readonly IReportService _reportService;
    private readonly ReportWindow _reportWindow;

    private readonly string _reinforcement = "ФОП_ТИП_Армирование";

    private List<DesignTypeVM> _designTypes = [];

    public DesignTypeListVM(
        DesignTypeListAnalyzer designTypeListAnalyzer,
        DesignTypeAnalyzer designTypeAnalyzer,
        IReportService reportService,
        ReportWindow reportWindow) {

        _designTypeListAnalyzer = designTypeListAnalyzer;
        _designTypeAnalyzer = designTypeAnalyzer;
        _reportService = reportService;
        _reportWindow = reportWindow;
    }
    public List<DesignTypeVM> DesignTypes {
        get => _designTypes;
        set => RaiseAndSetIfChanged(ref _designTypes, value);
    }
    public MainViewModel MainVM { get; private set; }

    public bool GetElementsForAnalize() {
        return _designTypeListAnalyzer.GetElementsForAnalize();
    }

    public void GetDesignTypes() {
        DesignTypes = _designTypeListAnalyzer.CheckNSortByDesignTypes();
        ShowReportIfNeed();
    }

    private void ShowReportIfNeed() {
        if(_reportService.ReportItems.Count > 0) {
            UpdateReportData?.Invoke();
            _reportWindow.ShowDialog();
            _reportService.ClearReportItems();
        }
    }


    /// <summary>
    /// Используется в качестве аргумента предиката для фильтрации списка по выбранному комплекту документации
    /// </summary>
    public bool FilterByDocPackage(object o) {
        if(MainVM == null) {
            throw new InvalidOperationException(nameof(MainVM));
        }

        bool turnOffDockPackageFilt = MainVM.SelectedDockPackage == MainVM.FilterValueForNoFiltering;
        bool turnOffElemSectionFilt = MainVM.SelectedElemSection == MainVM.FilterValueForNoFiltering;

        if(turnOffDockPackageFilt && turnOffElemSectionFilt) {
            return true;
        }

        if(!turnOffDockPackageFilt && turnOffElemSectionFilt) {
            return CheckByDocPackage(o);
        } else {
            return CheckByDocPackage(o) && CheckByElemSection(o);
        }
    }

    private bool CheckByDocPackage(object o) {
        if(string.IsNullOrEmpty(MainVM.SelectedDockPackage)) {
            return string.IsNullOrEmpty(((DesignTypeVM) o).DocPackage);
        } else {
            return ((DesignTypeVM) o).DocPackage is null ? false : ((DesignTypeVM) o).DocPackage.Equals(MainVM.SelectedDockPackage);
        }
    }

    private bool CheckByElemSection(object o) {
        return string.IsNullOrEmpty(MainVM.SelectedElemSection)
            ? string.IsNullOrEmpty(((DesignTypeVM) o).ElemSection)
            : ((DesignTypeVM) o).ElemSection is not null && ((DesignTypeVM) o).ElemSection.Equals(MainVM.SelectedElemSection);
    }


    public void GetInfo(bool calcСoefficientOnAverage) {
        var selectedDesignTypes = DesignTypes.Where(o => o.IsCheck).ToList();

        foreach(var designType in selectedDesignTypes) {
            // Проверка элементов выбранного типа конструкции
            if(!designType.FormParamsChecked) {
                _designTypeAnalyzer.CheckParamsInFormworks(designType);
            }
            if(!designType.RebarParamsChecked) {
                _designTypeAnalyzer.CheckParamsInRebars(designType);
            }

            // Если есть ошибки либо в опалубке, либо в арматуре подсчет выполняться не будет, т.к. нужны оба
            if(!designType.HasErrors && !designType.AlreadyCalculated) {
                // Выполняем расчет объема опалубки
                designType.CalculateFormworks();
                // Выполняем расчет массы арматуры
                designType.CalculateRebars();
            }
        }

        // В зависимости от выбора пользователя, рассчитываем коэффициент усредненно или по отдельности
        if(calcСoefficientOnAverage) {
            if(selectedDesignTypes.All(o => !o.HasErrors)) {
                CalculateRebarCoefBySeveral(selectedDesignTypes);
            }
        } else {
            foreach(var designType in selectedDesignTypes) {
                if(!designType.HasErrors) {
                    designType.CalculateRebarCoef();
                }
            }
        }
        ShowReportIfNeed();
    }

    /// <summary>
    /// Запись значений коэффициентов армирования
    /// </summary>
    public void WriteRebarCoef(Document doc) {
        foreach(var designType in DesignTypes.Where(o => o.IsCheck)) {
            // Если нет ошибок то выполняем запись значений коэффициента армирования в опалубочные элементы
            if(!designType.HasErrors) {
                using var transaction = doc.StartTransaction("Запись коэффициентов армирования");
                foreach(var elem in designType.Formworks) {
                    elem.RevitElement.SetParamValue(_reinforcement, designType.RebarCoef);
                }
                _ = transaction.Commit();
            }
        }
    }

    /// <summary>
    /// Ставит галочки выбора у видимых с учетом фильтрации типов конструкций
    /// </summary>
    public void SelectAllVisible() {
        foreach(var item in DesignTypes.Where(FilterByDocPackage)) {
            item.IsCheck = true;
        }
    }

    /// <summary>
    /// Снимает галочки выбора у видимых с учетом фильтрации типов конструкций
    /// </summary>
    public void UnselectAllVisible() {
        foreach(var item in DesignTypes.Where(FilterByDocPackage)) {
            item.IsCheck = false;
        }
    }

    /// <summary>
    /// Задаем фильтрацию списка типов конструкций по выбранному комплекту документации
    /// </summary>
    public void SetFiltering(MainViewModel mainViewModel) {
        MainVM = mainViewModel;
        CollectionViewSource.GetDefaultView(DesignTypes).Filter = new Predicate<object>(FilterByDocPackage);
    }

    /// <summary>
    /// Обновляем элемент в UI, привязанный к коллекции типов конструкций с учетом фильтрации
    /// </summary>
    public void UpdateFiltering() {
        CollectionViewSource.GetDefaultView(DesignTypes).Refresh();
    }


    /// <summary>
    /// Рассчитывает коэффициент армирования у нескольких типов конструкции по массе арматуры и объему бетона
    /// </summary>
    private void CalculateRebarCoefBySeveral(List<DesignTypeVM> typesInfo) {
        double totalRebarMass = 0;
        double totalConcreteVolume = 0;

        // Получаем суммарный объем бетона опалубки и суммарную массу арматуры
        foreach(var typeInfo in typesInfo) {
            totalRebarMass += typeInfo.RebarMass;
            totalConcreteVolume += typeInfo.FormworkVolume;
        }
        // Рассчитываем коэф армирования
        string averageReinforcementCoefficient = Math.Round(totalRebarMass / totalConcreteVolume).ToString(CultureInfo.GetCultureInfo("ru-Ru"));

        // Записываем рассчитанный коэф армирования в типы конструкций, по которым считали
        foreach(var typeInfo in typesInfo) {
            typeInfo.RebarCoef = averageReinforcementCoefficient;
            typeInfo.AlreadyCalculated = true;
        }
    }
}
