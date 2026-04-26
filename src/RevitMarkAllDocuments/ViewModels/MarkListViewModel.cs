using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.Revit.Comparators;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class MarkListViewModel : BaseViewModel {
    private readonly Document _document;
    private readonly RevitParam _markParam;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<MarkedElementViewModel> _markedElements = [];
    private string _errorText;

    public MarkListViewModel(MarkDataByDocument markDataForCurrentDoc,
                             RevitParam markParam,
                             RevitRepository revitRepository,
                             ILocalizationService localizationService) {
        _markParam = markParam;
        _document = revitRepository.Document;
        _localizationService = localizationService;

        if(markDataForCurrentDoc != null) {
            _markedElements = [..markDataForCurrentDoc
                .Elements
                .Select(x => new MarkedElementViewModel(x, _document, localizationService))
                .OrderBy(x => x.MarkValue, new LogicalStringComparer())
                .ToList()];
        }

        MarkElementsCommand = RelayCommand.Create(MarkElements, CanMarkElements);
    }

    public ICommand MarkElementsCommand { get; }

    public string ParameterName => _markParam.Name;

    public ObservableCollection<MarkedElementViewModel> MarkedElements {
        get => _markedElements;
        set => RaiseAndSetIfChanged(ref _markedElements, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void MarkElements() {
        var storageType = _markParam.StorageType;

        string transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
        using(Transaction t = _document.StartTransaction(transactionName)) {
            foreach(var element in MarkedElements) {
                var mark = element.MarkedElement;
                var revitElement = _document.GetElement(new ElementId(mark.Id));

                if(storageType == StorageType.String) {
                    revitElement.SetParamValue(_markParam, mark.MarkValue);
                } else if(storageType == StorageType.Double) {
                    bool result = double.TryParse(mark.MarkValue, out double number);
                    if(result) {
                        revitElement.SetParamValue(_markParam, number);
                    }
                } else if(storageType == StorageType.Integer) {
                    bool result = int.TryParse(mark.MarkValue, out int number);
                    if(result) {
                        revitElement.SetParamValue(_markParam, number);
                    }
                }
            }

            t.Commit();
        }
    }

    private bool CanMarkElements() {
        if(_markedElements.Any(x => x.HasError)) {
            ErrorText = _localizationService.GetLocalizedString("MarkList.ErrorText");
            return false;
        }

        ErrorText = null;
        return true;
    }
}
