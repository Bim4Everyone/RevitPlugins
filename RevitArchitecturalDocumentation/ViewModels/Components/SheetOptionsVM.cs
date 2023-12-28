using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Options;

namespace RevitArchitecturalDocumentation.ViewModels.Components {
    internal class SheetOptionsVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly SheetOptions _sheetOptions;

        private bool _workWithSheets = true;
        private List<FamilySymbol> _titleBlocksInProject;
        private FamilySymbol _selectedTitleBlock;
        private string _selectedTitleBlockName;
        private string _sheetNamePrefix;

        public SheetOptionsVM(PluginConfig pluginConfig, RevitRepository revitRepository, SheetOptions sheetOptions) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _sheetOptions = sheetOptions;

            WorkWithSheets = _sheetOptions.WorkWithSheets;
            SheetNamePrefix = _sheetOptions.SheetNamePrefix;
            TitleBlocksInProject = _revitRepository.TitleBlocksInProject;
        }


        public bool WorkWithSheets {
            get => _workWithSheets;
            set => RaiseAndSetIfChanged(ref _workWithSheets, value);
        }

        public List<FamilySymbol> TitleBlocksInProject {
            get => _titleBlocksInProject;
            set => RaiseAndSetIfChanged(ref _titleBlocksInProject, value);
        }

        public FamilySymbol SelectedTitleBlock {
            get => _selectedTitleBlock;
            set => RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
        }

        public string SelectedTitleBlockName {
            get => _selectedTitleBlockName;
            set => RaiseAndSetIfChanged(ref _selectedTitleBlockName, value);
        }

        public string SheetNamePrefix {
            get => _sheetNamePrefix;
            set => RaiseAndSetIfChanged(ref _sheetNamePrefix, value);
        }


        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        public void LoadConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            WorkWithSheets = settings.WorkWithSheets;
            SheetNamePrefix = settings.SheetNamePrefix;
            SelectedTitleBlockName = settings.SelectedTitleBlockName;

            SelectedTitleBlock = TitleBlocksInProject?.FirstOrDefault(a => a.Name.Equals(SelectedTitleBlockName));
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        public void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.WorkWithSheets = WorkWithSheets;
            settings.SheetNamePrefix = SheetNamePrefix;
            settings.SelectedTitleBlockName = SelectedTitleBlock.Name;

            _pluginConfig.SaveProjectConfig();
        }


        public SheetOptions GetSheetOption() {
            return new SheetOptions() {
                WorkWithSheets = WorkWithSheets,
                SelectedTitleBlock = SelectedTitleBlock,
                SheetNamePrefix = SheetNamePrefix
            };
        }
    }
}
