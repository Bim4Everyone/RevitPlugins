using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitMechanicalSpecification.Models.Classes {
    internal class DuctElementsCalculator {

        private readonly SpecConfiguration _specConfiguration;
        private readonly UnitConverter _unitConverter;
        
        public DuctElementsCalculator(SpecConfiguration config) {
            _specConfiguration = config;
            _unitConverter = new UnitConverter();
        }

        public string GetDuctThikness(Element element) {
            MEPCurve curve = element as MEPCurve;
            double thikness = 0;

            double minDuctThikness = element.GetSharedParamValueOrDefault(_specConfiguration.MinDuctThikness, 0);
            double maxDuctThikness = element.GetSharedParamValueOrDefault(_specConfiguration.MaxDuctThikness, 0);
            double minInsulThikness = 0;
            double maxInsulThikness = 0;


            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_DuctInsulations);
            List<ElementId> dependent = element.GetDependentElements(filter).ToList();
            if(dependent.Count > 0) 
                {
                Element insulation = _specConfiguration.Document.GetElement(dependent[0]);
                minInsulThikness = insulation.GetSharedParamValueOrDefault(_specConfiguration.MinDuctThikness, 0);
                maxInsulThikness = insulation.GetSharedParamValueOrDefault(_specConfiguration.MaxDuctThikness, 0);
            }


            if(GetConnectors(element)[0].Shape == ConnectorProfileType.Rectangular || GetConnectors(element)[0].Shape == ConnectorProfileType.Oval) {
                double sizeA = curve.Width;
                double sizeB = curve.Height;
                double size = Math.Max(sizeA, sizeB);

                if(size < 251) { thikness = 0.5; }
                if(size < 1001) { thikness = 0.7; }
                if(size < 2001) { thikness = 0.9; }
                if(size > 2001) { thikness = 1.4; }
            }
            if(GetConnectors(element)[0].Shape == ConnectorProfileType.Round) {
                double size = curve.Diameter;
                if(size < 201) { thikness = 0.5; }
                if(size < 451) { thikness = 0.6; }
                if(size < 801) { thikness = 0.7; }
                if(size < 1251) { thikness = 1.0; }
                if(size < 1601) { thikness = 1.2; }
                if(size > 1601) { thikness = 1.4; }
            }

            double upCriteria = Math.Max(maxInsulThikness, maxDuctThikness);
            double minCriteria = Math.Max(minInsulThikness, minDuctThikness);

            if(thikness > upCriteria && upCriteria!=0) { return Math.Max(maxInsulThikness, maxDuctThikness).ToString(); }
            if(thikness < minCriteria && minCriteria!=0) { return Math.Max(minInsulThikness, minDuctThikness).ToString(); }

            return thikness.ToString();
        }

        public void GetFittingThikness() {}

        public void GetFittingName() { }

        public List<Connector> GetConnectors(Element element) {
            List<Connector> connectors = new List<Connector>();

            if (element.GetType().Name == "FamilyInstance") 
                {
                FamilyInstance instance = element as FamilyInstance;
                ConnectorSetIterator set = instance.MEPModel.ConnectorManager.Connectors.ForwardIterator();
                while(set.MoveNext()) { connectors.Add(set.Current as Connector); }
            }
            if (element.Category.IsId(BuiltInCategory.OST_DuctCurves)||element.Category.IsId(BuiltInCategory.OST_PipeCurves)) { 
                MEPCurve curve = element as MEPCurve;
                ConnectorSetIterator set = curve.ConnectorManager.Connectors.ForwardIterator();
                while(set.MoveNext()) { connectors.Add(set.Current as Connector); }
            }
            
            return connectors;
        }

        public double GetFittingArea(Element element) {

            double area = 0;
            List<Solid> solids = element.GetSolids().ToList();

            foreach(Solid solid in solids) 
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
                    
                area -= falseArea;
            }
            return area;
        }

    }
}
