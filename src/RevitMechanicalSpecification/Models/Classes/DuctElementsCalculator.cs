using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

namespace RevitMechanicalSpecification.Models.Classes {
    internal class DuctElementsCalculator {
        
        public DuctElementsCalculator() { }

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
            if (element.GetType().Name == "MEPCurve") { 
                MEPCurve curve = element as MEPCurve;
                ConnectorSetIterator set = curve.ConnectorManager.Connectors.ForwardIterator();
                while(set.MoveNext()) { connectors.Add(set.Current as Connector); }
            }
            
            return connectors;
        }

        public double GetFittingArea(Element element) {

            double area = 0;
            UnitConverter unitConverter = new UnitConverter();
            List<Solid> solids = element.GetSolids().ToList();

            foreach(Solid solid in solids) 
                foreach(Face face in solid.Faces)
                    area += face.Area;

            area = unitConverter.DoubleToSquareMeters(area);

            if (area > 0) 
                { 
                double falseArea = 0;
                List<Connector> connectors = GetConnectors(element);
                foreach(Connector connector in connectors) {
                    if(connector.Shape == ConnectorProfileType.Rectangular) 
                        { falseArea += unitConverter.DoubleToSquareMeters(connector.Height * connector.Width); }
                    if(connector.Shape == ConnectorProfileType.Round) 
                        { falseArea += unitConverter.DoubleToSquareMeters(connector.Radius * connector.Radius * 3.14); }  
                }
                    
                area -= falseArea;
            }
            return area;
        }

    }
}
