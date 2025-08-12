using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyObjectStylesCommand : ICopyStandartsCommand {
    private readonly Document _source;
    private readonly Document _target;
    private readonly ILocalizationService _localizationService;

    public CopyObjectStylesCommand(Document source, Document target, ILocalizationService localizationService) {
        _source = source;
        _target = target;
        _localizationService = localizationService;
    }

    public void Execute() {
        using var transactionGroup = new TransactionGroup(_target);
        transactionGroup.BIMStart(
            _localizationService.GetLocalizedString("CopyObjectStylesCommandTransactionGroup"));

        foreach(Category sourceCategory in _source.Settings.Categories) {
            using var transaction = new Transaction(_target);
            transaction.BIMStart(_localizationService.GetLocalizedString("CopyObjectStylesCommandTransaction", sourceCategory.Name));

            var targetCategory = GetCategory(sourceCategory, _target.Settings.Categories);
            if(targetCategory == null) {
                continue;
            }

            CopyCategory(sourceCategory, targetCategory);

            foreach(Category sourceSubCategory in sourceCategory.SubCategories) {
                var targetSubCategory = GetCategory(sourceSubCategory, targetCategory.SubCategories)
                                        ?? CreateSubCategory(targetCategory, sourceSubCategory.Name);
                CopyCategory(sourceSubCategory, targetSubCategory);
            }

            transaction.Commit();
        }

        transactionGroup.Assimilate();
    }

    private Category CreateSubCategory(Category parentCategory, string categoryName) {
        return _target.Settings.Categories.NewSubcategory(parentCategory, categoryName);
    }

    private Category GetCategory(Category category, CategoryNameMap categories) {
        try {
            return categories.get_Item(category.Name);
        } catch(InvalidOperationException) {
            return null;
        }
    }

    private void CopyCategory(Category sourceCategory, Category targetCategory) {
        targetCategory.LineColor = sourceCategory.LineColor;
        if(sourceCategory.CategoryType != CategoryType.Annotation
           && sourceCategory.CategoryType != CategoryType.Internal) {
            targetCategory.Material = sourceCategory.Material;
        }

        try {
            int? weight = sourceCategory.GetLineWeight(GraphicsStyleType.Cut);
            if(weight > 0) {
                targetCategory.SetLineWeight(weight.Value, GraphicsStyleType.Cut);
            }
        } catch(InvalidOperationException) {
        }

        try {
            int? weight = sourceCategory.GetLineWeight(GraphicsStyleType.Projection);
            if(weight > 0) {
                targetCategory.SetLineWeight(weight.Value, GraphicsStyleType.Projection);
            }
        } catch(InvalidOperationException) {
        }

        SetLinePattern(
            _source.GetElement(sourceCategory.GetLinePatternId(GraphicsStyleType.Cut)),
            targetCategory,
            GraphicsStyleType.Cut);
        SetLinePattern(
            _source.GetElement(sourceCategory.GetLinePatternId(GraphicsStyleType.Projection)),
            targetCategory,
            GraphicsStyleType.Projection);
    }

    private void SetLinePattern(
        Element sourceLinePattern,
        Category targetCategory,
        GraphicsStyleType graphicsStyleType) {
        if(sourceLinePattern == null
           || targetCategory == null) {
            return;
        }

        var targetLinePattern = new FilteredElementCollector(_target).OfClass(typeof(LinePatternElement))
            .ToElements()
            .FirstOrDefault(item => item.Name.Equals(sourceLinePattern.Name));
        if(targetLinePattern != null) {
            targetCategory.SetLinePatternId(targetLinePattern.Id, graphicsStyleType);
        }
    }
}
