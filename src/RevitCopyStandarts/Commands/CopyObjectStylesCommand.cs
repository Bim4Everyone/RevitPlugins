using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyObjectStylesCommand : ICopyStandartsCommand {
        private readonly Document _source;
        private readonly Document _target;

        public CopyObjectStylesCommand(Document source, Document target) {
            _source = source;
            _target = target;
        }

        public void Execute() {
            using(var transactionGroup = new TransactionGroup(_target)) {
                transactionGroup.Start($"Копирование \"Стили объектов\"");

                foreach(Category sourceCategory in _source.Settings.Categories) {
                    using(var transaction = new Transaction(_target)) {
                        transaction.Start($"Копирование \"Стили объектов - {sourceCategory.Name}\"");

                        Category targetCategory = GetCategory(sourceCategory, _target.Settings.Categories);
                        if(targetCategory == null) {
                            continue;
                        }

                        CopyCategory(sourceCategory, targetCategory);

                        foreach(Category sourceSubCategory in sourceCategory.SubCategories) {
                            Category targetSubCategory = GetCategory(sourceSubCategory, targetCategory.SubCategories) ?? CreateSubCategory(targetCategory, sourceSubCategory.Name);
                            CopyCategory(sourceSubCategory, targetSubCategory);
                        }

                        transaction.Commit();
                    }
                }

                transactionGroup.Assimilate();
            }
        }

        private Category CreateSubCategory(Category parentCategory, string categoryName) {
            return _target.Settings.Categories.NewSubcategory(parentCategory, categoryName);
        }

        private Category GetCategory(Category category, CategoryNameMap categories) {
            try {
                return categories.get_Item(category.Name);
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                return null;
            }
        }

        private void CopyCategory(Category sourceCategory, Category targetCategory) {
            targetCategory.LineColor = sourceCategory.LineColor;
            if(sourceCategory.CategoryType != CategoryType.Annotation && sourceCategory.CategoryType != CategoryType.Internal) {
                targetCategory.Material = sourceCategory.Material;
            }

            try {
                var weight = sourceCategory.GetLineWeight(GraphicsStyleType.Cut);
                if(weight.HasValue && weight.Value > 0) {
                    targetCategory.SetLineWeight(weight.Value, GraphicsStyleType.Cut);
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            }

            try {
                var weight = sourceCategory.GetLineWeight(GraphicsStyleType.Projection);
                if(weight.HasValue && weight.Value > 0) {
                    targetCategory.SetLineWeight(weight.Value, GraphicsStyleType.Projection);
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            }

            SetLinePattern(_source.GetElement(sourceCategory.GetLinePatternId(GraphicsStyleType.Cut)), targetCategory, GraphicsStyleType.Cut);
            SetLinePattern(_source.GetElement(sourceCategory.GetLinePatternId(GraphicsStyleType.Projection)), targetCategory, GraphicsStyleType.Projection);
        }

        private void SetLinePattern(Element sourceLinePattern, Category targetCategory, GraphicsStyleType graphicsStyleType) {
            if(sourceLinePattern == null || targetCategory == null) {
                return;
            }

            var targetLinePattern = new FilteredElementCollector(_target).OfClass(typeof(LinePatternElement)).ToElements().FirstOrDefault(item => item.Name.Equals(sourceLinePattern.Name));
            if(targetLinePattern != null) {
                targetCategory.SetLinePatternId(targetLinePattern.Id, graphicsStyleType);
            }
        }
    }
}
