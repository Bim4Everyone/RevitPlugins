using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.Models;
internal class StringParamSetService {
    private readonly string _keyWord = "Formula";

    public void SetAll(object instance, IEnumerable<PluginParamVM> sheetSetParams) {
        // Отбираем свойства, которые string, имеют в имени "Formula" и запускаем обновления значения у всех
        instance.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite && p.Name.Contains(_keyWord))
            .ToList()
            .ForEach(p => Set(instance, p.Name, sheetSetParams));
    }

    public void SetAll(object instance, IEnumerable<PluginParamVM> sheetSetParams, StringParamVM stringParam) {
        // Отбираем свойства, которые string, имеют в имени "Formula" и запускаем обновления значения у всех
        instance.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite && p.Name.Contains(_keyWord))
            .Where(p => p.GetValue(instance) is string formula && formula.Contains($"{{{stringParam.ParamName}}}"))
            .ToList()
            .ForEach(p => Set(instance, p.Name, sheetSetParams));
    }

    public void Set(object instance, string formulaPropertyName, IEnumerable<PluginParamVM> sheetSetParams) {
        var propFormula = instance.GetType().GetProperty(formulaPropertyName);
        var prop = instance.GetType().GetProperty(formulaPropertyName.Replace(_keyWord, ""));
        if(prop is null) {
            return;
        }

        string propFormulaValue = propFormula.GetValue(instance) as string;
        prop.SetValue(instance, SetValue(propFormulaValue, sheetSetParams));
    }


    public string SetValue(string formula, IEnumerable<PluginParamVM> sheetSetParams) {
        // префикс_{ФОП_Блок СМР}_суффикс1_{ФОП_Секция СМР}_суффикс2
        string tempValue = formula;

        var regex = new Regex(@"{([^\}]+)}");
        MatchCollection matches = regex.Matches(formula);

        Regex regexForParam;
        foreach(Match match in matches) {
            string paramName = match.Value.Replace("{", "").Replace("}", "");

            if(sheetSetParams.FirstOrDefault(p => p.ParamName == paramName) is not StringParamVM param) {
                continue;
            }
            regexForParam = new Regex(match.Value);
            tempValue = regexForParam.Replace(tempValue, param.StringValue, 1);
        }
        return tempValue;
    }
}
