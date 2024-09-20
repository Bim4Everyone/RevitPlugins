using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitValueModifier.Models;

namespace RevitValueModifier.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private string _errorText;
        private string _saveProperty;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        private void LoadView() {

            ICollection<ElementId> selectedIds = _revitRepository.ActiveUIDocument.Selection.GetElementIds();

            if(0 == selectedIds.Count) {
                TaskDialog.Show("Ошибка!", "Не выбрано ни одного элемента");
                return;
            }

            var selectedElems = new List<Element>();
            foreach(ElementId selectedId in selectedIds) {
                selectedElems.Add(_revitRepository.Document.GetElement(selectedId));
            }


            RevitParameterHelper paramHelper = new RevitParameterHelper();
            List<ElementId> intersectedParameterIds = paramHelper.GetIntersectedParameterIds(selectedElems);

            RevitElemHelper elemHelper = new RevitElemHelper(_revitRepository.Document);
            List<RevitElem> revitElems = elemHelper.GetRevitElements(selectedElems, intersectedParameterIds);




            //var element = selectedElems.First();
            //var parameter = element.LookupParameter("ФОП_Блок СМР");

            //var parameterId = parameter.Id;


            //var parameters = element.Parameters.Cast<Parameter>();

            //var sameParameter = parameters.First(p => p.Id == parameterId);
            //var val = sameParameter.AsValueString();



            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
        }






        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
