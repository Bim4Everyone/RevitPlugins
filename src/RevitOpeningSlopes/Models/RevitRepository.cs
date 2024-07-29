using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class RevitRepository {
        private readonly WindowSelectionFilter _selectionFilter;

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;
        public View ActiveView => ActiveUIDocument.ActiveView;
        public View3D Default3DView => GetDefaultView3D();

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
            _selectionFilter = new WindowSelectionFilter();
        }

        public XYZ GetOpeningVector(FamilyInstance opening) {
            XYZ openingVector = null;
            if(opening != null) {
                openingVector = opening.FacingOrientation.Normalize();
            }
            return openingVector;
        }

        /// <summary>
        /// Функция возвращает допустимые типоразмеры откоса для выбора пользователем
        /// </summary>
        /// <returns></returns>
        public IList<FamilySymbol> GetSlopeTypes() {
            string familyName = "ОбщМд_Откос_Отлив_Примыкание";
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .Where(el => el is FamilySymbol fs && fs.Family.Name == familyName)
                .Cast<FamilySymbol>()
                .OrderBy(el => el.Name)
                .ToList();
        }

        public FamilySymbol GetSlopeType(ElementId slopeTypeId) {
            if(slopeTypeId.IsNull()) {
                throw new ArgumentNullException(nameof(slopeTypeId));
            }
            return Document.GetElement(slopeTypeId) as FamilySymbol ?? throw new ArgumentException(nameof(slopeTypeId));
        }

        public ICollection<FamilyInstance> SelectWindowsOnView() {
            IList<Reference> pickedElementsReference = ActiveUIDocument
                .Selection
                .PickObjects(ObjectType.Element, _selectionFilter, "Выберите экземпляры окон");

            return pickedElementsReference
                .Where(r => r != null)
                .Select(r => Document.GetElement(r))
                .Where(el => el is FamilyInstance)
                .Cast<FamilyInstance>()
                .ToList();
        }

        public ICollection<FamilyInstance> GetSelectedWindows() {
            return ActiveUIDocument
                .GetSelectedElements()
                .Where(el => el.Category.IsId(BuiltInCategory.OST_Windows))
                .Cast<FamilyInstance>()
                .ToList();
        }

        public ICollection<FamilyInstance> GetWindowsOnActiveView() {
            return new FilteredElementCollector(Document, ActiveView.Id)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(el => el.SuperComponent == null)
                .ToList();
        }

        public double ConvertToFeet(double value) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Millimeters);
#endif
        }

        public double ConvertToMillimeters(double value) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Millimeters);
#endif
        }

        /// <summary>
        /// Возвращает 3D вид по умолчанию
        /// </summary>
        /// <returns></returns>
        private View3D GetDefaultView3D() {
            // хоть в ревите по умолчанию и присутствует "{3D}" вид, фигурные скобки запрещены в названиях
            const string defaultRevitView3dName = "{3D}";
            const string defaultView3dName = "3D";
            var views3D = new FilteredElementCollector(Document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .ToArray();

            // ищем 3D вид ревита по умолчанию
            var view = views3D.FirstOrDefault(
                item => item.Name.Equals(defaultRevitView3dName, StringComparison.CurrentCultureIgnoreCase));
            if(view == null) {
                // ищем наш 3D вид по умолчанию
                view = views3D.FirstOrDefault(
                    item => item.Name.Equals(defaultView3dName, StringComparison.CurrentCultureIgnoreCase));
            }
            if(view == null) {
                // создаем наш 3D вид по умолчанию
                var type = new FilteredElementCollector(Document)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                type.DefaultTemplateId = ElementId.InvalidElementId;
                view = View3D.CreateIsometric(Document, type.Id);
                view.Name = defaultView3dName;
            }
            return view;
        }
    }
}
