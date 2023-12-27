using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;

namespace RevitArchitecturalDocumentation.ViewModels {
    internal class SheetOptionsVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly SheetOptions _sheetOptions;

        private List<FamilySymbol> _titleBlocksInProject;
        private FamilySymbol _selectedTitleBlock;
        private string _selectedTitleBlockName;
        private string _sheetNamePrefix;
        private bool _workWithSheets;

        public SheetOptionsVM(PluginConfig pluginConfig, RevitRepository revitRepository, SheetOptions sheetOptions) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _sheetOptions = sheetOptions;

            WorkWithSheets = _sheetOptions.WorkWithSheets;                             // Используется в интерфейсе и работе                                    || Берется из выбора пользователя
            SheetNamePrefix = _sheetOptions.SheetNamePrefix;                           // Используется в интерфейсе и работе                                    || Берется из выбора пользователя
            SelectedTitleBlockName = _sheetOptions.SelectedTitleBlockName;             // Используется опосредованно только в интерфейсе для сохранения имени   || Берется из выбора пользователя



            LoadConfig();


            TitleBlocksInProject = _revitRepository.TitleBlocksInProject;                                           // Используется только в интерфейсе для заполнения комбобокса
            SelectedTitleBlock = TitleBlocksInProject.FirstOrDefault(a => a.Name.Equals(SelectedTitleBlockName));   // Используется в интерфейсе и работе
        }


        public bool WorkWithSheets {
            get => _workWithSheets;
            set => this.RaiseAndSetIfChanged(ref _workWithSheets, value);
        }


        public List<FamilySymbol> TitleBlocksInProject {
            get => _titleBlocksInProject;
            set => this.RaiseAndSetIfChanged(ref _titleBlocksInProject, value);
        }

        public FamilySymbol SelectedTitleBlock {
            get => _selectedTitleBlock;
            set => this.RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
        }

        public string SelectedTitleBlockName {
            get => _selectedTitleBlockName;
            set => this.RaiseAndSetIfChanged(ref _selectedTitleBlockName, value);
        }

        public string SheetNamePrefix {
            get => _sheetNamePrefix;
            set => this.RaiseAndSetIfChanged(ref _sheetNamePrefix, value);
        }


        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        private void LoadConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            WorkWithSheets = settings.WorkWithSheets;
            SheetNamePrefix = settings.SheetNamePrefix;
            SelectedTitleBlockName = settings.SelectedTitleBlockName;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        private void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.WorkWithSheets = WorkWithSheets;
            settings.SheetNamePrefix = SheetNamePrefix;
            settings.SelectedTitleBlockName = SelectedTitleBlock.Name;

            _pluginConfig.SaveProjectConfig();
        }
    }
}
