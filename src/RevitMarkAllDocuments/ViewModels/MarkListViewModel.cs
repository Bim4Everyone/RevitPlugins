using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.Services;

using static Autodesk.Revit.DB.SpecTypeId;

namespace RevitMarkAllDocuments.ViewModels;

internal class MarkListViewModel : BaseViewModel {
    private readonly Document _document;
    private readonly MarkData _markData;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<MarkedElementViewModel> _markedElements;

    public MarkListViewModel(MarkData markData, 
                             RevitRepository revitRepository,
                             DocumentService documentService,
                             ILocalizationService localizationService) {
        _markData = markData;
        _document = revitRepository.Document;
        _localizationService = localizationService;

        string currentDocName = documentService.GetDocumentFullName(_document);
        var markDataForCurrentDoc = markData.GetDataByDocument(currentDocName);

        _markedElements = [..markDataForCurrentDoc
            .Elements
            .Select(x => new MarkedElementViewModel(x, _document))
            .ToList()];

        MarkElementsCommand = RelayCommand.Create(MarkElements);
    }

    public ICommand MarkElementsCommand { get; }

    public ObservableCollection<MarkedElementViewModel> MarkedElements {
        get => _markedElements;
        set => RaiseAndSetIfChanged(ref _markedElements, value);
    }

    private void MarkElements() {
        string transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");

        using(Transaction t = _document.StartTransaction(transactionName)) {
            foreach(var element in MarkedElements) {
                var mark = element.MarkedElement;
                var revitElement = _document.GetElement(new ElementId(mark.Id));

                var storageType = _markData.MarkRevitParam.StorageType;
                if(storageType == StorageType.String) {
                    revitElement.SetParamValue(_markData.MarkRevitParam, mark.MarkValue);
                } else if(storageType == StorageType.Double) {
                    bool result = double.TryParse(mark.MarkValue, out var number);
                    if(result == true) {
                        revitElement.SetParamValue(_markData.MarkRevitParam, number);
                    }                    
                } else if(storageType == StorageType.Integer) {
                    bool result = int.TryParse(mark.MarkValue, out var number);
                    if(result == true) {
                        revitElement.SetParamValue(_markData.MarkRevitParam, number);
                    }
                }

            }

            t.Commit();
        }
    }
}
