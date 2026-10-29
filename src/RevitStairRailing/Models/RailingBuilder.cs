using System.Globalization;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

namespace RevitStairRailing.Models;

/// <summary>
/// Создание элементов ограждения в модели и сборки из них.
/// </summary>
internal class RailingBuilder {
    private const double MillimetersToFeet = 1 / 304.8;

    /// <summary>
    /// Допуск при выборе уровня под элементом и сравнении размеров пластин, футы (0,01 мм).
    /// </summary>
    private const double SizeTolerance = 0.01 / 304.8;

    /// <summary>
    /// Параметры типа семейства пластины: толщина и ширина сечения.
    /// </summary>
    public const string PlateThicknessParameter = "t";
    public const string PlateWidthParameter = "h";
    private const string PlateNameParameter = "ADSK_Наименование";

    /// <summary>
    /// Значение «Привязка по Z» — «По центру».
    /// </summary>
    private const int CenterZJustification = 1;

    private readonly Document _document;
    private readonly Dictionary<string, FamilySymbol> _plateTypes = new();

    public RailingBuilder(Document document) {
        _document = document;
    }

    /// <summary>
    /// Создаёт элементы ограждения и (по параметрам) сборку.
    /// </summary>
    public RailingResult Build(List<RailingMember> members, RailingParameters parameters, string transactionName) {
        var typeIds = new Dictionary<RailingMemberRole, ElementId> {
            [RailingMemberRole.Post] = parameters.PostTypeId,
            [RailingMemberRole.Handrail] = parameters.HandrailTypeId,
            [RailingMemberRole.Rod] = parameters.RodTypeId,
            [RailingMemberRole.Anchor] = parameters.AnchorTypeId
        };
        List<Level> levels = new FilteredElementCollector(_document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(level => level.ProjectElevation)
            .ToList();
        if(levels.Count == 0) {
            throw new RailingException("Build.NoLevels");
        }

        using var transactionGroup = new TransactionGroup(_document, transactionName);
        transactionGroup.Start();

        var createdIds = new List<ElementId>();
        using(Transaction transaction = _document.StartTransaction(transactionName)) {
            foreach(RailingMember member in members) {
                FamilySymbol symbol = member.PlateSize != null
                    ? GetPlateType(parameters.PlateFamilyId, member.PlateSize)
                    : (FamilySymbol) _document.GetElement(typeIds[member.Role]);
                if(!symbol.IsActive) {
                    symbol.Activate();
                }

                FamilyInstance instance = member.Line != null
                    ? CreateLinear(member, symbol, levels)
                    : _document.Create.NewFamilyInstance(
                        member.Point,
                        symbol,
                        GetLevelBelow(levels, member.Point.Z),
                        StructuralType.NonStructural);
                createdIds.Add(instance.Id);
            }

            transaction.Commit();
        }

        AssemblyInstance assembly = null;
        if(parameters.CreateAssembly && createdIds.Count > 0) {
            assembly = CreateAssembly(createdIds, parameters.AssemblyNamePrefix, transactionName);
        }

        transactionGroup.Assimilate();
        return new RailingResult(createdIds, assembly);
    }

    private FamilyInstance CreateLinear(RailingMember member, FamilySymbol symbol, List<Level> levels) {
        Level level = GetLevelBelow(levels, Math.Min(member.Line.GetEndPoint(0).Z, member.Line.GetEndPoint(1).Z));
        FamilyInstance instance = _document.Create.NewFamilyInstance(member.Line, symbol, level, StructuralType.Beam);

        // профили ограждения не подрезаются соседними балками
        StructuralFramingUtils.DisallowJoinAtEnd(instance, 0);
        StructuralFramingUtils.DisallowJoinAtEnd(instance, 1);
        if(member.BendAngle != null) {
            instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE)?.Set(member.BendAngle.Value);
        }

        if(member.PlateSize != null) {
            instance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION)?.Set(CenterZJustification);
        }

