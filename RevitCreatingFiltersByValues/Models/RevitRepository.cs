using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitCreatingFiltersByValues.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public string UserName => Application.Username;

        public List<ElementId> FilterableCategories => ParameterFilterUtilities.GetAllFilterableCategories().ToList();
        public List<Element> ElementsInView => new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

        public List<FillPatternElement> AllPatterns => new FilteredElementCollector(Document)
                .OfClass(typeof(FillPatternElement))
                .OfType<FillPatternElement>()
                .ToList();
        
        public List<ParameterFilterElement> AllFilterElements => new FilteredElementCollector(Document)
                .OfClass(typeof(ParameterFilterElement))
                .OfType<ParameterFilterElement>()
                .ToList();

        public List<ParameterFilterElement> AllFilterElementsInView => Document.ActiveView.GetFilters()
            .Select(id => Document.GetElement(id) as ParameterFilterElement)
            .ToList();

        public List<string> AllFilterElementNames => new FilteredElementCollector(Document)
                .OfClass(typeof(ParameterFilterElement))
                .Select(p => p.Name)
                .ToList();

        //public List<string> AllFilterElementNamesInView => Document.ActiveView.GetFilters()
        //    .Select(id => Document.GetElement(id) as ParameterFilterElement)
        //    .Select(p => p.Name)
        //    .ToList();


        /// <summary>
        /// Удаляет все пользовательские временные виды (напр., "$divin_n_...")
        /// </summary>
        public void DeleteTempFiltersInView() {
            List<ParameterFilterElement> filters = AllFilterElementsInView;
            string checkString = string.Format("${0}_", UserName);

            foreach(ParameterFilterElement filter in filters) {
                if(filter.Name.Contains(checkString)) {
                    try {
                        Document.ActiveView.RemoveFilter(filter.Id);
                    } catch(Exception) { }
                }
            }
        }
    }
}