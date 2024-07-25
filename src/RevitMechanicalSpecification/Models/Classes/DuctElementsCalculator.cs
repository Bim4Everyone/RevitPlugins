using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitMechanicalSpecification.Models.Classes {
    internal class DuctElementsCalculator {

        private readonly SpecConfiguration _specConfiguration;
        private readonly UnitConverter _unitConverter;
        private readonly Document _document;
        
        public DuctElementsCalculator(SpecConfiguration config, Document document) {
            _specConfiguration = config;
            _unitConverter = new UnitConverter();
            _document = document;
        }

        private double GetBaseThikness(Element element) 
            {
            MEPCurve curve = element as MEPCurve;
            Connector connector = GetConnectors(element).FirstOrDefault();
            if(connector.Shape == ConnectorProfileType.Rectangular || connector.Shape == ConnectorProfileType.Oval) {
                double sizeA = curve.Width;
                double sizeB = curve.Height;
                double size = Math.Max(sizeA, sizeB);
                //ссылка откуда цифры
                if(size < 251) 
                    { return 0.5; }
                if(size < 1001) 
                    { return 0.7; }
                if(size < 2001) 
                    { return 0.9; }
                if(size > 2001) 
                    { return 1.4; }
            }
            if(connector.Shape == ConnectorProfileType.Round) {
                double size = curve.Diameter;
                if(size < 201) 
                    { return 0.5; }
                if(size < 451) 
                    { return 0.6; }
                if(size < 801) 
                    { return 0.7; }
                if(size < 1251) 
                    { return 1.0; }
                if(size < 1601) 
                    { return 1.2; }
                if(size > 1601) 
                    { return 1.4; }
            }
            return 0;
        }

        public string GetDuctThikness(Element element) {
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_DuctInsulations);

            double minDuctThikness = element.GetSharedParamValueOrDefault(_specConfiguration.MinDuctThikness, 0);
            double maxDuctThikness = element.GetSharedParamValueOrDefault(_specConfiguration.MaxDuctThikness, 0);
            double minInsulThikness = 0;
            double maxInsulThikness = 0;



            //Возвращается лист элементID, но тяжело представить ситуацию, где у нас больше изоляции на воздуховоде, чем 1 штука
            List<ElementId> dependent = element.GetDependentElements(filter).ToList();
            ElementId insulationId = dependent.FirstOrDefault();
            if(insulationId.IsNotNull()) 
                {
                Element insulation = _document.GetElement(insulationId);
                minInsulThikness = insulation.GetSharedParamValueOrDefault(_specConfiguration.MinDuctThikness, 0);
                maxInsulThikness = insulation.GetSharedParamValueOrDefault(_specConfiguration.MaxDuctThikness, 0);
            }

            double thikness = GetBaseThikness(element);
            double upCriteria = Math.Max(maxInsulThikness, maxDuctThikness);
            double minCriteria = Math.Max(minInsulThikness, minDuctThikness);

            //currentculture передаем
            if(thikness > upCriteria && upCriteria!=0) 
                { return Math.Max(maxInsulThikness, maxDuctThikness).ToString(); }
            if(thikness < minCriteria && minCriteria!=0) 
                { return Math.Max(minInsulThikness, minDuctThikness).ToString(); }

            return thikness.ToString();
        }

        public void GetFittingThikness() {}

        private string GetFittingAngle(Element element) 
            {
            double angle = _unitConverter.DoubleToDegree(GetConnectors(element).First().Angle);
            if(angle <= 15.1) 
                { return "15"; }
            if(angle <= 30.1) 
                { return "30"; }
            if(angle <= 45.1) 
                { return "45"; }
            if(angle <= 60.1) 
                { return "60"; }
            if(angle <= 75.1) 
                { return "75"; }
            if(angle <= 90.1) 
                { return "90"; }
            return "0";
        }
        public string GetFittingName(Element element) 
            {

            string startName = "Не удалось определить тип фитинга";
            FamilyInstance instanse = element as FamilyInstance;
            MechanicalFitting fitting = instanse.MEPModel as MechanicalFitting;
            

            if(fitting.PartType is PartType.Transition) 
                { startName = "Переход между сечениями воздуховода "; }
            if(fitting.PartType is PartType.Tee) 
                { startName = "Тройник "; }
            if(fitting.PartType is PartType.TapAdjustable) 
                { startName = "Врезка в воздуховод "; }
            if(fitting.PartType is PartType.Cross) 
                { startName = "Крестовина "; }
            if(fitting.PartType is PartType.Union) 
                { return "!Не учитывать"; }

            if(fitting.PartType == PartType.Elbow) 
                {
                Connector connector = GetConnectors(element).First();
                string angle = GetFittingAngle(element);
                if(connector.Shape == ConnectorProfileType.Round) 
                    { startName = "Отвод "+angle+ "° круглого сечения"; }
                if(connector.Shape == ConnectorProfileType.Rectangular)
                    { startName = "Отвод " + angle + "° прямоугольного сечения"; }
            }

            string size = element.GetParamValue<string>(BuiltInParameter.RBS_CALCULATED_SIZE);
            //Ревит пишет размеры всех коннекторов. Для всего кроме тройника и перехода нам хватит первого размера
            if( !(fitting.PartType is PartType.Transition) || !(fitting.PartType is PartType.Tee)) 
                { size = size.Split('-').First(); }

            return startName + " " + size;
        }

        public List<Connector> GetConnectors(Element element) {
            List<Connector> connectors = new List<Connector>();

            if (element is FamilyInstance) 
                {
                FamilyInstance instance = element as FamilyInstance;
                ConnectorSetIterator set = instance.MEPModel.ConnectorManager.Connectors.ForwardIterator();
                while(set.MoveNext()) { connectors.Add(set.Current as Connector); }
            }

            if (element.InAnyCategory(new List<BuiltInCategory>() { BuiltInCategory.OST_DuctCurves, BuiltInCategory.OST_PipeCurves })) { 
                MEPCurve curve = element as MEPCurve;
                ConnectorSetIterator set = curve.ConnectorManager.Connectors.ForwardIterator();
                while(set.MoveNext()) { connectors.Add(set.Current as Connector); }
            }
            
            return connectors;
        }

        public double GetFittingArea(Element element) {

            double area = 0;

            foreach(Solid solid in element.GetSolids()) 
                foreach(Face face in solid.Faces)
                    area += face.Area;

            area = _unitConverter.DoubleToSquareMeters(area);

            if (area > 0) 
                { 
                double falseArea = 0;
                List<Connector> connectors = GetConnectors(element);
                foreach(Connector connector in connectors) {
                    if(connector.Shape == ConnectorProfileType.Rectangular) 
                        { falseArea += _unitConverter.DoubleToSquareMeters(connector.Height * connector.Width); }
                    if(connector.Shape == ConnectorProfileType.Round) 
                        { falseArea += _unitConverter.DoubleToSquareMeters(connector.Radius * connector.Radius * 3.14); }  
                }
                //Вычетаем площадь пустоты на местах коннекторов
                area -= falseArea;
            }
            return area;
        }

    }
}
