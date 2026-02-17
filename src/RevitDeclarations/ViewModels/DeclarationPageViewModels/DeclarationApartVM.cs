using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels.DeclarationPageViewModels;

internal class DeclarationApartVM : DeclarationViewModel {
    private readonly ApartmentsExcelExportVM _excelExportViewModel;
    private readonly ApartmentsCsvExportVM _csvExportViewModel;
    private readonly ApartmentsJsonExportVM _jsonExportViewModel;

    public DeclarationApartVM(RevitRepository revitRepository, ApartmentsSettings settings) 
        : base(revitRepository, settings) {
        _excelExportViewModel =
            new ApartmentsExcelExportVM("Excel", new Guid("01EE33B6-69E1-4364-92FD-A2F94F115A9E"), _settings);
        _csvExportViewModel =
            new ApartmentsCsvExportVM("csv", new Guid("BF1869ED-C5C4-4FCE-9DA9-F8F75A6B190D"), _settings);
        _jsonExportViewModel =
            new ApartmentsJsonExportVM("json", new Guid("159FA27A-06E7-4515-9221-0BAFC0008F21"), _settings);

        _exportFormats = [
            _excelExportViewModel,
            _csvExportViewModel,
            _jsonExportViewModel,
        ];
        _selectedFormat = _exportFormats[0];


        _loadUtp = true;
        _canLoadUtp = true;
    }

    public ApartmentsCsvExportVM CsvExportViewModel => _csvExportViewModel;
}
