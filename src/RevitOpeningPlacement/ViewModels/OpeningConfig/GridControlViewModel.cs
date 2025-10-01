using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

using RevitClashDetective.Models.Interfaces;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
/// <summary>
/// Динамический Grid для перечисления элементов, попадающих в заданный фильтр. 
/// Использовать для проверки фильтра в настройках расстановки заданий на отверстия в активном файле ВИС
/// </summary>
internal class GridControlViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly IEnumerable<IFilterableValueProvider> _providers;
    private readonly IEnumerable<ElementModel> _elements;

    public GridControlViewModel(RevitRepository revitRepository, Filter filter, IEnumerable<ElementModel> elements) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        if(filter is null) {
            throw new ArgumentNullException(nameof(filter));
        }
        _elements = elements ?? throw new ArgumentNullException(nameof(elements));

        _providers = filter.GetProviders();
        InitializeRows();

        SelectCommand = RelayCommand.Create<ExpandoObject>(SelectElement, CanSelectElement);
    }


    public ObservableCollection<ExpandoObject> Rows { get; set; }
    public ICommand SelectCommand { get; }


    private void InitializeRows() {
        Rows = [];
        foreach(var elementModel in _elements) {
            var element = elementModel.GetElement(_revitRepository.DocInfos);
            IDictionary<string, object> row = new ExpandoObject();
            foreach(var provider in _providers) {
                string value = provider.GetElementParamValue(element)?.DisplayValue;
                if((provider.StorageType == StorageType.Integer || provider.StorageType == StorageType.Double)
                    && double.TryParse(value, out double resultValue)
                    && resultValue != 0) {
                    AddValue(row, provider.Name, resultValue);
                } else if(!string.IsNullOrEmpty(value) && value != "0") {
                    AddValue(row, provider.Name, value);
                }

            }
            row.Add("File", RevitRepository.GetDocumentName(element.Document));
            row.Add("Transform", JsonConvert.SerializeObject(elementModel.TransformModel));
            row.Add("Category", element.Category?.Name);
            row.Add("FamilyName", element.GetTypeId().IsNotNull()
                ? (element.Document.GetElement(element.GetTypeId()) as ElementType)?.FamilyName
                : null);
            row.Add("Name", element.Name);
            row.Add("Id", element.Id.GetIdValue());
            Rows.Add((ExpandoObject) row);
        }
    }

    private void AddValue<T>(IDictionary<string, object> row, string key, T value) {
        if(!row.ContainsKey(key)) {
            row.Add(key, value);
        } else {
            row[key] = value;
        }
    }

    private void SelectElement(ExpandoObject row) {
        ((IDictionary<string, object>) row).TryGetValue("Id", out object resultId);
        ((IDictionary<string, object>) row).TryGetValue("File", out object resultFile);
        ((IDictionary<string, object>) row).TryGetValue("Transform", out object transform);
        if(resultId == null) {
            return;
        }
        if(resultFile != null) {
            var element = GetElement(
                GetElementId(resultId),
                resultFile.ToString(),
                JsonConvert.DeserializeObject<TransformModel>(transform.ToString()));
            if(element != null) {
                _revitRepository.GetClashRevitRepository().SelectAndShowElement(new[] { element });
            }
        }
    }

    private bool CanSelectElement(ExpandoObject row) {
        return row != null;
    }

    private ElementModel GetElement(ElementId id, string documentName, TransformModel transform) {
        var doc = GetDocument(documentName);
        return doc != null && id.IsNotNull() ? new ElementModel(doc.GetElement(id), transform) : null;
    }

    private Document GetDocument(string documentName) {
        return _revitRepository
            .DocInfos
            .Select(item => item.Doc)
            .FirstOrDefault(
                item => RevitRepository
                    .GetDocumentName(item)
                    .Equals(documentName, StringComparison.CurrentCultureIgnoreCase));
    }

    private ElementId GetElementId(object idValue) {
#if REVIT_2023_OR_LESS
        if(idValue is int id) {
            return new ElementId(id);
        }
#else
        if(idValue is long id) {
            return new ElementId(id);
        }
#endif
        return ElementId.InvalidElementId;
    }
}
