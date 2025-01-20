using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;


namespace RevitRoomExtrusion.Models {
    internal class FamilyDocument {

#if REVIT_2022
        private const string _templatePath = @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\English\Metric Generic Model.rft";
#elif REVIT_2023
        private const string _templatePath = @"C:\ProgramData\Autodesk\RVT 2023\Family Templates\English\Metric Generic Model.rft";
#elif REVIT_2024
        private const string _templatePath = @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\English\Metric Generic Model.rft";
#endif

        private readonly Application _application;
        private readonly Document _familyDocument;                       
        private readonly double _location;
        private readonly string _familyName;
        private readonly int _normalDirection = 10;

        public FamilyDocument(Application application, double location, string familyName) {            
            
            _application = application; 
            _familyDocument = _application.NewFamilyDocument(_templatePath);
            _location = location;
            _familyName = familyName;

            SetFamilyNameAndPath();
        }

        public string FamName { get; private set; }
        public string FamPath { get; private set; }         

        public Document CreateDocument(double amount, List<RoomElement> roomList) {

            Category familyCategory = Category.GetCategory(_familyDocument, BuiltInCategory.OST_Roads);
            ElementId materialElementId = GetMaterialElementId();

            using(Transaction t = new Transaction(_familyDocument, "Категория, выдавливания, материал")) {
                
                t.Start();                
                _familyDocument.OwnerFamily.FamilyCategory = familyCategory;                

                List<Extrusion> extrusionList = roomList
                    .Select(roomElement => {
                        CurveArrArray curveArrArray = roomElement.ArrArray;
                        return CreateExtrusion(curveArrArray, amount);
                        })
                    .ToList();

                foreach(Extrusion extrusion in extrusionList) {
                    Parameter param = extrusion.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                    if(param != null && materialElementId != null) {
                        param.Set(materialElementId);
                    }
                }
                t.Commit();
            }

            SaveAsOptions opt = new SaveAsOptions {
                OverwriteExistingFile = true
            };
            _familyDocument.SaveAs(FamPath, opt);
            _familyDocument.Close(false);
            return _familyDocument;
        }

        private ElementId GetMaterialElementId() {
            
            var materials = new FilteredElementCollector(_familyDocument)
                    .OfClass(typeof(Material))
                    .WhereElementIsNotElementType()
                    .ToElements();

            return materials
                .FirstOrDefault(mat => mat.Name.Equals("glass", StringComparison.OrdinalIgnoreCase) ||
                    mat.Name.Equals("стекло", StringComparison.OrdinalIgnoreCase))
                    ?.Id;            
        }

        private Extrusion CreateExtrusion(CurveArrArray curveArrArray, double amount) {
            XYZ normal = new XYZ(0, 0, _normalDirection);
            XYZ originPlane = new XYZ(0, 0, 0);            
            Plane plane = Plane.CreateByNormalAndOrigin(normal, originPlane);
            SketchPlane sketchPlane = SketchPlane.Create(_familyDocument, plane);

            Autodesk.Revit.Creation.FamilyItemFactory familyCreator = _familyDocument.FamilyCreate;
            
            double amountFt = UnitUtils.ConvertToInternalUnits(amount, UnitTypeId.Millimeters);                     
            
            Extrusion extrusion = familyCreator.NewExtrusion(true, curveArrArray, sketchPlane, amountFt);
            return extrusion;
        }

        private void SetFamilyNameAndPath() {            
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\";
            string famName = $"Држ_Проверка_{_familyName}_Отм.{_location}";
            string famPath = $"{directory}{famName}.rfa";
            FamName = famName;
            FamPath = famPath;
        }         
    }
}
