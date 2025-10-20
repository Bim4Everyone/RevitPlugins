using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.PluginOptions;
internal class DimensionLineOffsetOption {

    public DimensionLineOffsetOption(Element elemForOffset, DirectionType offsetDirectionType, double offsetCoefficient) {
        ElemForOffset = elemForOffset;
        OffsetDirectionType = offsetDirectionType;
        OffsetCoefficient = offsetCoefficient;
    }

    public Element ElemForOffset { get; set;  }
    public DirectionType OffsetDirectionType { get; set;  }
    public double OffsetCoefficient { get; set;  }
}
