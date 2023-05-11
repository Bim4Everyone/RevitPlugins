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






        public List<string> ProjectSectionParamNames { get; set; } = new List<string>();

        private string _selectedProjectSectionParamName;
        public string SelectedProjectSectionParamName {
            get => _selectedProjectSectionParamName;
            set {
                this.RaiseAndSetIfChanged(ref _selectedProjectSectionParamName, value);
            }
        }










        public ObservableCollection<string> ProjectSections { get; set; } = new ObservableCollection<string>();     // Список всех комплектов док-ции (обр_ФОП_Раздел проекта)
        private string _selectedProjectSection { get; set; }                                                        // Выбранный пользователем комплект док-ции
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
            SaveConfig();
        }




        public ICommand ShakeCommand { get; }
        private void Shake(object p) {
            Transaction transaction = new Transaction(_revitRepository.Document, "Встряска спецификаций");
            transaction.Start();
            foreach(var item in _revitRepository.AllExistingSheets) {
                ViewSheet viewSheet = item as ViewSheet;
                Parameter param = viewSheet.LookupParameter(SelectedProjectSectionParamName);

                //var paramValue = viewSheet.GetParamValueOrDefault(SelectedProjectSectionParamName);


                //if(viewSheet is null || param is null) { continue; }

                //if(param.AsString() != SelectedProjectSection) {
                //    continue;
                //}

                //SheetUtils sheetUtils = new SheetUtils(this, viewSheet);
                //sheetUtils.FindAndShakeSpecsOnSheet();

            }

            transaction.Commit();
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

                Parameter param = viewSheet.LookupParameter(SelectedProjectSectionParamName);

                if(param is null) {
                    ErrorText = "Данный параметр не найден у листов";
                    return;
                }

                projectSection = param.AsString();

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