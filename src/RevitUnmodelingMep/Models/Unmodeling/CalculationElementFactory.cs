using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.Revit.Geometry;


using RevitUnmodelingMep.Models.Entities;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class CalculationElementFactory {
    private readonly Document _doc;
    private readonly ProjectStockProvider _projectStockProvider;

    public CalculationElementFactory(Document doc, ProjectStockProvider projectStockProvider) {
        _doc = doc;
        _projectStockProvider = projectStockProvider;
    }

    public CalculationElementBase Create(Element element) {
        if(element == null) {
            throw new ArgumentNullException(nameof(element));
        }

        if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
            return CreateDuct((MEPCurve) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
            return CreatePipe((Pipe) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) {
            return CreatePipeInsulation((PipeInsulation) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) {
            return CreateDuctInsulation((DuctInsulation) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_DuctSystem) || element.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
            return CreateSystem(element);
        }

        throw new InvalidOperationException($"GetGeometricDescription unexpected element type: (Id {element?.Id})");
    }

    private CalculationElementBase CreateDuct(MEPCurve duct) {
        DuctType ductType = (DuctType) duct.GetElementType();
        CalculationElementDuct calculationElement = new CalculationElementDuct(duct) {
            ProjectStock = _projectStockProvider.Get(BuiltInCategory.OST_DuctCurves)
        };

        calculationElement.SystemSharedName =
            duct.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");

        calculationElement.SystemTypeName = duct.MEPSystem?.GetElementType()?.Name ?? string.Empty;

        double length = duct.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        if(ductType.Shape == ConnectorProfileType.Round) {
            calculationElement.IsRound = true;
            calculationElement.Diameter_mm = duct.Diameter;
            calculationElement.Perimeter_mm = Math.PI * duct.Diameter;
            double crossSectionArea = Math.PI * Math.Pow(duct.Diameter / 2, 2);
            double volume = crossSectionArea * length;
            calculationElement.Volume_m3 = volume;
        } else {
            calculationElement.IsRound = false;
            calculationElement.Width_mm = duct.Width;
            calculationElement.Height_mm = duct.Height;
            calculationElement.Perimeter_mm = duct.Width * 2 + duct.Height * 2;
            double crossSectionArea = duct.Width * duct.Height;
            double volume = crossSectionArea * length;
            calculationElement.Volume_m3 = volume;
        }

        double area = duct.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
        calculationElement.Area_m2 = area;
        calculationElement.Length_mm = length;
        calculationElement.InsulationThikness_mm =
            duct.GetParamValueOrDefault<double>(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);

        if(calculationElement.InsulationThikness_mm == 0) {
            calculationElement.IsInsulated = false;
        } else {
            calculationElement.IsInsulated = true;
            ElementFilter insulationFilter = new ElementClassFilter(typeof(DuctInsulation));
            ICollection<ElementId> dependentInsulations = duct.GetDependentElements(insulationFilter);
            double sumInsulationArea = 0;
            foreach(ElementId insulationId in dependentInsulations) {
                Element insulation = _doc.GetElement(insulationId);
                double insArea = insulation.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                sumInsulationArea += insArea;
            }
            calculationElement.InsulationArea_m2 = sumInsulationArea;
        }

        return calculationElement;
    }

    private CalculationElementBase CreatePipe(Pipe pipe) {
        CalculationElementPipe calculationElement = new CalculationElementPipe(pipe) {
            ProjectStock = _projectStockProvider.Get(BuiltInCategory.OST_PipeCurves)
        };

        calculationElement.SystemSharedName =
            pipe.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        calculationElement.SystemTypeName = pipe.MEPSystem?.GetElementType()?.Name ?? string.Empty;

        double length = pipe.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        double outDiameter = pipe.GetParamValueOrDefault<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
        double crossSectionArea = Math.PI * Math.Pow(pipe.Diameter / 2, 2);
        double surfaceArea = Math.PI * outDiameter * length;
        double volume = crossSectionArea * length;

        calculationElement.IsRound = true;
        calculationElement.Diameter_mm = pipe.Diameter;
        calculationElement.OutDiameter_mm = outDiameter;
        calculationElement.Area_m2 = surfaceArea;
        calculationElement.Volume_m3 = volume;
        calculationElement.Perimeter_mm = Math.PI * pipe.Diameter;
        calculationElement.Length_mm = length;

        calculationElement.InsulationThikness_mm =
            pipe.GetParamValueOrDefault<double>(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
        if(calculationElement.InsulationThikness_mm == 0) {
            calculationElement.IsInsulated = false;
        } else {
            calculationElement.IsInsulated = true;
            ElementFilter insulationFilter = new ElementClassFilter(typeof(PipeInsulation));
            ICollection<ElementId> dependentInsulations = pipe.GetDependentElements(insulationFilter);
            double sumInsulationArea = 0;
            foreach(ElementId insulationId in dependentInsulations) {
                Element insulation = _doc.GetElement(insulationId);
                double insArea = insulation.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                sumInsulationArea += insArea;
            }
            calculationElement.InsulationArea_m2 = sumInsulationArea;
        }

        return calculationElement;
    }

    private CalculationElementBase CreatePipeInsulation(PipeInsulation pipeIns) {
        CalculationElementPipeIns calculationElement = new CalculationElementPipeIns(pipeIns) {
            ProjectStock = _projectStockProvider.Get(BuiltInCategory.OST_PipeInsulations)
        };

        Pipe pipe = (Pipe) _doc.GetElement(pipeIns.HostElementId);
        calculationElement.SystemSharedName =
            pipeIns.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        calculationElement.SystemTypeName = pipe?.MEPSystem?.GetElementType()?.Name ?? string.Empty;

        calculationElement.InsulationThikness_mm = pipeIns.Thickness;
        calculationElement.IsRound = true;
        calculationElement.Diameter_mm = pipe.Diameter;
        calculationElement.OutDiameter_mm = pipeIns.Diameter;
        calculationElement.Area_m2 = pipeIns.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
        calculationElement.Perimeter_mm = Math.PI * pipeIns.Diameter;
        calculationElement.Length_mm = pipeIns.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);

        return calculationElement;
    }

    private double GetFittingArea(Element element) {
        double area = 0;

        foreach(Solid solid in element.GetSolids())
        foreach(Face face in solid.Faces) {
            area += face.Area;
        }

        if(area <= 0) {
            return area;
        }

        double connectorArea = 0;
        foreach(Connector connector in GetConnectors(element)) {
            if(connector.Shape == ConnectorProfileType.Rectangular) {
                connectorArea += connector.Height * connector.Width;
            }
            if(connector.Shape == ConnectorProfileType.Round) {
                connectorArea += Math.PI * Math.Pow(connector.Radius, 2);
            }
        }

        return area - connectorArea;
    }

    private List<Connector> GetConnectors(Element element) {
        if(element is not FamilyInstance instance || instance.MEPModel?.ConnectorManager == null) {
            return new List<Connector>();
        }

        return instance.MEPModel.ConnectorManager.Connectors.Cast<Connector>().ToList();
    }

    private CalculationElementBase CreateDuctInsulation(DuctInsulation ductIns) {
        CalculationElementDuctIns calculationElement = new CalculationElementDuctIns(ductIns) {
            ProjectStock = _projectStockProvider.Get(BuiltInCategory.OST_DuctInsulations)
        };
        
        calculationElement.SystemSharedName =
            ductIns.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        
        calculationElement.Length_mm = ductIns.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        
        var insulationHost = _doc.GetElement(ductIns.HostElementId);

        if(insulationHost.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
            calculationElement.Area_m2 = GetFittingArea(insulationHost);
            
            calculationElement.SystemTypeName =
                insulationHost.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM).AsValueString();
            
            return calculationElement;
        }
        calculationElement.Area_m2 = ductIns.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
        MEPCurve duct = insulationHost as MEPCurve;
        if(duct == null) {
            return calculationElement;
        }
        
        calculationElement.SystemTypeName = duct.MEPSystem?.GetElementType()?.Name ?? string.Empty;
        
        DuctType ductType = (DuctType) duct.GetElementType();

        if(ductType.Shape == ConnectorProfileType.Round) {
            calculationElement.IsRound = true;
            calculationElement.Diameter_mm = ductIns.Diameter;
            calculationElement.Perimeter_mm = Math.PI * ductIns.Diameter;
        } else {
            calculationElement.IsRound = false;
            calculationElement.Width_mm = ductIns.Width;
            calculationElement.Height_mm = ductIns.Height;
            calculationElement.Perimeter_mm = ductIns.Width * 2 + ductIns.Height * 2;
        }

        return calculationElement;
    }

    private CalculationElementBase CreateSystem(Element element) {
        CalculationElementBase calculationElement = element.Category.IsId(BuiltInCategory.OST_PipingSystem)
            ? new CalculationElementPipeSystem(element)
            : new CalculationElementDuctSystem(element);

        MEPSystem mepSystem = element as MEPSystem;
        ElementSet network = new ElementSet();

        if(mepSystem != null && mepSystem.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
            network = ((PipingSystem) mepSystem).PipingNetwork;
        } else if(mepSystem != null && mepSystem.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
            network = ((MechanicalSystem) mepSystem).DuctNetwork;
        }

        Element firstNetworkElement = network.Cast<Element>().FirstOrDefault();
        if(firstNetworkElement is null) {
            return calculationElement;
        }

        string system =
            firstNetworkElement.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");

        calculationElement.SystemSharedName = system;
        calculationElement.SystemTypeName = element.GetElementType().Name;

        return calculationElement;
    }
}
