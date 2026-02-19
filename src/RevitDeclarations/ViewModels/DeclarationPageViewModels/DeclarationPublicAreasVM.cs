using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;

internal class DeclarationPublicAreasVM : DeclarationViewModel {
    private readonly PublicAreasExcelExportVM _excelExportViewModel;
    private readonly PublicAreasCsvExportVM _csvExportViewModel;

    public DeclarationPublicAreasVM(RevitRepository revitRepository, PublicAreasSettings settings, IMessageBoxService messageBoxService)
        : base(revitRepository, settings, messageBoxService) {
        _excelExportViewModel =
            new PublicAreasExcelExportVM("Excel", new Guid("186F3EEE-303A-42DF-910E-475AD2525ABD"), _settings, messageBoxService);
        _csvExportViewModel =
            new PublicAreasCsvExportVM("csv", new Guid("A674AB16-642A-4642-BE51-51B812378734"), _settings, messageBoxService);

        _exportFormats = [
            _excelExportViewModel,
            _csvExportViewModel
        ];
        _selectedFormat = _exportFormats[0];

        _loadUtp = false;
        _canLoadUtp = false;
    }

    public PublicAreasCsvExportVM CsvExportViewModel => _csvExportViewModel;
}
