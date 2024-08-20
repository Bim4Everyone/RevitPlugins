using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitFamilyParameterAdder.Models;
using dosymep.WPF.Commands;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows.Controls;
using Ninject.Infrastructure.Language;
using static Autodesk.AdvanceSteel.Modelling.Beam;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls;
using dosymep.Revit;

namespace RevitFamilyParameterAdder.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private string _saveProperty;
        private string _allGroup = "<Выбрать все>";
        private string _selectedParamGroupName;
        private bool _isParamsForKR;
        private bool _writeFormulasInParamsForKR;
        private List<string> _paramGroupNames = new List<string>();
        private List<Guid> _paramGUIDsInFM = new List<Guid>();
        public StringBuilder _reportAdded = new StringBuilder();
        public StringBuilder _reportError = new StringBuilder();
        public StringBuilder _reportAlreadyBeen = new StringBuilder();
        private List<SharedParam> _selectedParams = new List<SharedParam>();
        private List<ParameterGroupHelper> _bINParameterGroups = new List<ParameterGroupHelper>();
        private List<DefaultParam> _defaultParamsKR = new List<DefaultParam>() {
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
        };

   
        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            FamilyManagerFm = _revitRepository.Document.FamilyManager;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            GetParamsNSetParamFilterCommand = RelayCommand.Create(GetParamsNSetParamFilter);
            SelectionParamsCommand = RelayCommand.Create<object>(SelectionParams);
        }


        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand GetParamsNSetParamFilterCommand { get; }
        public ICommand SelectionParamsCommand { get; }



        public FamilyManager FamilyManagerFm { get; set; }

        public ObservableCollection<SharedParam> Params { get; set; } = new ObservableCollection<SharedParam>();
        public List<SharedParam> SelectedParams {
            get => _selectedParams;
            set => this.RaiseAndSetIfChanged(ref _selectedParams, value);
        }


        public List<string> ParamGroupNames {
            get => _paramGroupNames;
            set => this.RaiseAndSetIfChanged(ref _paramGroupNames, value);
        }

        public string SelectedParamGroupName {
            get => _selectedParamGroupName;
            set => this.RaiseAndSetIfChanged(ref _selectedParamGroupName, value);
        }

        public List<ParameterGroupHelper> BINParameterGroups {
            get => _bINParameterGroups;
            set => this.RaiseAndSetIfChanged(ref _bINParameterGroups, value);
        }


        public bool IsParamsForKR {
            get => _isParamsForKR;
            set {
                this.RaiseAndSetIfChanged(ref _isParamsForKR, value);
                WriteFormulasInParamsForKR = false;
            }
        }

        public bool WriteFormulasInParamsForKR {
            get => _writeFormulasInParamsForKR;
            set => this.RaiseAndSetIfChanged(ref _writeFormulasInParamsForKR, value);
        }


        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }



        /// <summary>
        /// Метод для команды,отрабатывающей во время загрузки окна
        /// </summary>
        private void LoadView() {
            IsParamsForKR = true;
            WriteFormulasInParamsForKR = true;

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
        /// Метод для команды, отрабатываеющий при нажатии "Ок"
        /// </summary>
        private void AcceptView() {
            // Метод по добавлению параметров
            Add();
            SaveConfig();
        }
        private bool CanAcceptView() {
            if(SelectedParams.Count > 0) {
                foreach(SharedParam item in SelectedParams) {
                    if(item.SelectedParamGroupInFM is null) {
                        ErrorText = "Назначьте группу всем выбранным параметрам";
                        return false;
                    }
                }
                ErrorText = string.Empty;
                return true;
            } else {
                ErrorText = "Выберите параметры для добавления";
                return false;
            }
        }



        private void Add() {
            using(Transaction t = new Transaction(_revitRepository.Document)) {
                t.Start("Добавление параметров");

                // Перебираем параметры в группе
                foreach(SharedParam param in SelectedParams) {
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
                    // Если параметр найден в семействе и у него пустой блок заполнения формул и он общий и в него можно добавить формулу, добавляем
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
                _reportAdded.Insert(0, "Добавлены параметры:" + Environment.NewLine);
                TaskDialog.Show("Отчет", _reportAdded.ToString());
            }
            if(_reportAlreadyBeen.Length > 0) {
                _reportAlreadyBeen.Insert(0, "Параметры уже были:" + Environment.NewLine);
                TaskDialog.Show("Отчет", _reportAlreadyBeen.ToString());
            }
            if(_reportError.Length > 0) {
                _reportError.Insert(0, "Произошла ошибка при добавлении параметров:" + Environment.NewLine);
                TaskDialog.Show("Отчет", _reportError.ToString());
            }
        }




        /// <summary>
        /// Получение параметров из ФОП с переводом в оболочку SharedParam
        /// </summary>
        private void GetParams() {
            Params.Clear();
            List<ExternalDefinition> paramsInShPF = _revitRepository.GetParamsInShPF();
            foreach(ExternalDefinition item in paramsInShPF) {
                SharedParam sharedParam = new SharedParam(item, BINParameterGroups);
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
                            item.SelectedParamGroupInFM = defParam.BINParameterGroup;
                        }
                    }
                }

                Params = new ObservableCollection<SharedParam>(
                        Params
                        .Where(item => item.IsDefaultParam));

            }
            OnPropertyChanged(nameof(Params));
        }

        /// <summary>
        /// Заполняем список выбранных параметров для добавления. Работает по событию выбора в ListView
        /// </summary>
        /// <param name="p"></param>
        private void SelectionParams(object p) {
            // Забираем список выбранных элементов через CommandParameter
            SelectedParams.Clear();
            foreach(var item in p as System.Collections.IList) {
                SharedParam sharedParam = item as SharedParam;
                if(sharedParam == null) {
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

            SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }

    }
}