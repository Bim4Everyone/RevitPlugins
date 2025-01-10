using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;


namespace RevitRoomExtrusion.Models {
    internal class FamilyDocument {
        private readonly RevitRepository _revitRepository;
        private readonly Application _application;

        private readonly string _templatePath;
        private readonly double _amount;
        private readonly List<RoomElement> _roomList;

        private readonly Document _familyDocument;


        public FamilyDocument(RevitRepository revitRepository, double amount, double location, List<RoomElement> roomList, string familyName = "Помещения") {
            _revitRepository = revitRepository;
            _application = _revitRepository.Application;

            string templatePath = @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\English\Metric Generic Model.rft";
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\";
            double locationString = location * 304.8;
            string fileName = $"Држ_проверка_{familyName}_отм.{locationString}";
            string filePath = $"{directory}{fileName}.rfa";

            _templatePath = templatePath;
            _amount = amount;
            _roomList = roomList;

            _familyDocument = _application.NewFamilyDocument(_templatePath);

            FamPath = filePath;
            FamName = fileName;
        }

        public string FamPath { get; }
        public string FamName { get; }


        public Document CreateFamily() {
            using(Transaction tf = new Transaction(_familyDocument, "Изменение категории, создание выдавливания")) {
                tf.Start();
                Category familyCategory = Category.GetCategory(_familyDocument, BuiltInCategory.OST_Roads);
                _familyDocument.OwnerFamily.FamilyCategory = familyCategory;

                List<Extrusion> extrusionList = new List<Extrusion>();
                foreach(RoomElement roomElement in _roomList) {
                    CurveArrArray curveArrArray = roomElement.ArrArray;
                    Extrusion extrusion = CreateExtrusion(curveArrArray, _amount);
                    extrusionList.Add(extrusion);
                }

                var materials = new FilteredElementCollector(_familyDocument)
                    .OfClass(typeof(Material))
                    .WhereElementIsNotElementType()
                    .ToElements();

                ElementId matsId = null;
                foreach(Material mat in materials) {
                    if(mat.Name.Equals("Glass", StringComparison.OrdinalIgnoreCase) || mat.Name.Equals("Стекло", StringComparison.OrdinalIgnoreCase)) {
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
            XYZ originPlane = new XYZ(0, 0, 0);
            XYZ normal = new XYZ(0, 0, 10);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, originPlane);
            SketchPlane sketchPlane = SketchPlane.Create(_familyDocument, plane);

            Autodesk.Revit.Creation.FamilyItemFactory familyCreator = _familyDocument.FamilyCreate;

            double amountFt = amount / 304.8;
            Extrusion extrusion = familyCreator.NewExtrusion(true, curveArrArray, sketchPlane, amountFt);

            return extrusion;
        }

        public static implicit operator Document(FamilyDocument v) {
            throw new NotImplementedException();
        }
    }
}
