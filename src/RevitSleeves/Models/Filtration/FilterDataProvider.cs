using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitClashDetective.Models.FilterableValueProviders;

namespace RevitSleeves.Models.Filtration;

/// <summary>
/// Создает <see cref="DataProvider"/> с категорией, параметрами и значениями параметров,
/// доступными для построения фильтра
/// </summary>
internal class FilterDataProvider {
    private readonly Category _category;
    private readonly ICollection<Document> _documents;

    public FilterDataProvider(Category category, ICollection<Document> documents) {
        if(documents is null) {
            throw new ArgumentNullException(nameof(documents));
        }

        if(documents.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(documents));
        }

        _category = category ?? throw new ArgumentNullException(nameof(category));
        _documents = documents;
    }

    public DataProvider CreateDataProvider() {
        return new DataProvider([_category], GetParams, _documents);
    }

    private ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        var doc = _documents.FirstOrDefault();
        if(doc is null) {
            return [];
        }

        if(categories.Count != 1
           || categories.First().GetBuiltInCategory() != _category.GetBuiltInCategory()) {
            return [];
        }

        return ParameterFilterUtilities
            .GetFilterableParametersInCommon(doc, [_category.Id])
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
