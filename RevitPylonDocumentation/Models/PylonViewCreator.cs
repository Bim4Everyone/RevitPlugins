using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models {
    public class PylonViewCreator {
        internal PylonViewCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }


        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }



        public ViewSection CreateGeneralView(ViewFamilyType SelectedViewFamilyType) {

            // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
            int count = 0;
            Element elemForWork = null;
            foreach(Element elem in SheetInfo.HostElems) {
                elemForWork = elem;
                count++;
            }

            if(elemForWork is null) { return null; }

            ViewSection viewSection = null;
            BoundingBoxXYZ bb = elemForWork.get_BoundingBox(null);
            double minZ = bb.Min.Z;
            double maxZ = bb.Max.Z;


            double hostLength;
            double hostWidth;
            double offset;

            XYZ sectionBoxMin = null;
            XYZ sectionBoxMax = null;

            // Переменные для данных для объекта Transform
            XYZ originPoint = null;
            XYZ hostDir = null;
            XYZ upDir = null;
            XYZ viewDir = null;

            XYZ midlePoint = null;
            XYZ hostVector = null;


            if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralColumns) {
                FamilyInstance column = elemForWork as FamilyInstance;

                LocationPoint locationPoint = column.Location as LocationPoint;
                midlePoint = locationPoint.Point;
                double rotation = locationPoint.Rotation + (90 * Math.PI / 180);
                hostVector = Transform.CreateRotation(XYZ.BasisZ, rotation).OfVector(XYZ.BasisX);

                FamilySymbol hostSymbol = column.Symbol;
                hostLength = hostSymbol.LookupParameter("ADSK_Размер_Ширина").AsDouble();
                hostWidth = hostSymbol.LookupParameter("ADSK_Размер_Высота").AsDouble();

            } else if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls) {
                Wall wall = elemForWork as Wall;
                if(wall is null) { return null;  }
                LocationCurve locationCurve = wall.Location as LocationCurve;
                Line line = locationCurve.Curve as Line;

                if(line is null) { return null; }

                XYZ wallLineStart = line.GetEndPoint(0);
                XYZ wallLineEnd = line.GetEndPoint(1);
                hostVector = wallLineEnd - wallLineStart;
                hostLength = hostVector.GetLength();

                hostWidth = wall.WallType.Width;
                midlePoint = wallLineStart + 0.5 * hostVector;
            } else { return null; }


            offset = 0.1 * hostLength;

            // Формируем данные для объекта Transform
            originPoint = midlePoint;
            hostDir = hostVector.Normalize();
            upDir = XYZ.BasisZ;
            viewDir = hostDir.CrossProduct(upDir);


            // Передаем данные для объекта Transform
            Transform t = Transform.Identity;
            t.Origin = originPoint;
            t.BasisX = hostDir;
            t.BasisY = upDir;
            t.BasisZ = viewDir;


            sectionBoxMin = new XYZ(-hostLength * 0.6, minZ - originPoint.Z - offset, -hostWidth);
            sectionBoxMax = new XYZ(hostLength * 0.6, maxZ - originPoint.Z + offset, hostWidth);


            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = t;
            sectionBox.Min = sectionBoxMin;
            sectionBox.Max = sectionBoxMax;

            viewSection = ViewSection.CreateSection(Repository.Document, SelectedViewFamilyType.Id, sectionBox);

            if(viewSection != null) { viewSection.Name = ViewModel.GENERAL_VIEW_PREFIX + SheetInfo.PylonKeyName + ViewModel.GENERAL_VIEW_SUFFIX; }

            return viewSection;
        }




        public ViewSection CreateGeneralPerpendicularView(ViewFamilyType SelectedViewFamilyType) {

            // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
            int count = 0;
            Element elemForWork = null;
            foreach(Element elem in SheetInfo.HostElems) {
                elemForWork = elem;
                count++;
            }

            if(elemForWork is null) { return null; }

            ViewSection viewSection = null;
            BoundingBoxXYZ bb = elemForWork.get_BoundingBox(null);
            double minZ = bb.Min.Z;
            double maxZ = bb.Max.Z;


            double hostLength;
            double hostWidth;
            double offset;

            XYZ sectionBoxMin = null;
            XYZ sectionBoxMax = null;

            // Переменные для данных для объекта Transform
            XYZ originPoint = null;
            XYZ rightDir = null;
            XYZ upDir = null;
            XYZ viewDir = null;

            XYZ midlePoint = null;
            XYZ hostVector = null;


            if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralColumns) {
                FamilyInstance column = elemForWork as FamilyInstance;

                LocationPoint locationPoint = column.Location as LocationPoint;
                midlePoint = locationPoint.Point;
                double rotation = locationPoint.Rotation + (90 * Math.PI / 180);
                hostVector = Transform.CreateRotation(XYZ.BasisZ, rotation).OfVector(XYZ.BasisX);

                FamilySymbol hostSymbol = column.Symbol;
                hostLength = hostSymbol.LookupParameter("ADSK_Размер_Ширина").AsDouble();
                hostWidth = hostSymbol.LookupParameter("ADSK_Размер_Высота").AsDouble();

            } else if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls) {
                Wall wall = elemForWork as Wall;
                if(wall is null) { return null; }
                LocationCurve locationCurve = wall.Location as LocationCurve;
                Line line = locationCurve.Curve as Line;

                if(line is null) { return null; }

                XYZ wallLineStart = line.GetEndPoint(0);
                XYZ wallLineEnd = line.GetEndPoint(1);
                hostVector = wallLineEnd - wallLineStart;
                hostLength = hostVector.GetLength();

                hostWidth = wall.WallType.Width;
                midlePoint = wallLineStart + 0.5 * hostVector;
            } else { return null; }


            offset = 0.1 * hostLength;

            // Формируем данные для объекта Transform
            originPoint = midlePoint;
            upDir = XYZ.BasisZ;
            viewDir = hostVector.Normalize();
            rightDir = upDir.CrossProduct(viewDir);


            // Передаем данные для объекта Transform
            Transform t = Transform.Identity;
            t.Origin = originPoint;
            t.BasisX = rightDir;
            t.BasisY = upDir;
            t.BasisZ = viewDir;


            sectionBoxMin = new XYZ(-hostWidth * 1.5, minZ - originPoint.Z - offset, -hostLength * 0.4);
            sectionBoxMax = new XYZ(hostWidth * 1.5, maxZ - originPoint.Z + offset, hostLength * 0.4);


            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = t;
            sectionBox.Min = sectionBoxMin;
            sectionBox.Max = sectionBoxMax;

            viewSection = ViewSection.CreateSection(Repository.Document, SelectedViewFamilyType.Id, sectionBox);

            if(viewSection != null) { viewSection.Name = ViewModel.GENERAL_VIEW_PERPENDICULAR_PREFIX + SheetInfo.PylonKeyName + ViewModel.GENERAL_VIEW_PERPENDICULAR_SUFFIX; }

            return viewSection;
        }





        public ViewSection CreateTransverseView(ViewFamilyType SelectedViewFamilyType, int transverseViewNum) {

            // Потом сделать выбор через уникальный идентификатор (или сделать подбор раньше)
            int count = 0;
            Element elemForWork = null;
            foreach(Element elem in SheetInfo.HostElems) {
                elemForWork = elem;
                count++;
            }

            if(elemForWork is null) { return null; }

            ViewSection viewSection = null;
            BoundingBoxXYZ bb = elemForWork.get_BoundingBox(null);
            double minZ = bb.Min.Z;
            double maxZ = bb.Max.Z;
            //double hostHeight = maxZ - minZ;


            double hostLength;
            double hostWidth;
            double offset;

            XYZ sectionBoxMin = null;
            XYZ sectionBoxMax = null;

            // Переменные для данных для объекта Transform
            XYZ originPoint = null;
            XYZ hostDir = null;
            XYZ upDir = null;
            XYZ viewDir = null;

            XYZ midlePoint = null;
            XYZ hostVector = null;


            if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralColumns) {
                FamilyInstance column = elemForWork as FamilyInstance;

                LocationPoint locationPoint = column.Location as LocationPoint;
                midlePoint = locationPoint.Point;
                double rotation = locationPoint.Rotation + (90 * Math.PI / 180);
                hostVector = Transform.CreateRotation(XYZ.BasisZ, rotation).OfVector(XYZ.BasisX);

                FamilySymbol hostSymbol = column.Symbol;
                hostLength = hostSymbol.LookupParameter("ADSK_Размер_Ширина").AsDouble();
                hostWidth = hostSymbol.LookupParameter("ADSK_Размер_Высота").AsDouble();

            } else if(elemForWork.Category.GetBuiltInCategory() == BuiltInCategory.OST_Walls) {
                Wall wall = elemForWork as Wall;
                if(wall is null) { return null; }
                LocationCurve locationCurve = wall.Location as LocationCurve;
                Line line = locationCurve.Curve as Line;

                if(line is null) { return null; }

                XYZ wallLineStart = line.GetEndPoint(0);
                XYZ wallLineEnd = line.GetEndPoint(1);
                hostVector = wallLineEnd - wallLineStart;
                hostLength = hostVector.GetLength();

                hostWidth = wall.WallType.Width;
                midlePoint = wallLineStart + 0.5 * hostVector;
            } else { return null; }


            offset = 0.1 * hostLength;

            // Формируем данные для объекта Transform
            originPoint = midlePoint;
            hostDir = hostVector.Normalize();
            viewDir = XYZ.BasisZ.Negate();
            upDir = viewDir.CrossProduct(hostDir);


            // Передаем данные для объекта Transform
            Transform t = Transform.Identity;
            t.Origin = originPoint;
            t.BasisX = hostDir;
            t.BasisY = upDir;
            t.BasisZ = viewDir;


            if(transverseViewNum == 1) {
                // Располагаем сечение на высоте 1/4 высоты пилона
                sectionBoxMin = new XYZ(-hostLength * 0.6, -hostWidth, -(minZ + (maxZ - minZ) / 4 - originPoint.Z));
                sectionBoxMax = new XYZ(hostLength * 0.6, hostWidth, -(minZ + (maxZ - minZ) / 8 - originPoint.Z));
            } else if(transverseViewNum == 2) {
                // Располагаем сечение на высоте 1/2 высоты пилона
                sectionBoxMin = new XYZ(-hostLength * 0.6, -hostWidth, -(minZ + (maxZ - minZ) / 2 - originPoint.Z));
                sectionBoxMax = new XYZ(hostLength * 0.6, hostWidth, -(minZ + (maxZ - minZ) / 8 * 3 - originPoint.Z));
            } else if(transverseViewNum == 3) {
                // Располагаем сечение на высоте 5/4 высоты пилона
                sectionBoxMin = new XYZ(-hostLength * 0.6, -hostWidth, -(minZ + (maxZ - minZ) / 4 * 5 - originPoint.Z));
                sectionBoxMax = new XYZ(hostLength * 0.6, hostWidth, -(minZ + (maxZ - minZ) / 8 * 9 - originPoint.Z));
            } else {
                return null;
            }



            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = t;
            sectionBox.Min = sectionBoxMin;
            sectionBox.Max = sectionBoxMax;

            viewSection = ViewSection.CreateSection(Repository.Document, SelectedViewFamilyType.Id, sectionBox);

            if(viewSection != null) {
                if(transverseViewNum == 1) {
                    viewSection.Name = ViewModel.TRANSVERSE_VIEW_FIRST_PREFIX + SheetInfo.PylonKeyName + ViewModel.TRANSVERSE_VIEW_FIRST_SUFFIX;
                } else if(transverseViewNum == 2) {
                    viewSection.Name = ViewModel.TRANSVERSE_VIEW_SECOND_PREFIX + SheetInfo.PylonKeyName + ViewModel.TRANSVERSE_VIEW_SECOND_SUFFIX;
                } else if(transverseViewNum == 3) {
                    viewSection.Name = ViewModel.TRANSVERSE_VIEW_THIRD_PREFIX + SheetInfo.PylonKeyName + ViewModel.TRANSVERSE_VIEW_THIRD_SUFFIX;
                }
            }


            return viewSection;
        }

    }
}
