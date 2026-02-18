using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;
using RevitDeclarations.ViewModels;

namespace RevitDeclarations.ViewModels;

internal class DeclarationCommercialVM : DeclarationViewModel {
    private readonly CommercialExcelExportVM _excelExportViewModel;
    private readonly CommercialCsvExportVM _csvExportViewModel;

    public DeclarationCommercialVM(RevitRepository revitRepository, CommercialSettings settings)
        : base(revitRepository, settings) {
        _excelExportViewModel =
            new CommercialExcelExportVM("Excel", new Guid("8D69848F-159D-4B26-B4C0-17E3B3A132CC"), _settings);
        _csvExportViewModel =
            new CommercialCsvExportVM("csv", new Guid("EC72C14A-9D4A-4D8B-BD35-50801CE68C24"), _settings);

        _exportFormats = [
            _excelExportViewModel,
            _csvExportViewModel
        ];
        _selectedFormat = _exportFormats[0];

        _loadUtp = false;
        _canLoadUtp = false;
    }
}
