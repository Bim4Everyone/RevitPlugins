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

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class GridControlViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<IFilterableValueProvider> _providers;
        private readonly IEnumerable<ElementModel> _elements;
        private int _elementsCount;

        public GridControlViewModel(RevitRepository revitRepository, Filter filter, IEnumerable<ElementModel> elements) {
            if(filter is null) { throw new ArgumentNullException(nameof(filter)); }
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));
            _providers = filter.GetProviders();
            InitializeColumns();
            InitializeRows();
            AddCommonInfo();

            SelectCommand = RelayCommand.Create<ExpandoObject>(SelectElement, CanSelectElement);
        }

        public ObservableCollection<ColumnViewModel> Columns { get; set; }
        public ObservableCollection<ExpandoObject> Rows { get; set; }

        public ICommand SelectCommand { get; }
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand RenewCountCommand { get; }
        public int ElementsCount {
            get => _elementsCount;
            set => RaiseAndSetIfChanged(ref _elementsCount, value);
        }

        private void InitializeColumns() {
            Columns = new ObservableCollection<ColumnViewModel>(
                _providers.Select(item => new ColumnViewModel() {
                    FieldName = item.Name,
                    Header = $"Параметр: {item.DisplayValue}"
                })
                .GroupBy(item => item.FieldName)
                .Select(item => item.First()));
        }

        private void InitializeRows() {
            Rows = new ObservableCollection<ExpandoObject>();
            foreach(var elementModel in _elements) {
                var element = elementModel.GetElement(_revitRepository.DocInfos);
                if(element is null) {
                    continue;
                }
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
                row.Add("FamilyName", element.GetTypeId().IsNotNull() ? (element.Document.GetElement(element.GetTypeId()) as ElementType)?.FamilyName : null);
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

        private void AddCommonInfo() {
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Name", Header = "Имя типоразмера" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "FamilyName", Header = "Имя семейства" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Category", Header = "Категория" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Id", Header = "Id" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "File", Header = "Файл" });
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
                    _revitRepository.SelectAndShowElement(new[] { element });
                }
            }
        }

        private bool CanSelectElement(ExpandoObject row) {
            return row != null;
        }

        private ElementModel GetElement(ElementId id, string documentName, TransformModel transform) {
            var doc = GetDocument(documentName);
            if(doc != null && id.IsNotNull()) {
                return new ElementModel(doc.GetElement(id), transform);
            }
            return null;
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
}
