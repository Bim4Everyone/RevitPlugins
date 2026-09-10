using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.FilterableValueProviders;

namespace RevitOpeningPlacement.Models.Filtration;

/// <summary>
/// Создает <see cref="DataProvider"/> с категориями, параметрами и значениями параметров,
/// доступными для построения фильтра
/// </summary>
internal class FilterDataProvider {
    private readonly ICollection<Category> _categories;
    private readonly ICollection<Document> _documents;

    /// <summary>
    /// Конструктор класса, создающего <see cref="DataProvider"/>
    /// </summary>
    /// <param name="categories">Категории, по которым можно фильтровать элементы</param>
    /// <param name="documents">Документы, из которых берутся значения параметров</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Исключение, если коллекции пустые</exception>
    public FilterDataProvider(ICollection<Category> categories, ICollection<Document> documents) {
        if(categories is null) {
            throw new ArgumentNullException(nameof(categories));
        }
        if(documents is null) {
            throw new ArgumentNullException(nameof(documents));
        }
        if(categories.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(categories));
        }
        if(documents.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(documents));
        }

        _categories = categories;
        _documents = documents;
    }


    public DataProvider CreateDataProvider() {
        return new DataProvider(_categories, GetParams, _documents);
    }


    private ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        var doc = _documents.First();
        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(doc, [.. _categories.Select(category => category.Id)])
            .Select(paramId => GetFilterableParam(doc, paramId))
            .Where(param => param != null)
            .ToArray();
    }

    private RevitParam GetFilterableParam(Document doc, ElementId paramId) {
        try {
            return ParameterInitializer.InitializeParameter(doc, paramId);
        } catch(ArgumentException) {
            return null;
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            return null;
        }
    }
}
