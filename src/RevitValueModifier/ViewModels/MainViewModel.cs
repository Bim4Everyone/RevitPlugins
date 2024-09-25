using System;
using System.Collections.Generic;
using System.Windows.Input;

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
        private string _taskForWrite;
        private RevitElemUtils _elemHelper;
        private TaskParser _parserForTask;
        private List<RevitParameter> _intersectedParameters;
        private List<RevitParameter> _intersectedParametersNotReadOnly;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

            TaskForWriteChangedCommand = RelayCommand.Create(TaskForWriteChanged);

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand TaskForWriteChangedCommand { get; }
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string TaskForWrite {
            get => _taskForWrite;
            set => this.RaiseAndSetIfChanged(ref _taskForWrite, value);
        }

        public RevitElemUtils ElemHelper {
            get => _elemHelper;
            set => this.RaiseAndSetIfChanged(ref _elemHelper, value);
        }

        public List<RevitParameter> IntersectedParameters {
            get => _intersectedParameters;
            set => this.RaiseAndSetIfChanged(ref _intersectedParameters, value);
        }


        public List<RevitParameter> IntersectedParametersNotReadOnly {
            get => _intersectedParametersNotReadOnly;
            set => this.RaiseAndSetIfChanged(ref _intersectedParametersNotReadOnly, value);
        }

        public TaskParser ParserForTask {
            get => _parserForTask;
            set => this.RaiseAndSetIfChanged(ref _parserForTask, value);
        }

        private void LoadView() {
            try {
                // Создаем объект RevitElemUtils и передаем элементы, с которыми в дальнейшем будем работать
                ElemHelper = new RevitElemUtils(_revitRepository.SelectedElements());
                // Оборачиваем переданные элементы в RevitElem
                ElemHelper.GetRevitElems();
                // Получаем параметры каждого элемента и сохраняем в RevitElem
                ElemHelper.GetElemParameters();

                // Создаем объект RevitParameterUtils и передаем список RevitElem, с которыми в дальнейшем будем работать
                var paramHelper = new RevitParameterUtils(ElemHelper.RevitElems);
                // Получаем список пересеченных параметров RevitParameter (имеются одновременно у всех элементов)
                IntersectedParameters = paramHelper.GetIntersectedParameters();
                // Получаем список RevitParameter, которые доступные не только для чтения
                IntersectedParametersNotReadOnly = paramHelper.GetNotReadOnlyParameters(IntersectedParameters);

                ParserForTask = new TaskParser();
            } catch(Exception e) {
                TaskDialog.Show("Ошибка!", e.Message);
            }

            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
        }



        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(TaskForWrite)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            TaskForWrite = setting?.TaskForWrite ?? _localizationService.GetLocalizedString("MainWindow.Hello");
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.TaskForWrite = TaskForWrite;
            _pluginConfig.SaveProjectConfig();
        }


        private void TaskForWriteChanged() {
            TaskDialog.Show("TaskForWrite", TaskForWrite);
            ParserForTask.ParseTask(TaskForWrite);
        }
    }
}
