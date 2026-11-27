using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.CustomParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

internal class ParamService(IRevitParamFactory revitParamFactory, ILoggerService loggerService) : IParamService {
    /// <inheritdoc />
    public bool WriteInstanceValue(FamilyInstance instance, LintelParameterValue value) {
        var parameter = FindMappedParameter(instance, null, value.Name);
        if(parameter is null || parameter.Element.Id != instance.Id || parameter.IsReadOnly
            || !HasExpectedType(parameter.Definition, parameter.StorageType, value.Kind)) {
            return false;
        }
        if(value.Kind == LintelParameterKind.LengthMm && value.LengthMm is double length) {
            instance.SetParamValue(value.Name, UnitUtils.ConvertToInternalUnits(length, UnitTypeId.Millimeters));
        } else if(value.Kind == LintelParameterKind.YesNo && value.YesNo is bool flag) {
            instance.SetParamValue(value.Name, flag ? 1 : 0);
        } else {
            return false;
        }
        return true;
    }

    /// <inheritdoc />
    public (IList<RevitParam> Parameters, IList<RevitParam> InstanceParameters) CreateRevitParams(
        Document document, FamilySymbol symbol, FamilyInstance instance) {
        var typeParams = CreateRevitParams(document, symbol).ToArray();
        var instanceParams = instance is null
            ? GetFamilyInstanceParams(document, symbol.Family)
            : CreateRevitParams(document, instance).ToArray();
        return (typeParams.Union(instanceParams).ToArray(), instanceParams);
    }

    /// <summary>Проверяет, что параметр имеет тип Да/Нет.</summary>
    public static bool IsYesNo(RevitParam parameter) {
        if(parameter is null || parameter.StorageType != StorageType.Integer) {
            return false;
        }

        try {
            return parameter.UnitType == SpecTypeId.Boolean.YesNo;
        } catch(ArgumentOutOfRangeException) {
            // Тип некоторых встроенных параметров определяется только при чтении UnitType.
            // Неизвестный тип нельзя считать параметром «Да/Нет».
            return false;
        }
    }

    /// <inheritdoc />
    public RevitParam GetLintelParam(RevitFamily family, string paramName, StorageType storageType) {
        return family?.OrderedParams?.FirstOrDefault(param =>
            param.Name == paramName && param.StorageType == storageType);
    }

    /// <inheritdoc />
    public RevitParam GetOpeningParam(RevitFamily[] families, string paramName, StorageType storageType) {
        if(families is null || families.Length == 0) {
            return null;
        }
        var parameter = GetLintelParam(families[0], paramName, storageType);
        return parameter is not null && families.All(family => family.OrderedParams.Any(param =>
            param.Name == parameter.Name && param.StorageType == storageType)) ? parameter : null;
    }

    /// <inheritdoc />
    public RevitParam GetFixParam(LintelFamilyConfig lintelConfig,
        OpeningFamilyConfig openingConfig, string paramName) {
        if(lintelConfig?.RevitLintelFamily is null || openingConfig?.RevitOpeningFamily is null) {
            return null;
        }
        return GetCommonFixParams(new[] { lintelConfig.RevitLintelFamily }
                .Concat(openingConfig.RevitOpeningFamily))
            .FirstOrDefault(param => param.Name == paramName);
    }

    /// <inheritdoc />
    public IEnumerable<RevitParam> GetCommonFixParams(IEnumerable<RevitFamily> families) {
        var familyArray = families.ToArray();
        if(familyArray.Length == 0) {
            return [];
        }
        return familyArray[0].InstanceParams.Where(IsYesNo)
            .Where(param => familyArray.All(family => family.InstanceParams.Any(other =>
                other.Equals(param) && IsYesNo(other))))
            .OrderBy(param => param.Name)
            .ToArray();
    }

