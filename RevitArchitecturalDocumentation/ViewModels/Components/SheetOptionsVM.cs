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

        private bool _workWithSheets;
        private List<FamilySymbol> _titleBlocksInProject;
        private FamilySymbol _selectedTitleBlock;
        private string _sheetNamePrefix;

        public SheetOptionsVM(PluginConfig pluginConfig, RevitRepository revitRepository, SheetOptions sheetOptions) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _sheetOptions = sheetOptions;

            WorkWithSheets = _sheetOptions.WorkWithSheets;
            SheetNamePrefix = _sheetOptions.SheetNamePrefix;
            TitleBlocksInProject = _revitRepository.TitleBlocksInProject;
            SelectedTitleBlock = TitleBlocksInProject?.FirstOrDefault(a => a.Name.Equals(_sheetOptions.SelectedTitleBlockName));
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

        public string SheetNamePrefix {
            get => _sheetNamePrefix;
            set => RaiseAndSetIfChanged(ref _sheetNamePrefix, value);
        }


        public SheetOptions GetSheetOption() {

            SheetOptions sheetOptions = new SheetOptions(_pluginConfig, _revitRepository) {
                WorkWithSheets = WorkWithSheets,
                SelectedTitleBlock = SelectedTitleBlock,
                SelectedTitleBlockName = SelectedTitleBlock.Name,
                SheetNamePrefix = SheetNamePrefix
            };
            sheetOptions.SaveConfig();

            return sheetOptions;
        }
    }
}