        return instance;
    }

    /// <summary>
    /// Ближайший уровень не выше точки; ниже всех уровней — самый нижний.
    /// </summary>
    private static Level GetLevelBelow(List<Level> levels, double elevation) {
        return levels.LastOrDefault(level => level.ProjectElevation <= elevation + SizeTolerance) ?? levels[0];
    }

    /// <summary>
    /// Типоразмер пластины заданной толщины и ширины: существующий или копия первого типоразмера семейства
    /// с именем «-tхh».
    /// </summary>
    private FamilySymbol GetPlateType(ElementId familyId, PlateSize size) {
        string name = "-" + FormatSize(size.Thickness) + "х" + FormatSize(size.Width);
        if(_plateTypes.TryGetValue(name, out FamilySymbol cached)) {
            return cached;
        }

        var family = (Family) _document.GetElement(familyId);
        List<FamilySymbol> symbols = family.GetFamilySymbolIds()
            .Select(id => (FamilySymbol) _document.GetElement(id))
            .ToList();
        double thickness = size.Thickness * MillimetersToFeet;
        double width = size.Width * MillimetersToFeet;
        if(symbols[0].LookupParameter(PlateThicknessParameter) == null
           || symbols[0].LookupParameter(PlateWidthParameter) == null) {
            throw new RailingException("Build.PlateParameters");
        }

        FamilySymbol plateType = symbols.FirstOrDefault(symbol =>
            Math.Abs(symbol.LookupParameter(PlateThicknessParameter).AsDouble() - thickness) < SizeTolerance
            && Math.Abs(symbol.LookupParameter(PlateWidthParameter).AsDouble() - width) < SizeTolerance);
        if(plateType == null) {
            string uniqueName = name;
            for(int copyNumber = 1; symbols.Any(symbol => symbol.Name == uniqueName); copyNumber++) {
                uniqueName = name + "_" + copyNumber;
            }

            plateType = (FamilySymbol) symbols[0].Duplicate(uniqueName);
            plateType.LookupParameter(PlateThicknessParameter).Set(thickness);
            plateType.LookupParameter(PlateWidthParameter).Set(width);
            Parameter nameParameter = plateType.LookupParameter(PlateNameParameter);
            if(nameParameter is { IsReadOnly: false, StorageType: StorageType.String }) {
                nameParameter.Set(FormatSize(size.Thickness) + "х" + FormatSize(size.Width));
            }
        }

        _plateTypes[name] = plateType;
        return plateType;
    }

    private static string FormatSize(double millimeters) {
        return millimeters.ToString("0.#", CultureInfo.InvariantCulture).Replace('.', ',');
    }

    /// <summary>
    /// Сборка «&lt;начало имени&gt;N» со следующим свободным номером.
    /// </summary>
    /// <remarks>
    /// Имя типа сборки задаётся после фиксации транзакции создания: до неё Revit имя не принимает.
    /// </remarks>
    private AssemblyInstance CreateAssembly(List<ElementId> memberIds, string namePrefix, string transactionName) {
        AssemblyInstance assembly;
        using(Transaction transaction = _document.StartTransaction(transactionName)) {
            ElementId namingCategoryId = _document.GetElement(memberIds[0]).Category.Id;
            assembly = AssemblyInstance.Create(_document, memberIds, namingCategoryId);
            transaction.Commit();
        }

        using(Transaction transaction = _document.StartTransaction(transactionName)) {
            assembly.AssemblyTypeName = namePrefix + GetNextAssemblyNumber(namePrefix);
            transaction.Commit();
        }

        return assembly;
    }

    private int GetNextAssemblyNumber(string namePrefix) {
        var numberPattern = new Regex("^" + Regex.Escape(namePrefix) + @"(\d+)$");
        var usedNumbers = new FilteredElementCollector(_document)
            .OfClass(typeof(AssemblyType))
            .Select(assemblyType => numberPattern.Match(assemblyType.Name))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value))
            .ToList();
        return usedNumbers.Count > 0 ? usedNumbers.Max() + 1 : 1;
    }
}

/// <summary>
/// Итог построения ограждения.
/// </summary>
internal class RailingResult {
    public RailingResult(List<ElementId> elementIds, AssemblyInstance assembly) {
        ElementIds = elementIds;
        Assembly = assembly;
    }

    public List<ElementId> ElementIds { get; }

    /// <summary>
    /// Сборка ограждения; null — сборка не создавалась.
    /// </summary>
    public AssemblyInstance Assembly { get; }
}
