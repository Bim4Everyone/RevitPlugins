using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.FilterableValueProviders;

namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Создает <see cref="DataProvider"/> с категориями, параметрами и значениями параметров,
/// доступными для поисковых наборов плагина.
/// </summary>
internal class FilterDataProvider {
    private readonly RevitRepository _revitRepository;

    public FilterDataProvider(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }

    /// <summary>
    /// Значения параметров берутся из экземпляров элементов активного документа и всех связей
    /// </summary>
    public DataProvider CreateDataProvider() {
        return new DataProvider(
            _revitRepository.GetCategories(),
            GetParams,
            [.. _revitRepository.DocInfos.Select(item => item.Doc)]);
    }

    /// <summary>
    /// Возвращает параметры, доступные для фильтрации заданных категорий.
    /// Параметр считается доступным, если он есть во всех заданных категориях, кроме, возможно, одной,
    /// - логика перенесена из RevitRepository.GetParameters.
    /// </summary>
    private ICollection<RevitParam> GetParams(ICollection<Category> categories) {
        var doc = _revitRepository.Doc;
        return categories
            .SelectMany(category => ParameterFilterUtilities
                .GetFilterableParametersInCommon(doc, [category.Id])
                .Select(paramId => GetFilterableParam(doc, paramId))
                .Where(param => param != null))
            .GroupBy(param => param.Name)
            .Where(group => group.Count() > categories.Count - 1)
            .SelectMany(group => group
                .GroupBy(param => param.Id)
                .Select(sameIdParams => sameIdParams.First()))
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
