using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitRooms.Models;

namespace RevitRooms.ViewModels;
internal class SpatialElementViewModel : ElementViewModel<SpatialElement> {
    public SpatialElementViewModel(SpatialElement element, RevitRepository revitRepository)
        : base(element, revitRepository) {
        var phase = revitRepository.GetPhase(element);
        if(phase != null) {
            Phase = new PhaseViewModel(phase, revitRepository);
        }
        var segments = Element.GetBoundarySegments(SpatialElementExtensions.DefaultBoundaryOptions);

        BoundarySegments = segments
            .SelectMany(item => item)
            .Select(item => item.ElementId)
            .ToHashSet();

        IsCountourIntersect = element.IsSelfCrossBoundaries();
        if(RoomArea == null || RoomArea == 0) {
            IsRedundant = segments.Count > 0;
            NotEnclosed = segments.Count == 0;
        }
    }

    public Element RoomTypeGroup => GetParamElement(ProjectParamsConfig.Instance.RoomTypeGroupName);

    public Element Room => GetParamElement(ProjectParamsConfig.Instance.RoomName);

    public Element RoomGroup => GetParamElement(ProjectParamsConfig.Instance.RoomGroupName);

    public Element RoomSection => GetParamElement(ProjectParamsConfig.Instance.RoomSectionName);

    public string RoomMultilevelGroup => Element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.RoomMultilevelGroup);

    public bool IsRoomMainLevel => Element.GetParamValueOrDefault(ProjectParamsConfig.Instance.IsRoomMainLevel, 0) == 1;

    public ElementId LevelId => Element.LevelId;

    public Level Level => (Level) Element.Document.GetElement(LevelId);
    public double? LevelElevation => Level?.Elevation;

    public override string PhaseName => Phase.Name;

    public override string LevelName => Element.Level.Name;

    public double? RoomArea => (double?) Element.GetParamValueOrDefault(BuiltInParameter.ROOM_AREA);

    public bool? IsRoomLiving => Convert.ToInt32(Element.GetParamValueOrDefault(ProjectParamsConfig.Instance.IsRoomLiving)) == 1;

    public bool? IsRoomBalcony => Convert.ToInt32(Element.GetParamValueOrDefault(ProjectParamsConfig.Instance.IsRoomBalcony)) == 1;
#if REVIT_2021_OR_LESS

    public double? RoomAreaRatio {
        get { return (double?) Element.GetParamValueOrDefault(ProjectParamsConfig.Instance.RoomAreaRatio); }
    }

#else

    public double? RoomAreaRatio => (double?) Element.GetParamValueOrDefault(SharedParamsConfig.Instance.RoomAreaRatio);

#endif

    public double? Area {
        get => (double?) Element.GetParamValueOrDefault(SharedParamsConfig.Instance.RoomArea); set => Element.SetParamValue(SharedParamsConfig.Instance.RoomArea, value ?? 0);
    }

    public double? AreaWithRatio {
        get => (double?) Element.GetParamValueOrDefault(SharedParamsConfig.Instance.RoomAreaWithRatio); set => Element.SetParamValue(SharedParamsConfig.Instance.RoomAreaWithRatio, value ?? 0);
    }

    public bool IsNumberFix => Element.GetParamValueOrDefault<int>(ProjectParamsConfig.Instance.IsRoomNumberFix) == 1;
    public bool IsLevelFix => Element.GetParamValueOrDefault<int>(ProjectParamsConfig.Instance.IsRoomLevelFix) == 1;

    public double ComputeRoomAreaWithRatio() {
        // Area = 0 - по умолчанию
        // RoomAreaRatio = 1 - по умолчанию
        return (Area ?? 0) * (RoomAreaRatio ?? 1);
    }

    public bool IsPlaced => Element.Location != null;

    public bool? IsRedundant { get; }
    public bool? NotEnclosed { get; }
    public bool? IsCountourIntersect { get; }

    public PhaseViewModel Phase { get; }
    public HashSet<ElementId> BoundarySegments { get; }

    public void UpdateSharedParams() {
#if REVIT_2021_OR_LESS
        Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaSpec, ProjectParamsConfig.Instance.ApartmentAreaSpec);
        Element.SetParamValue(SharedParamsConfig.Instance.ApartmentGroupName, ProjectParamsConfig.Instance.ApartmentGroupName);
        Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaMinSpec, ProjectParamsConfig.Instance.ApartmentAreaMinSpec);
        Element.SetParamValue(SharedParamsConfig.Instance.ApartmentAreaMaxSpec, ProjectParamsConfig.Instance.ApartmentAreaMaxSpec);

        Element.SetParamValue(SharedParamsConfig.Instance.RoomAreaRatio, ProjectParamsConfig.Instance.RoomAreaRatio);
        Element.SetParamValue(SharedParamsConfig.Instance.RoomGroupShortName, ProjectParamsConfig.Instance.RoomGroupShortName);
        Element.SetParamValue(SharedParamsConfig.Instance.RoomSectionShortName, ProjectParamsConfig.Instance.RoomSectionShortName);
        Element.SetParamValue(SharedParamsConfig.Instance.RoomTypeGroupShortName, ProjectParamsConfig.Instance.RoomTypeGroupShortName);
        Element.SetParamValue(SharedParamsConfig.Instance.FireCompartmentShortName, ProjectParamsConfig.Instance.FireCompartmentShortName);
#endif
    }

    private Element GetParamElement(RevitParam revitParam) {
        var elementId = (ElementId) Element.GetParamValueOrDefault(revitParam);
        return elementId == null ? null : Element.Document.GetElement(elementId);
    }
}
