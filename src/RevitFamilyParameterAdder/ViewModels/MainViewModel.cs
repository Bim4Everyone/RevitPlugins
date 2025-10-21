using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFamilyParameterAdder.Models;

namespace RevitFamilyParameterAdder.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private bool _isAllSelected;
    private string _errorText;
    private readonly string _allGroup = "";
    private string _selectedParamGroupName;
    private bool _isParamsForKR;
    private bool _writeFormulasInParamsForKR;
    private List<string> _paramGroupNames = [];
    private readonly List<Guid> _paramGUIDsInFM = [];
    private readonly StringBuilder _reportAdded = new();
    private readonly StringBuilder _reportError = new();
    private readonly StringBuilder _reportAlreadyBeen = new();
    private List<SharedParam> _selectedParams = [];
    private List<ParameterGroupHelper> _bINParameterGroups = [];
    private readonly List<DefaultParam> _defaultParamsKR = [
#if REVIT_2023_OR_LESS
        new DefaultParam("обр_ФОП_Форма_префикс", BuiltInParameterGroup.PG_CONSTRUCTION, 
            "\"П\""),
        new DefaultParam("обр_ФОП_Форма_номер", BuiltInParameterGroup.PG_CONSTRUCTION, 
            "201"),
        new DefaultParam("обр_ФОП_Количество типовых этажей", BuiltInParameterGroup.PG_REBAR_ARRAY),
        new DefaultParam("обр_ФОП_Количество типовых на этаже", BuiltInParameterGroup.PG_REBAR_ARRAY),
        new DefaultParam("обр_ФОП_Длина", BuiltInParameterGroup.PG_GEOMETRY, 
            "roundup(мод_ФОП_Габарит А / 5) * 5 + roundup(мод_ФОП_Габарит Б / 5) * 5 + roundup(мод_ФОП_Габарит В / 5) * 5"),
        new DefaultParam("мод_ФОП_Габарит А", BuiltInParameterGroup.PG_GEOMETRY),
        new DefaultParam("мод_ФОП_Габарит Б", BuiltInParameterGroup.PG_GEOMETRY),
        new DefaultParam("мод_ФОП_Габарит В", BuiltInParameterGroup.PG_GEOMETRY),
        new DefaultParam("обр_ФОП_Изделие_Обозначение", BuiltInParameterGroup.PG_GENERAL),
        new DefaultParam("обр_ФОП_Изделие_Наименование", BuiltInParameterGroup.PG_GENERAL),
        new DefaultParam("обр_ФОП_Изделие_Марка", BuiltInParameterGroup.PG_GENERAL),
        new DefaultParam("обр_ФОП_Изделие_Главная деталь", BuiltInParameterGroup.PG_GENERAL),
        new DefaultParam("обр_ФОП_Габарит А_ВД", BuiltInParameterGroup.INVALID,
            "roundup((мод_ФОП_Габарит А) / 5) * 5"),
        new DefaultParam("обр_ФОП_Габарит Б_ВД", BuiltInParameterGroup.INVALID,
            "roundup((мод_ФОП_Габарит Б) / 5) * 5"),
        new DefaultParam("обр_ФОП_Габарит В_ВД", BuiltInParameterGroup.INVALID,
            "roundup((мод_ФОП_Габарит В) / 5) * 5")
#else
        new DefaultParam("обр_ФОП_Форма_префикс", GroupTypeId.Construction,
            "\"П\""),
        new DefaultParam("обр_ФОП_Форма_номер", GroupTypeId.Construction,
            "201"),
        new DefaultParam("обр_ФОП_Количество типовых этажей", GroupTypeId.RebarArray),
        new DefaultParam("обр_ФОП_Количество типовых на этаже", GroupTypeId.RebarArray),
        new DefaultParam("обр_ФОП_Длина", GroupTypeId.Geometry,
            "roundup(мод_ФОП_Габарит А / 5) * 5 + roundup(мод_ФОП_Габарит Б / 5) * 5 + roundup(мод_ФОП_Габарит В / 5) * 5"),
        new DefaultParam("мод_ФОП_Габарит А", GroupTypeId.Geometry),
        new DefaultParam("мод_ФОП_Габарит Б", GroupTypeId.Geometry),
        new DefaultParam("мод_ФОП_Габарит В", GroupTypeId.Geometry),
        new DefaultParam("обр_ФОП_Изделие_Обозначение", GroupTypeId.General),
        new DefaultParam("обр_ФОП_Изделие_Наименование", GroupTypeId.General),
        new DefaultParam("обр_ФОП_Изделие_Марка", GroupTypeId.General),
        new DefaultParam("обр_ФОП_Изделие_Главная деталь", GroupTypeId.General),
        new DefaultParam("обр_ФОП_Габарит А_ВД", ForgeTypeIdExtensions.EmptyForgeTypeId,
            "roundup((мод_ФОП_Габарит А) / 5) * 5"),
        new DefaultParam("обр_ФОП_Габарит Б_ВД", ForgeTypeIdExtensions.EmptyForgeTypeId,
            "roundup((мод_ФОП_Габарит Б) / 5) * 5"),
        new DefaultParam("обр_ФОП_Габарит В_ВД", ForgeTypeIdExtensions.EmptyForgeTypeId,
            "roundup((мод_ФОП_Габарит В) / 5) * 5")
#endif
    ];


    public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository, 
                         ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        _allGroup = _localizationService.GetLocalizedString("MainWindow.SelectAll");

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        GetParamsNSetParamFilterCommand = RelayCommand.Create(GetParamsNSetParamFilter);
        SelectionParamsCommand = RelayCommand.Create<object>(SelectionParams);
        SelectAllCommand = RelayCommand.Create<bool>(SelectAllItems);
    }


    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public ICommand GetParamsNSetParamFilterCommand { get; }
    public ICommand SelectionParamsCommand { get; }
    public ICommand SelectAllCommand { get; }

    public FamilyManager FamilyManagerFm { get; set; }

    public bool IsAllSelected {
        get => _isAllSelected;
        set => RaiseAndSetIfChanged(ref _isAllSelected, value);
    }

    public ObservableCollection<SharedParam> Params { get; set; } = [];
    public List<SharedParam> SelectedParams {
        get => _selectedParams;
        set => RaiseAndSetIfChanged(ref _selectedParams, value);
    }


    public List<string> ParamGroupNames {
        get => _paramGroupNames;
        set => RaiseAndSetIfChanged(ref _paramGroupNames, value);
    }

    public string SelectedParamGroupName {
        get => _selectedParamGroupName;
        set => RaiseAndSetIfChanged(ref _selectedParamGroupName, value);
    }

    public List<ParameterGroupHelper> BINParameterGroups {
        get => _bINParameterGroups;
        set => RaiseAndSetIfChanged(ref _bINParameterGroups, value);
    }


    public bool IsParamsForKR {
        get => _isParamsForKR;
        set {
            RaiseAndSetIfChanged(ref _isParamsForKR, value);
            WriteFormulasInParamsForKR = false;
        }
    }

    public bool WriteFormulasInParamsForKR {
        get => _writeFormulasInParamsForKR;
        set => RaiseAndSetIfChanged(ref _writeFormulasInParamsForKR, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }



    /// <summary>
    /// Метод для команды,отрабатывающей во время загрузки окна
    /// </summary>
    private void LoadView() {
        IsParamsForKR = true;
        WriteFormulasInParamsForKR = true;
        FamilyManagerFm = _revitRepository.Document.FamilyManager;

        // Подгружаем сохраненные данные прошлого запуска
        LoadConfig();
        // Получаем имена групп параметров из ФОП
        GetParamGroupNames();
        SelectedParamGroupName = _allGroup;
        // Получаем параметры из ФОП и применяем установленные фильтры
        GetParamsNSetParamFilter();
        // Получаем группы параметров, доступные пользователю
        GetBuiltInParameterGroups();
        // Получаем параметры, которые уже есть в семействе
        GetFamilyParams();
    }

    /// <summary>
    /// Метод для команды, отрабатывающий при нажатии "Ок"
    /// </summary>
    private void AcceptView() {
        // Метод по добавлению параметров
        Add();
        SaveConfig();
    }
    private bool CanAcceptView() {
        var selectedParams = Params.Where(p => p.IsSelected).ToList();

        if(selectedParams.Count > 0) {
            foreach(SharedParam item in selectedParams) {
                if(item.SelectedParamGroupInFM is null) {
                    ErrorText = _localizationService.GetLocalizedString("MainWindow.SetGroupToAllSelectedParams");
                    return false;
                }
            }
            ErrorText = string.Empty;
            return true;
        } else {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.SelectParamsForAdd");
            return false;
        }
    }

    /// <summary>
    /// Добавляет выбранные параметры в семейство
    /// </summary>
    private void Add() {
        using(var t = new Transaction(_revitRepository.Document)) {
            t.Start(_localizationService.GetLocalizedString("MainWindow.Title"));

            // Перебираем параметры с установленным флагом IsSelected
            foreach(SharedParam param in Params.Where(p => p.IsSelected)) {
                //Если семейство уже имеет параметр, пропускаем, идем дальше
                if(_paramGUIDsInFM.Contains(param.ParamInShPF.GUID)) {
                    _reportAlreadyBeen.AppendLine(param.ParamName);
                    continue;
                }

                try {
#if REVIT_2023_OR_LESS
                FamilyParameter familyParam = FamilyManagerFm.AddParameter(
                    param.ParamInShPF, 
                    (BuiltInParameterGroup) param.SelectedParamGroupInFM.BuiltInParamGroup, 
                    param.IsInstanceParam);
#else
                    FamilyParameter familyParam = FamilyManagerFm.AddParameter(
                        param.ParamInShPF,
                        (ForgeTypeId) param.SelectedParamGroupInFM.BuiltInParamGroup,
                        param.IsInstanceParam);
#endif
                    _reportAdded.AppendLine(param.ParamName);

                } catch(Exception) {
                    _reportError.AppendLine(param.ParamName);
                }
            }

            // Сначала проверяем, есть ли типоразмеры в семействе, если нет явно создаем дефолтный с именем семейства
            // Без этого формулы не добавить
            FamilyTypeSet familyTypeSet = FamilyManagerFm.Types;

            if(WriteFormulasInParamsForKR == true && familyTypeSet.Size == 0) {
                FamilyType newFamilyType = FamilyManagerFm.NewType(_revitRepository.Document.Title);
            }

            // Назначаем добавленным параметрам формулы,если стоит соответствующая галка и есть формула
            foreach(SharedParam param in SelectedParams) {
                if(WriteFormulasInParamsForKR == false || param.Formula == string.Empty) {
                    continue;
                }
                FamilyParameter familyParam = FamilyManagerFm.get_Parameter(param.ParamName);
                // Если параметр найден в семействе и у него пустой блок заполнения формул и он общий и в него можно
                // добавить формулу, добавляем
                if(familyParam == null || familyParam.IsShared == false || familyParam.CanAssignFormula == false) {
                    continue;
                }

                try {
                    FamilyManagerFm.SetFormula(familyParam, param.Formula);

                } catch(Exception) {
                    _reportError.AppendLine(param.ParamName);
                }
            }
            t.Commit();
        }

        if(_reportAdded.Length > 0) {
            _reportAdded.Insert(0, _localizationService.GetLocalizedString("MainWindow.AddedParams") 
                                   + Environment.NewLine);
            TaskDialog.Show(_localizationService.GetLocalizedString("MainWindow.Report"), _reportAdded.ToString());
        }
        if(_reportAlreadyBeen.Length > 0) {
            _reportAlreadyBeen.Insert(0, _localizationService.GetLocalizedString("MainWindow.ParamsWereAlready") 
                                         + Environment.NewLine);
            TaskDialog.Show(_localizationService.GetLocalizedString("MainWindow.Report"), _reportAlreadyBeen.ToString());
        }
        if(_reportError.Length > 0) {
            _reportError.Insert(0, _localizationService.GetLocalizedString("MainWindow.ErrorWithAddingParams") 
                                   + Environment.NewLine);
            TaskDialog.Show(_localizationService.GetLocalizedString("MainWindow.Report"), _reportError.ToString());
        }
    }

    /// <summary>
    /// Получение параметров из ФОП с переводом в оболочку SharedParam
    /// </summary>
    private void GetParams() {
        Params.Clear();
        List<ExternalDefinition> paramsInShPF = _revitRepository.GetParamsInShPF();
        foreach(ExternalDefinition item in paramsInShPF) {
            SharedParam sharedParam = new SharedParam(item, BINParameterGroups, _localizationService);
            Params.Add(sharedParam);
        }
    }

    /// <summary>
    /// Получение параметров из ФОП и применение фильтров по группе параметров ФОП и галочке "для КР"
    /// </summary>
    private void GetParamsNSetParamFilter() {
        GetParams();
        if(!SelectedParamGroupName.Equals(_allGroup)) {
            Params = new ObservableCollection<SharedParam>(
                    Params
                    .Where(item => item.ParamGroupInShPF.Equals(SelectedParamGroupName))
                    .ToList());
        }
        if(IsParamsForKR) {
            foreach(SharedParam item in Params) {
                foreach(DefaultParam defParam in _defaultParamsKR) {
                    if(defParam.ParamName == item.ParamName) {
                        item.IsDefaultParam = true;
                        item.Formula = defParam.Formula;
                        // Отложенная установка после привязки ItemsSource
                        Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => {
                            item.SelectedParamGroupInFM =
                                item.ParamGroupsInFM.FirstOrDefault(e => e.GroupName == defParam.BINParameterGroup.GroupName);
                        }), DispatcherPriority.Loaded);
                    }
                }
            }
            Params = new ObservableCollection<SharedParam>(Params.Where(item => item.IsDefaultParam));
        }
        // Сбрасываем выделение всех при изменении фильтра
        IsAllSelected = false;
        OnPropertyChanged(nameof(Params));
    }

    /// <summary>
    /// Заполняем список выбранных параметров для добавления. Работает по событию выбора в ListView
    /// </summary>
    /// <param name="p"></param>
    private void SelectionParams(object p) {
        // Забираем список выбранных элементов через CommandParameter
        SelectedParams.Clear();
        foreach(object item in p as System.Collections.IList) {
            if(item is not SharedParam sharedParam) {
                continue;
            }
            SelectedParams.Add(sharedParam);
        }
    }

    /// <summary>
    /// Получение имен групп параметров в ФОП
    /// </summary>
    private void GetParamGroupNames() {
        ParamGroupNames = _revitRepository.GetParamGroupNames();
        ParamGroupNames.Insert(0, _allGroup);
    }

    /// <summary>
    /// Получение групп параметров семейства (для группировки параметров в списке параметров семейства)
    /// </summary>
    private void GetBuiltInParameterGroups() {

#if REVIT_2023_OR_LESS
        // Забираем все встроенные группы параметров
        Array array = Enum.GetValues(typeof(BuiltInParameterGroup));
        
        // Отбираем только те, что отображаются у пользователя
        foreach(BuiltInParameterGroup group in array) {
            if(FamilyManagerFm.IsUserAssignableParameterGroup(group)) {
                BINParameterGroups.Add(new ParameterGroupHelper(group));
            }
        }
#else
        Array array = typeof(GroupTypeId).GetProperties();
        BINParameterGroups.Add(new ParameterGroupHelper(ForgeTypeIdExtensions.EmptyForgeTypeId));
        foreach(PropertyInfo group in array) {
            if(FamilyManagerFm.IsUserAssignableParameterGroup((ForgeTypeId) group.GetValue(null))) {
                BINParameterGroups.Add(new ParameterGroupHelper((ForgeTypeId) group.GetValue(null)));
            }
        }
#endif
        BINParameterGroups.Sort((x, y) => x.GroupName.CompareTo(y.GroupName));
    }

    /// <summary>
    /// Получение параметров, которые уже имеются в семействе
    /// </summary>
    private void GetFamilyParams() {
        foreach(FamilyParameter familyParameter in FamilyManagerFm.Parameters) {
            // Отбираем имена общих параметров, которые уже есть в семействе
            if(familyParameter.IsShared == true) {
                _paramGUIDsInFM.Add(familyParameter.GUID);
            }
        }
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        _pluginConfig.SaveProjectConfig();
    }

    private void SelectAllItems(bool isSelected) {
        SelectAllVisibleItems(isSelected);
        IsAllSelected = isSelected;
    }

    /// <summary>
    /// Выделение/снятие выделения всех видимых элементов
    /// </summary>
    private void SelectAllVisibleItems(bool isSelected) {
        foreach(var param in Params) {
            param.IsSelected = isSelected;
        }
        // Обновляем SelectedParams
        UpdateSelectedParams();
    }

    /// <summary>
    /// Обновление списка выбранных параметров
    /// </summary>
    private void UpdateSelectedParams() {
        SelectedParams.Clear();
        foreach(var param in Params.Where(p => p.IsSelected)) {
            SelectedParams.Add(param);
        }
    }
}
