using System;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;

using dosymep.Bim4Everyone.SystemParams;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.Value;

namespace RevitClashDetective.Models.Filtration;

internal class LegacyRuleWriter {
    private readonly bool _isSystemParam;
    private readonly BuiltInParameter _paramId;
    private readonly string _paramName;
    private readonly Rule _rule;

    public LegacyRuleWriter(Rule rule) {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));

        _paramName = rule.Provider.Name;
        _isSystemParam = TryGetBuiltInParameter(rule.Provider, out var paramId);
        _paramId = paramId;
    }

    /// <summary>
    /// Конвертирует устаревшее правило <see cref="_rule"/> и добавляет его в фильтр <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">Фильтр</param>
    public void Write(ILogicalFilter filter) {
        if(_rule.Provider is null
           || _rule.Evaluator is null) {
            return;
        }

        if(_rule.Evaluator.Evaluator == RuleEvaluators.FilterHasValue) {
            WriteHasValueRule(filter);
            return;
        }

        if(_rule.Evaluator.Evaluator == RuleEvaluators.FilterHasNoValue) {
            WriteHasNoValueRule(filter);
            return;
        }

        if(_rule.Value is null) {
            return;
        }

        switch(_rule.Value) {
            case IntParamValue intValue:
                WriteIntRule(filter, _rule.Evaluator.Evaluator, intValue.TValue);
                break;
            case DoubleParamValue doubleValue:
                WriteDoubleRule(filter, _rule.Evaluator.Evaluator, doubleValue.TValue);
                break;
            case ElementIdParamValue:
            case StringParamValue:
                var sValue = (ParamValue<string>) _rule.Value;
                WriteStringRule(
                    filter,
                    _rule.Evaluator.Evaluator,
                    sValue.TValue ?? sValue.DisplayValue ?? string.Empty);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private void WriteHasValueRule(ILogicalFilter filter) {
        if(_isSystemParam) {
            filter.AddHasValueRule(_paramId);
        } else {
            filter.AddHasValueRule(_paramName);
        }
    }

    private void WriteHasNoValueRule(ILogicalFilter filter) {
        if(_isSystemParam) {
            filter.AddHasNoValueRule(_paramId);
        } else {
            filter.AddHasNoValueRule(_paramName);
        }
    }

    private void WriteIntRule(ILogicalFilter filter, RuleEvaluators evaluator, int value) {
        switch(evaluator) {
            case RuleEvaluators.FilterNumericEquals:
            case RuleEvaluators.FilterStringEquals:
                if(_isSystemParam) {
                    filter.AddEqualsRule(_paramId, value);
                } else {
                    filter.AddEqualsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNotEquals:
                if(_isSystemParam) {
                    filter.AddNotEqualsRule(_paramId, value);
                } else {
                    filter.AddNotEqualsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericGreater:
            case RuleEvaluators.FilterStringGreater:
                if(_isSystemParam) {
                    filter.AddGreaterRule(_paramId, value);
                } else {
                    filter.AddGreaterRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericGreaterOrEqual:
            case RuleEvaluators.FilterStringGreaterOrEqual:
                if(_isSystemParam) {
                    filter.AddGreaterOrEqualRule(_paramId, value);
                } else {
                    filter.AddGreaterOrEqualRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericLess:
            case RuleEvaluators.FilterStringLess:
                if(_isSystemParam) {
                    filter.AddLessRule(_paramId, value);
                } else {
                    filter.AddLessRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericLessOrEqual:
            case RuleEvaluators.FilterStringLessOrEqual:
                if(_isSystemParam) {
                    filter.AddLessOrEqualRule(_paramId, value);
                } else {
                    filter.AddLessOrEqualRule(_paramName, value);
                }

                break;
            default:
                throw new InvalidOperationException($"Не поддерживаемое условие фильтрации: {evaluator}");
        }
    }

    private void WriteDoubleRule(ILogicalFilter filter, RuleEvaluators evaluator, double value) {
        switch(evaluator) {
            case RuleEvaluators.FilterNumericEquals:
            case RuleEvaluators.FilterStringEquals:
                if(_isSystemParam) {
                    filter.AddEqualsRule(_paramId, value);
                } else {
                    filter.AddEqualsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNotEquals:
                if(_isSystemParam) {
                    filter.AddNotEqualsRule(_paramId, value);
                } else {
                    filter.AddNotEqualsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericGreater:
            case RuleEvaluators.FilterStringGreater:
                if(_isSystemParam) {
                    filter.AddGreaterRule(_paramId, value);
                } else {
                    filter.AddGreaterRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericGreaterOrEqual:
            case RuleEvaluators.FilterStringGreaterOrEqual:
                if(_isSystemParam) {
                    filter.AddGreaterOrEqualRule(_paramId, value);
                } else {
                    filter.AddGreaterOrEqualRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericLess:
            case RuleEvaluators.FilterStringLess:
                if(_isSystemParam) {
                    filter.AddLessRule(_paramId, value);
                } else {
                    filter.AddLessRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNumericLessOrEqual:
            case RuleEvaluators.FilterStringLessOrEqual:
                if(_isSystemParam) {
                    filter.AddLessOrEqualRule(_paramId, value);
                } else {
                    filter.AddLessOrEqualRule(_paramName, value);
                }

                break;
            default:
                throw new InvalidOperationException($"Не поддерживаемое условие фильтрации: {evaluator}");
        }
    }

    private void WriteStringRule(ILogicalFilter filter, RuleEvaluators evaluator, string value) {
        switch(evaluator) {
            case RuleEvaluators.FilterStringEquals:
            case RuleEvaluators.FilterNumericEquals:
                if(_isSystemParam) {
                    filter.AddEqualsRule(_paramId, value);
                } else {
                    filter.AddEqualsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterNotEquals:
                if(_isSystemParam) {
                    filter.AddNotEqualsRule(_paramId, value);
                } else {
                    filter.AddNotEqualsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringGreater:
            case RuleEvaluators.FilterNumericGreater:
                if(_isSystemParam) {
                    filter.AddGreaterRule(_paramId, value);
                } else {
                    filter.AddGreaterRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringGreaterOrEqual:
            case RuleEvaluators.FilterNumericGreaterOrEqual:
                if(_isSystemParam) {
                    filter.AddGreaterOrEqualRule(_paramId, value);
                } else {
                    filter.AddGreaterOrEqualRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringLess:
            case RuleEvaluators.FilterNumericLess:
                if(_isSystemParam) {
                    filter.AddLessRule(_paramId, value);
                } else {
                    filter.AddLessRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringLessOrEqual:
            case RuleEvaluators.FilterNumericLessOrEqual:
                if(_isSystemParam) {
                    filter.AddLessOrEqualRule(_paramId, value);
                } else {
                    filter.AddLessOrEqualRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringContains:
                if(_isSystemParam) {
                    filter.AddContainsRule(_paramId, value);
                } else {
                    filter.AddContainsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringNotContains:
                if(_isSystemParam) {
                    filter.AddNotContainsRule(_paramId, value);
                } else {
                    filter.AddNotContainsRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringBeginsWith:
                if(_isSystemParam) {
                    filter.AddBeginsWithRule(_paramId, value);
                } else {
                    filter.AddBeginsWithRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringNotBeginsWith:
                if(_isSystemParam) {
                    filter.AddNotBeginsWithRule(_paramId, value);
                } else {
                    filter.AddNotBeginsWithRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringEndsWith:
                if(_isSystemParam) {
                    filter.AddEndsWithRule(_paramId, value);
                } else {
                    filter.AddEndsWithRule(_paramName, value);
                }

                break;
            case RuleEvaluators.FilterStringNotEndsWith:
                if(_isSystemParam) {
                    filter.AddNotEndsWithRule(_paramId, value);
                } else {
                    filter.AddNotEndsWithRule(_paramName, value);
                }

                break;
            default:
                throw new InvalidOperationException($"Не поддерживаемое условие фильтрации: {evaluator}");
        }
    }

    /// <summary>
    /// Возвращает системный параметр, если правило задано по системному параметру
    /// либо по рабочему набору.
    /// </summary>
    private bool TryGetBuiltInParameter(
        IFilterableValueProvider provider,
        out BuiltInParameter builtInParameter) {
        if(provider is WorksetValueProvider) {
            builtInParameter = BuiltInParameter.ELEM_PARTITION_PARAM;
            return true;
        }

        if(provider is FilterableValueProviders.ParameterValueProvider {
               RevitParam: SystemParam systemParam
           }) {
            builtInParameter = systemParam.SystemParamId;
            return true;
        }

        builtInParameter = BuiltInParameter.INVALID;
        return false;
    }
}
