using System.Text.RegularExpressions;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

namespace RevitStairRailing.Models;

/// <summary>
/// Создание элементов ограждения в модели и сборки из них.
/// </summary>
internal class RailingBuilder {
    /// <summary>
    /// Допуск при выборе уровня под элементом, футы (1 мм).
    /// </summary>
    private const double LevelTolerance = 1 / 304.8;

    private readonly Document _document;

    public RailingBuilder(Document document) {
        _document = document;
    }

    /// <summary>
    /// Создаёт элементы ограждения и (по параметрам) сборку. Возвращает созданные элементы и сборку (или null).
    /// </summary>
    public RailingResult Build(List<RailingMember> members, RailingParameters parameters, string transactionName) {
        var typeIds = new Dictionary<RailingMemberRole, ElementId> {
            [RailingMemberRole.Post] = parameters.PostTypeId,
            [RailingMemberRole.Handrail] = parameters.HandrailTypeId,
            [RailingMemberRole.Rod] = parameters.RodTypeId,
            [RailingMemberRole.Plate] = parameters.PlateTypeId,
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
            foreach(ElementId typeId in typeIds.Values.Where(typeId => typeId != null).Distinct()) {
                var symbol = (FamilySymbol) _document.GetElement(typeId);
                if(!symbol.IsActive) {
                    symbol.Activate();
                }
            }

            double postRotation = parameters.PostRotationDegrees * Math.PI / 180;
            foreach(RailingMember member in members) {
                var symbol = (FamilySymbol) _document.GetElement(typeIds[member.Role]);
                Level level = GetLevelBelow(levels, Math.Min(member.Line.GetEndPoint(0).Z, member.Line.GetEndPoint(1).Z));
                FamilyInstance instance = _document.Create.NewFamilyInstance(
                    member.Line,
                    symbol,
                    level,
                    StructuralType.Beam);

                // профили ограждения не подрезаются соседними балками
                StructuralFramingUtils.DisallowJoinAtEnd(instance, 0);
                StructuralFramingUtils.DisallowJoinAtEnd(instance, 1);
                if(member.Role == RailingMemberRole.Post) {
                    instance.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE)?.Set(postRotation);
                }

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

    /// <summary>
    /// Ближайший уровень не выше точки; ниже всех уровней — самый нижний.
    /// </summary>
    private static Level GetLevelBelow(List<Level> levels, double elevation) {
        return levels.LastOrDefault(level => level.ProjectElevation <= elevation + LevelTolerance) ?? levels[0];
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
