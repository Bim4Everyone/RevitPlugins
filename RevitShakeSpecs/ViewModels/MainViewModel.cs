using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitShakeSpecs.Models;

namespace RevitShakeSpecs.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        public readonly RevitRepository _revitRepository;

        private string _errorText;
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }



        public List<string> ProjectSectionParamNames { get; set; } = new List<string>();                            // Параметры листов

        private string _selectedProjectSectionParamName;
        public string SelectedProjectSectionParamName {
            get => _selectedProjectSectionParamName;
            set {
                this.RaiseAndSetIfChanged(ref _selectedProjectSectionParamName, value);
            }
        }




        public ObservableCollection<string> ProjectSections { get; set; } = new ObservableCollection<string>();     // Разделы проекта
        private string _selectedProjectSection { get; set; }                                                        
        public string SelectedProjectSection {
            get => _selectedProjectSection;
            set {
                _selectedProjectSection = value;
            }
        }








        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;


            LoadConfig();
            GetFilterableParams();
            GetProjectSections(null);

            GetProjectSectionsCommand = new RelayCommand(GetProjectSections);
            RegenerateCommand = new RelayCommand(Regenerate, CanShake);
            ShakeCommand = new RelayCommand(Shake, CanShake);
        }









        public ICommand RegenerateCommand { get; }
        private void Regenerate(object p) {
            using(Transaction transaction = new Transaction(_revitRepository.Document)) {
                transaction.Start("Регенирация документа");
                _revitRepository.Document.Regenerate();
                transaction.Commit();
            }

            SaveConfig();
        }




        public ICommand ShakeCommand { get; }
        private void Shake(object p) {

            using(Transaction transaction = new Transaction(_revitRepository.Document)) {
                transaction.Start("Встряска спецификаций");

                foreach(var item in _revitRepository.AllExistingSheets) {
                    ViewSheet viewSheet = item as ViewSheet;

                    var paramValueTemp = viewSheet.GetParamValueOrDefault(SelectedProjectSectionParamName);
                    if(paramValueTemp is null) { continue; }
                    string paramValue = paramValueTemp.ToString();

                    if(paramValue != SelectedProjectSection) {
                        continue;
                    }

                    SheetUtils sheetUtils = new SheetUtils(this, viewSheet);
                    sheetUtils.FindAndShakeSpecsOnSheet();
                }

                transaction.Commit();
            }

            SaveConfig();
        }
        private bool CanShake(object p) {
            if(ErrorText.Length > 0) {
                return false;
            }
            if(SelectedProjectSection is null) {
                return false;
            }
            return true;
        }














        // Заполнение списка параметров, где может быть прописан раздел проекта
        private void GetFilterableParams() {
            Category category = _revitRepository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);

            var filterableParametersIds = ParameterFilterUtilities.GetFilterableParametersInCommon(_revitRepository.Document, new List<ElementId> { category.Id });

            foreach(var id in filterableParametersIds) {
                ParameterElement param = _revitRepository.Document.GetElement(id) as ParameterElement;
                if(param is null) { continue; }

                ProjectSectionParamNames.Add(param.Name);
            }

            ProjectSectionParamNames.Sort();
        }



        // Получение возможных вариантов заполнения параметра раздела проекта
        public ICommand GetProjectSectionsCommand { get; }

        private void GetProjectSections(object p) {
            ProjectSections.Clear();
            ErrorText = string.Empty;
            string projectSection;
            foreach(var item in _revitRepository.AllExistingSheets) {
                ViewSheet viewSheet = item as ViewSheet;
                if(viewSheet is null) { continue; }

                var projectSectionTemp = viewSheet.GetParamValueOrDefault(SelectedProjectSectionParamName);
                if(projectSectionTemp is null) { continue; }
                projectSection = projectSectionTemp.ToString();

                // Заполнение списка разделов проекта
                if(!ProjectSections.Contains(projectSection)) {
                    ProjectSections.Add(projectSection);
                }
            }
            // Сортировка
            ProjectSections = new ObservableCollection<string>(ProjectSections.OrderBy(i => i));
        }






        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            if(setting is null) {
                setting = _pluginConfig.AddSettings(_revitRepository.Document);
            }

            setting.ProjectSectionParamName = SelectedProjectSectionParamName;
            setting.SelectedProjectSection = SelectedProjectSection;

            _pluginConfig.SaveProjectConfig();
        }

        private void LoadConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SelectedProjectSectionParamName = setting?.ProjectSectionParamName;
            SelectedProjectSection = setting?.SelectedProjectSection;
        }
    }

}