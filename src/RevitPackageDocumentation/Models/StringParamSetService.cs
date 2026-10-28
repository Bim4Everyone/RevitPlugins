using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Пересчет свойств по формулам. У объекта для свойства-формулы XxxFormula хранится результат в свойстве Xxx.
/// В формуле ссылки на текстовые параметры комплекта записываются в фигурных скобках:
/// префикс_{ФОП_Блок СМР}_суффикс1_{ФОП_Секция СМР}_суффикс2
/// </summary>
internal class StringParamSetService {
    private const string _keyWord = "Formula";

    // Ссылка на параметр комплекта: {Имя параметра}
    private static readonly Regex _paramReferenceRegex = new(@"{([^\}]+)}", RegexOptions.Compiled);

    /// <summary>
    /// Пересчитывает все свойства-формулы объекта
    /// </summary>
    public void SetAll(object instance, IEnumerable<PluginParamVM> sheetSetParams) {
        foreach(var propertyName in GetFormulaPropertyNames(instance)) {
            Set(instance, propertyName, sheetSetParams);
        }
    }

    /// <summary>
    /// Пересчитывает свойства-формулы объекта, которые ссылаются на указанный параметр
    /// </summary>
    public void SetAll(object instance, IEnumerable<PluginParamVM> sheetSetParams, StringParamVM stringParam) {
        string paramReference = $"{{{stringParam.ParamName}}}";
        foreach(var propertyName in GetFormulaPropertyNames(instance)
            .Where(name => instance.GetType().GetProperty(name).GetValue(instance) is string formula
                && formula.Contains(paramReference))) {
            Set(instance, propertyName, sheetSetParams);
        }
    }

    /// <summary>
    /// Пересчитывает одно свойство по формуле: результат записывается в свойство с именем без "Formula"
    /// </summary>
    /// <param name="formulaPropertyName">Имя свойства формулы, например ViewNameFormula</param>
    public void Set(object instance, string formulaPropertyName, IEnumerable<PluginParamVM> sheetSetParams) {
        var propFormula = instance.GetType().GetProperty(formulaPropertyName);
        var prop = instance.GetType().GetProperty(formulaPropertyName.Replace(_keyWord, ""));
        if(propFormula is null || prop is null) {
            return;
        }

        string propFormulaValue = propFormula.GetValue(instance) as string;
        prop.SetValue(instance, ResolveFormula(propFormulaValue, sheetSetParams));
    }

    /// <summary>
    /// Имена строковых записываемых свойств, в имени которых есть "Formula".
    /// Список материализуется, так как пересчет меняет свойства объекта
    /// </summary>
    private static List<string> GetFormulaPropertyNames(object instance) {
        return instance.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite && p.Name.Contains(_keyWord))
            .Select(p => p.Name)
            .ToList();
    }

    /// <summary>
    /// Подставляет в формулу значения текстовых параметров комплекта. Ссылки на несуществующие параметры
    /// и на параметры выбора элемента остаются в тексте без изменений
    /// </summary>
    private static string ResolveFormula(string formula, IEnumerable<PluginParamVM> sheetSetParams) {
        if(string.IsNullOrEmpty(formula)) {
            return formula;
        }

        return _paramReferenceRegex.Replace(formula, match => {
            string paramName = match.Groups[1].Value;
            return sheetSetParams.FirstOrDefault(p => p.ParamName == paramName) is StringParamVM param
                ? param.StringValue
                : match.Value;
        });
    }
}
