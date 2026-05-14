using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitParamsChecker.Exceptions;
using RevitParamsChecker.Models.Rules.ComparisonOperators;

namespace RevitParamsChecker.Models.Rules;

internal class ParameterRule : ValidationRule {
    public ParameterRule() {
    }

    public string ParameterName { get; set; }

    public string ExpectedValue { get; set; }

    public ComparisonOperator Operator { get; set; } = new EqualsOperator();

    public override bool Evaluate(Element element) {
        if(string.IsNullOrWhiteSpace(ParameterName)) {
            throw new InvalidOperationException($"Перед вызовом метода необходимо назначить {nameof(ParameterName)}");
        }

        if(ExpectedValue is null) {
            throw new InvalidOperationException($"Перед вызовом метода необходимо назначить {nameof(ExpectedValue)}");
        }

        if(Operator is null) {
            throw new InvalidOperationException($"Перед вызовом метода необходимо назначить {nameof(Operator)}");
        }

        if(!element.IsExistsParam(ParameterName)) {
            throw new ParamNotFoundException(ParameterName);
        }

        var parameter = element.GetParam(ParameterName);
        string actualValue = GetParamActualValue(parameter);

        return Operator.Evaluate(actualValue, ExpectedValue);
    }

    public override ValidationRule Copy() {
        return new ParameterRule() {
            ParameterName = ParameterName,
            ExpectedValue = ExpectedValue,
            Operator = Operator.Copy()
        };
    }

    private string GetParamActualValue(Parameter parameter) {
        if(parameter.StorageType == StorageType.Double) {
            // у double параметров в конце могут быть единицы измерения, например:
            // Площадь = "84 м²"
            return parameter.AsValueString()?.Split(' ').FirstOrDefault() ?? string.Empty;
        } else {
            return parameter.AsValueString() ?? string.Empty;
        }
    }
}
