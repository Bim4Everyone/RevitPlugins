using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitUnmodelingMep.Models.Entities;
using RevitUnmodelingMep.Models.Unmodeling;
using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Models;

internal class UnmodelingCalculator {
    private readonly DraftRowsBuilder _draftRowsBuilder;
    private readonly CalculationElementFactory _calculationElementFactory;
    private readonly FormulaEvaluator _formulaEvaluator;
    private readonly NoteGenerator _noteGenerator;

    public UnmodelingCalculator(Document doc, ILocalizationService localizationService) {
        var projectStockProvider = new ProjectStockProvider(doc);
        _calculationElementFactory = new CalculationElementFactory(doc, projectStockProvider);
        _draftRowsBuilder = new DraftRowsBuilder(doc, _calculationElementFactory, new PlaceholderReplacer());
        _formulaEvaluator = new FormulaEvaluator(localizationService);
        _noteGenerator = new NoteGenerator();
    }

    public List<NewRowElement> GetElementsToGenerate(ConsumableTypeItem config) {
        Dictionary<string, List<NewRowElement>> rowsByKey = _draftRowsBuilder.Build(config);
        return rowsByKey.Values
            .Select(rows => CreateFinalRow(rows, config))
            .ToList();
    }

    private NewRowElement CreateFinalRow(IReadOnlyList<NewRowElement> draftRows, ConsumableTypeItem config) {
        if(draftRows == null || draftRows.Count == 0) {
            return new NewRowElement();
        }

        List<CalculationElementBase> calculationElements = new List<CalculationElementBase>(draftRows.Count);
        foreach(NewRowElement draftRow in draftRows) {
            CalculationElementBase calcElement =
                draftRow.CalculationElement ?? _calculationElementFactory.Create(draftRow.Element);

            draftRow.CalculationElement = calcElement;
            draftRow.Number = _formulaEvaluator.Evaluate(config.Formula, calcElement);
            calculationElements.Add(calcElement);
        }

        NewRowElement baseRow = draftRows[0];
        double totalNumber = draftRows.Sum(r => r.Number);

        return new NewRowElement {
            Element = baseRow.Element,
            Group = baseRow.Group,
            SmrBlock = baseRow.SmrBlock,
            SmrSection = baseRow.SmrSection,
            SmrFloor = baseRow.SmrFloor,
            SmrFloorCurrency = baseRow.SmrFloorCurrency,
            Name = baseRow.Name,
            Code = baseRow.Code,
            Mark = baseRow.Mark,
            Maker = baseRow.Maker,
            Unit = baseRow.Unit,
            Number = totalNumber,
            System = baseRow.System,
            Function = baseRow.Function,
            Description = baseRow.Description,
            Mass = baseRow.Mass,
            Note = _noteGenerator.Create(config.Note, calculationElements)
        };
    }
}

