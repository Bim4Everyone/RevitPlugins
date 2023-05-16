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

namespace RevitFamilyParameterAdder.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private string _saveProperty;
        private string _allGroup = "<Выбрать все>";
        private string _selectedParamGroupName;
        private bool _isParamsForKR;
        private List<string> _paramGroupNames = new List<string>();
        private List<string> _paramsInFM = new List<string>();
        public StringBuilder _report = new StringBuilder();
        private List<SharedParam> _selectedParams = new List<SharedParam>();
        private List<ParameterGroupHelper> _bINParameterGroups = new List<ParameterGroupHelper>();
        private List<string> _defaultParamsKR = new List<string>() {
            "обр_ФОП_Форма_префикс",
            "обр_ФОП_Форма_номер",
            "обр_ФОП_Количество типовых этажей",
            "обр_ФОП_Количество типовых на этаже",
            "обр_ФОП_Длина",
            "мод_ФОП_Габарит А",
            "мод_ФОП_Габарит Б",
            "мод_ФОП_Габарит В",
            "обр_ФОП_Изделие_Обозначение",
            "обр_ФОП_Изделие_Наименование",
            "обр_ФОП_Изделие_Марка",
            "обр_ФОП_Изделие_Главная деталь",
            "обр_ФОП_Габарит А_ВД",
            "обр_ФОП_Габарит Б_ВД",
            "обр_ФОП_Габарит В_ВД"
        };

   
        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            FamilyManagerFm = _revitRepository.Document.FamilyManager;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            GetParamsNSetParamFilterCommand = RelayCommand.Create(GetParamsNSetParamFilter);
            SelectionParamsCommand = new RelayCommand(SelectionParams);
        }


        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand GetParamsNSetParamFilterCommand { get; }
        public ICommand SelectionParamsCommand { get; }







        public ObservableCollection<SharedParam> Params { get; set; } = new ObservableCollection<SharedParam>();
        public List<SharedParam> SelectedParams {
            get => _selectedParams;
            set => this.RaiseAndSetIfChanged(ref _selectedParams, value);
        }


        public FamilyManager FamilyManagerFm { get; set; }

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
            set => this.RaiseAndSetIfChanged(ref _isParamsForKR, value);
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
            SelectedParamGroupName = _allGroup;

            // Подгружаем сохраненные данные прошлого запуска
            LoadConfig();
            // Получаем имена групп параметров из ФОП
            GetParamGroupNames();
            // Получаем параметры из ФОП и применяем установленные фильтры
            GetParamsNSetParamFilter();
            // Получаем группы параметров, доступные пользователю
            GetBuiltInParameterGroups();
            // Получаем параметры, которые уже есть в семействе
            GetFamilyParams();
        }
        private void AcceptView() {

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


        private void GetFamilyParams() {
            foreach(FamilyParameter familyParameter in FamilyManagerFm.Parameters) {
                _paramsInFM.Add(familyParameter.Definition.Name);
            }
        }



        private void Add() {

            using(Transaction t = new Transaction(_revitRepository.Document)) {
                t.Start("Добавление параметров");
                // Перебираем параметры в группе
                foreach(SharedParam param in SelectedParams) {
                    //Если семейство уже имеет параметр,пропускаем, идем дальше
                    if(_paramsInFM.Contains(param.ParamName)) {
                        _report.AppendLine(string.Format("Параметр {0} уже имеется в семействе", param.ParamName));
                        continue; 
                    }

                    try {
                        FamilyParameter familyParam = FamilyManagerFm.AddParameter(param.ParamInShPF, param.SelectedParamGroupInFM.BuiltInParamGroup, true);
                        _report.AppendLine(string.Format("Добавлен параметр {0} в группу {1} на уровень экземпляра", param.ParamName, param.SelectedParamGroupInFM.GroupName));

                    } catch(Exception) {
                        _report.AppendLine(string.Format("При добавлении параметра {0} произошла ошибка", param.ParamName));
                    }
                }
                t.Commit();
            }
            TaskDialog.Show("Отчет", _report.ToString());
        }



        private void GetParamGroupNames() {
            ParamGroupNames = _revitRepository.GetParamGroupNames();
            ParamGroupNames.Sort();
            ParamGroupNames.Insert(0, _allGroup);
        }

        private void GetParams() {
            Params.Clear();
            List<ExternalDefinition> paramsInShPF = _revitRepository.GetParamsInShPF();
            foreach(ExternalDefinition item in paramsInShPF) {
                Params.Add(new SharedParam(item, BINParameterGroups));
            }
        }

        private void GetParamsNSetParamFilter() {
            GetParams();
            if(!SelectedParamGroupName.Equals(_allGroup)) {
                Params = new ObservableCollection<SharedParam>(
                        Params
                        .Where(item => item.ParamGroupInShPF.Equals(SelectedParamGroupName))
                        .ToList());
            }

            if(IsParamsForKR) {
                Params = new ObservableCollection<SharedParam>(
                        Params
                        .Where(item => _defaultParamsKR.Contains(item.ParamName))
                        .ToList());
            }
            OnPropertyChanged(nameof(Params));
        }


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


        private void GetBuiltInParameterGroups() {
            // Забираем все встроенные группы параметров
            Array array = Enum.GetValues(typeof(BuiltInParameterGroup));
            
            // Отбираем только те, что отображаются у пользователя
            foreach(BuiltInParameterGroup group in array) {
                if(FamilyManagerFm.IsUserAssignableParameterGroup(group)) {
                    BINParameterGroups.Add(new ParameterGroupHelper(group));
                }
            }
            BINParameterGroups.OrderBy(i => i.GroupName);
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