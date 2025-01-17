using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;


namespace RevitRoomExtrusion.Models {
    internal class FamilyDocument {
        
        private readonly Application _application;
        private readonly Document _familyDocument;
        private readonly string _templatePath;         

        public FamilyDocument(
            Application application, 
            double location,             
            string familyName) {             

            string templatePath = @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\English\Metric Generic Model.rft";
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\";
            double locationString = location;
            string famName = $"Држ_Проверка_{familyName}_Отм.{locationString}";
            string famPath = $"{directory}{famName}.rfa";
            
            _application = application;
            _templatePath = templatePath;            

            _familyDocument = _application.NewFamilyDocument(_templatePath);

            FamName = famName;
            FamPath = famPath;
            
        }
        public string FamName { get; }
        public string FamPath { get; }         

        public Document CreateDocument(double amount, List<RoomElement> roomList) {
            
            using(Transaction tf = new Transaction(_familyDocument, "Изменение категории, создание выдавливания")) {            

                tf.Start();
                Category familyCategory = Category.GetCategory(_familyDocument, BuiltInCategory.OST_Roads);
                _familyDocument.OwnerFamily.FamilyCategory = familyCategory;
                List<Extrusion> extrusionList = new List<Extrusion>();                
                
                foreach(RoomElement roomElement in roomList) {
                    CurveArrArray curveArrArray = roomElement.ArrArray;
                    Extrusion extrusion = CreateExtrusion(curveArrArray, amount);
                    extrusionList.Add(extrusion);
                }
                var materials = new FilteredElementCollector(_familyDocument)
                    .OfClass(typeof(Material))
                    .WhereElementIsNotElementType()
                    .ToElements();
               
                ElementId matsId = null;
                foreach(Material mat in materials) {
                    if(mat.Name.Equals("Glass", StringComparison.OrdinalIgnoreCase) || 
                        mat.Name.Equals("Стекло", StringComparison.OrdinalIgnoreCase)) {
                        matsId = mat.Id;
                        break;
                    }
                }
                foreach(Extrusion extrusion in extrusionList) {
                    Parameter param = extrusion.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                    if(param != null && matsId != null) {
                        param.Set(matsId);
                    }
                }
                tf.Commit();
            }

            SaveAsOptions opt = new SaveAsOptions {
                OverwriteExistingFile = true
            };
            _familyDocument.SaveAs(FamPath, opt);
            _familyDocument.Close(false);
            return _familyDocument;
        }

        private Extrusion CreateExtrusion(CurveArrArray curveArrArray, double amount) {
            XYZ normal = new XYZ(0, 0, 10);
            XYZ originPlane = new XYZ(0, 0, 0);            
            Plane plane = Plane.CreateByNormalAndOrigin(normal, originPlane);
            SketchPlane sketchPlane = SketchPlane.Create(_familyDocument, plane);

            Autodesk.Revit.Creation.FamilyItemFactory familyCreator = _familyDocument.FamilyCreate;
            
            double amountFt = UnitUtils.ConvertToInternalUnits(amount, UnitTypeId.Millimeters);                     
            
            Extrusion extrusion = familyCreator.NewExtrusion(true, curveArrArray, sketchPlane, amountFt);
            return extrusion;
        }

        public static implicit operator Document(FamilyDocument v) {
            throw new NotImplementedException();
        }
    }
}
