using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using RevitMirroredElements.Interfaces;
using RevitMirroredElements.Models;
using RevitMirroredElements.ViewModels;
using RevitMirroredElements.Views;

namespace RevitMirroredElements.Services
{
    public class CategorySelectionService : ICategorySelectionService {
        private readonly Func<CategoriesWindow> _createCategoriesWindow;

        public CategorySelectionService(Func<CategoriesWindow> createCategoriesWindow) {
            _createCategoriesWindow = createCategoriesWindow;
        }

        public List<Category> SelectCategories() {
            var categoriesWindow = _createCategoriesWindow();
            if(categoriesWindow.ShowDialog() == true) {
                return ((CategoriesViewModel) categoriesWindow.DataContext).GetSelectedCategories();
            }
            return new List<Category>();
        }
    }
}
