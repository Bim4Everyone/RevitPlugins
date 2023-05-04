using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;

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


        private string _projectSection = "обр_ФОП_Раздел проекта";
        public string PROJECT_SECTION {
            get => _projectSection;
            set {
                _projectSection = value;
                GetProjectSections();
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

            GetProjectSections();
            
            RegenerateCommand = new RelayCommand(Regenerate, CanShake);
            ShakeCommand = new RelayCommand(Shake, CanShake);

        }








        public ICommand RegenerateCommand { get; }
        private void Regenerate(object p) {
            Transaction transaction = new Transaction(_revitRepository.Document, "Встряска спецификаций");
            transaction.Start();
            _revitRepository.Document.Regenerate();
            transaction.Commit();
        }






        public ICommand ShakeCommand { get; }
        private void Shake(object p) {
            Transaction transaction = new Transaction(_revitRepository.Document, "Встряска спецификаций");
            transaction.Start();
            foreach(var item in _revitRepository.AllExistingSheets) {
                ViewSheet viewSheet = item as ViewSheet;
                Parameter param = viewSheet.LookupParameter(PROJECT_SECTION);
                
                if(viewSheet is null || param is null) { continue; }

                if(param.AsString() != SelectedProjectSection) {
                    continue;
                }

                SheetUtils sheetUtils = new SheetUtils(this, viewSheet);
                sheetUtils.FindAndShakeSpecsOnSheet();

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



        private void GetProjectSections() {
            ProjectSections.Clear();
            ErrorText = string.Empty;
            string projectSection;
            foreach(var item in _revitRepository.AllExistingSheets) {
                ViewSheet viewSheet = item as ViewSheet;
                if (viewSheet is null) { continue; }

                Parameter param = viewSheet.LookupParameter(PROJECT_SECTION);

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
        }
    }
}