    /// <inheritdoc />
    public double? ReadLengthMm(Element instance, Element type, string name) {
        var parameter = FindMappedParameter(instance, type, name);
        if(parameter?.HasValue != true || parameter.StorageType != StorageType.Double
            || parameter.Definition.GetDataType() != SpecTypeId.Length) {
            return null;
        }
        double value = parameter.AsDouble();
        return double.IsNaN(value) || double.IsInfinity(value) ? null
            : UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Millimeters);
    }

    /// <inheritdoc />
    public int? ReadInteger(Element instance, Element type, string name) {
        var parameter = FindMappedParameter(instance, type, name);
        return parameter?.HasValue == true && parameter.StorageType == StorageType.Integer
            ? parameter.AsInteger() : null;
    }

    /// <inheritdoc />
    public bool? ReadYesNo(Element instance, string name, bool? unsetValue = null) {
        var parameter = FindMappedParameter(instance, null, name);
        if(parameter is null || parameter.StorageType != StorageType.Integer
            || parameter.Definition.GetDataType() != SpecTypeId.Boolean.YesNo) {
            return null;
        }
        if(!parameter.HasValue) {
            return unsetValue;
        }
        return parameter.AsInteger() switch { 0 => false, 1 => true, _ => null };
    }

    /// <inheritdoc />
    public bool? ReadFixation(Element instance, string name) {
        return ReadYesNo(instance, name, unsetValue: false);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, LintelDefinitionParameter> ReadDefinitionParameters(Document familyDocument,
        IReadOnlyDictionary<string, string> mappings) {
        var familyParameters = familyDocument.FamilyManager.GetParameters().ToLookup(p => p.Definition.Name);
        return LintelParameterSpec.All.ToDictionary(spec => spec.Key, spec => {
            mappings.TryGetValue(spec.Key, out string name);
            var candidates = familyParameters[name].Take(2).ToArray();
            if(candidates.Length != 1) {
                return new LintelDefinitionParameter(candidates.Length == 0 ? LintelParameterState.Missing
                    : LintelParameterState.Ambiguous, false, null);
            }
            var parameter = candidates[0];
            return new LintelDefinitionParameter(GetDefinitionState(parameter, spec), parameter.IsInstance,
                parameter.IsShared ? parameter.GUID : null);
        });
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, LintelParameterFact> ReadLintelParameterFacts(Document project,
        IReadOnlyDictionary<string, LintelDefinitionParameter> familyParameters, Family family,
        FamilyInstance sample, IReadOnlyDictionary<string, string> mappings) {
        var bindings = new List<(InternalDefinition Definition, bool Instance)>();
        using var iterator = project.ParameterBindings.ForwardIterator();
        while(iterator.MoveNext()) {
            if(iterator.Key is InternalDefinition definition && iterator.Current is ElementBinding binding
                && binding.Categories.Contains(family.FamilyCategory)) {
                bindings.Add((definition, binding is InstanceBinding));
            }
        }
        var byName = bindings.ToLookup(item => item.Definition.Name);
        return new ReadOnlyDictionary<string, LintelParameterFact>(LintelParameterSpec.All.ToDictionary(
            spec => spec.Key, spec => {
                mappings.TryGetValue(spec.Key, out string name);
                var projectParameters = byName[name].Take(2).ToArray();
                var state = InspectParameter(project, sample, spec, name, familyParameters[spec.Key], projectParameters);
                return new LintelParameterFact(name, state);
            }));
    }

    // Учитывает параметры определения и отдельные проектные привязки с тем же именем.
    private static LintelParameterState InspectParameter(Document project, FamilyInstance sample,
        LintelParameterSpec spec, string name, LintelDefinitionParameter parameter,
        (InternalDefinition Definition, bool Instance)[] projectParameters) {
        if(parameter.State == LintelParameterState.Ambiguous || projectParameters.Length > 1) {
            return LintelParameterState.Ambiguous;
        }
        if(projectParameters.Length == 1) {
            var (definition, instance) = projectParameters[0];
            if(parameter.State == LintelParameterState.Missing) {
                return instance ? InspectProjectInstance(sample, spec, name) : LintelParameterState.Type;
            }
            if(!parameter.SharedId.HasValue || project.GetElement(definition.Id) is not SharedParameterElement shared
                || shared.GuidValue != parameter.SharedId || instance != parameter.Instance) {
                return LintelParameterState.Ambiguous;
            }
        }
        return parameter.State;
    }

    // У параметров типа, управляемых формулой и отчётных параметров разные причины недоступности.
    private static LintelParameterState GetDefinitionState(FamilyParameter parameter, LintelParameterSpec spec) {
        if(!parameter.IsInstance) {
            return LintelParameterState.Type;
        }
        if(!HasExpectedType(parameter.Definition, parameter.StorageType, spec.Kind)) {
            return LintelParameterState.Incompatible;
        }
        return parameter.IsReadOnly || parameter.IsReporting || parameter.IsDeterminedByFormula
            ? LintelParameterState.ReadOnly : LintelParameterState.Writable;
    }

    // У проектной привязки доступность записи для экземпляра проверяется только на фактическом образце.
    private static LintelParameterState InspectProjectInstance(FamilyInstance sample, LintelParameterSpec spec,
        string name) {
        if(sample is null) {
            return LintelParameterState.Unavailable;
        }
        if(sample.GetParameters(name).Count > 1) {
            return LintelParameterState.Ambiguous;
        }
        var parameter = FindMappedParameter(sample, null, name);
        if(parameter is null) {
            return LintelParameterState.Missing;
        }
        if(!HasExpectedType(parameter.Definition, parameter.StorageType, spec.Kind)) {
            return LintelParameterState.Incompatible;
        }
        return parameter.IsReadOnly ? LintelParameterState.ReadOnly : LintelParameterState.Writable;
    }

    // Для линейного параметра требуется Length, для флага — именно Yes/No, а не произвольное целое.
    private static bool HasExpectedType(Definition definition, StorageType storage, LintelParameterKind kind) {
        return kind == LintelParameterKind.LengthMm
            ? storage == StorageType.Double && definition.GetDataType() == SpecTypeId.Length
            : storage == StorageType.Integer && definition.GetDataType() == SpecTypeId.Boolean.YesNo;
    }

    // Неоднозначное имя не разрешается произвольно; тип используется только при отсутствии параметра экземпляра.
    private static Parameter FindMappedParameter(Element instance, Element type, string name) {
        if(string.IsNullOrEmpty(name)) {
            return null;
        }
        int instanceMatches = instance.GetParameters(name).Count;
        if(instanceMatches > 0) {
            return instanceMatches == 1 ? instance.GetParam(name) : null;
        }
        return type?.GetParameters(name).Count == 1 ? type.GetParam(name) : null;
    }

    // Создаёт каталог параметров элемента.
    private IEnumerable<RevitParam> CreateRevitParams(Document document, Element element) {
        return element.Parameters.Cast<Parameter>()
            .Select(parameter => TryCreateRevitParam(document, parameter.Id)
                ?? CreateCustomParam(parameter.Definition, parameter.StorageType))
            .ToArray();
    }

    // Создаёт параметр фабрикой Dosymep, если тип поддерживается.
    private RevitParam TryCreateRevitParam(Document document, ElementId parameterId) {
        if(!revitParamFactory.CanCreate(document, parameterId)) {
            return null;
        }
        try {
            return revitParamFactory.Create(document, parameterId);
        } catch(ArgumentOutOfRangeException) {
            return null;
        }
    }

    // Получает параметры экземпляров семейства и проектные привязки.
    private RevitParam[] GetFamilyInstanceParams(Document document, Family family) {
        var parameters = new List<RevitParam>();
        Document familyDocument = null;
        try {
            if(family.IsEditable) {
                familyDocument = document.EditFamily(family);
                parameters.AddRange(familyDocument.FamilyManager.GetParameters()
                    .Where(parameter => parameter.IsInstance)
                    .Select(parameter => CreateFamilyParam(document, parameter)));
            }
        } catch(Exception ex) {
            loggerService.Warning(ex, family.Name);
        } finally {
            familyDocument?.Close(false);
        }

        var bindings = document.ParameterBindings.ForwardIterator();
        while(bindings.MoveNext()) {
            if(bindings.Current is InstanceBinding binding && binding.Categories.Contains(family.FamilyCategory)
                && bindings.Key is InternalDefinition definition) {
                var parameter = TryCreateRevitParam(document, definition.Id);
                if(parameter is not null) {
                    parameters.Add(parameter);
                }
            }
        }
        return parameters.Distinct().ToArray();
    }

    // Разрешает общий параметр или создаёт описание параметра семейства.
    private RevitParam CreateFamilyParam(Document document, FamilyParameter parameter) {
        if(parameter.IsShared) {
            var sharedParameter = SharedParameterElement.Lookup(document, parameter.GUID);
            if(sharedParameter is not null) {
                var result = TryCreateRevitParam(document, sharedParameter.Id);
                if(result is not null) {
                    return result;
                }
            }
        }
        return CreateCustomParam(parameter.Definition, parameter.StorageType);
    }

    // Создаёт описание пользовательского параметра без записи в модель.
    private static RevitParam CreateCustomParam(Definition definition, StorageType storageType) {
        return new CustomParam(definition.Name) {
            Name = definition.Name,
            StorageType = storageType,
            UnitType = definition.GetDataType()
        };
    }
}
