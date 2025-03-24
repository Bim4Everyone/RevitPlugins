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
    internal class CategorySelectionService : ICategorySelectionService {
        private readonly Func<CategoriesWindow> _createCategoriesWindow;
        private readonly RevitRepository _revitRepository;

        public CategorySelectionService(Func<CategoriesWindow> createCategoriesWindow, RevitRepository revitRepository) {
            _createCategoriesWindow = createCategoriesWindow;
            _revitRepository = revitRepository;
        }

        public List<Category> SelectCategories(List<Category> currentSelection) {
            var window = _createCategoriesWindow();
            window.DataContext = new CategoriesViewModel(_revitRepository, currentSelection);
            if(window.ShowDialog() == true) {
                return ((CategoriesViewModel) window.DataContext).GetSelectedCategories();
            }

            return null;
        }
    }
}
