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

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView);
            GetParamsNSetParamFilterCommand = RelayCommand.Create(GetParamsNSetParamFilter);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand GetParamsNSetParamFilterCommand { get; }



        public ObservableCollection<SharedParam> Params { get; set; } = new ObservableCollection<SharedParam>();
        public System.Collections.IList SelectedParams { get; set; }             // Список меток основ, которые выбрал пользователь




        public List<string> ParamGroupNames {
            get => _paramGroupNames;
            set => this.RaiseAndSetIfChanged(ref _paramGroupNames, value);
        }

        public string SelectedParamGroupName {
            get => _selectedParamGroupName;
            set => this.RaiseAndSetIfChanged(ref _selectedParamGroupName, value);
        }


        public bool IsParamsForKR {
            get => _isParamsForKR;
            set => this.RaiseAndSetIfChanged(ref _isParamsForKR, value);
        }







        //private string _hostMarkForSearch = string.Empty;
        //public string HostMarkForSearch {
        //    get => _hostMarkForSearch;
        //    set {
        //        //this.RaiseAndSetIfChanged(ref _hostMarkForSearch, value);
        //        _hostMarkForSearch = value;
        //    }
        //}







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
        }
        private void AcceptView() {

            //Add();
            SaveConfig();
        }


        private void Add() {
            //Transaction transaction = new Transaction(_revitRepository.Document, "Добавление параметров");
            //transaction.Start();



            //transaction.Commit();






            DefinitionFile sharedParametersFile = _revitRepository.Application.OpenSharedParameterFile();


            FamilyManager familyManager = _revitRepository.Document.FamilyManager;
            IList<FamilyParameter> existfamilyPar = familyManager.GetParameters();


            // Проходимся по каждой группе ФОП
            foreach(DefinitionGroup sharedGroup in sharedParametersFile.Groups) {

                var paramsInGroup = (from ExternalDefinition def in sharedGroup.Definitions select def);


                using(Transaction t = new Transaction(_revitRepository.Document)) {
                    t.Start("Add Shared Parameters");
                    // Перебираем параметры в группе
                    //foreach(ExternalDefinition paramInShPF in paramsInGroup) {

                    //    if(defaultParamsKR.Contains(paramInShPF.Name)) {
                    //        //FamilyParameter fp = familyManager.AddParameter(paramInShPF, BuiltInParameterGroup.PG_ELECTRICAL_LIGHTING, false);
                    //        Params.Add(paramInShPF.Name);
                    //    }
                    //}
                    t.Commit();
                }
            }
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
                Params.Add(new SharedParam(item));
            }
        }

        private void GetParamsNSetParamFilter() {
            GetParams();
            if(!SelectedParamGroupName.Equals(_allGroup)) {
                Params = new ObservableCollection<SharedParam>(
                        Params
                        .Where(item => item.ParamGroupInShPF.Equals(SelectedParamGroupName))
                        .ToList());
                OnPropertyChanged(nameof(Params));
            }

            if(IsParamsForKR) {
                Params = new ObservableCollection<SharedParam>(
                        Params
                        .Where(item => _defaultParamsKR.Contains(item.ParamName))
                        .ToList());
                OnPropertyChanged(nameof(Params));
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