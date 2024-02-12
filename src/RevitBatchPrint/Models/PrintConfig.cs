using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitBatchPrint.Models {
    public class PrintConfig : ProjectConfig<PrintSettingsConfig> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PrintConfig GetPrintConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitBatchPrint))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PrintConfig) + ".json")
                .Build<PrintConfig>();
        }
    }

    public class PrintSettingsConfig : ProjectSettings {
        public string FolderName { get; set; }
        public string PrinterName { get; set; }
        public string PrintParamName { get; set; }
        public List<string> SelectedAlbums { get; set; } = new List<string>();

        public int Zoom { get; set; }
        public ZoomType ZoomType { get; set; }

        public HiddenLineViewsType HiddenLineViews { get; set; }
        public ColorDepthType ColorDepth { get; set; }
        public RasterQualityType RasterQuality { get; set; }
        public PageOrientationType PageOrientation { get; set; }

        public PaperPlacementType PaperPlacement { get; set; }
        public MarginType MarginType { get; set; }
        public double UserDefinedMarginX { get; set; }
        public double UserDefinedMarginY { get; set; }

        public bool MaskCoincidentLines { get; set; }
        public bool ViewLinksinBlue { get; set; }
        public bool HideReforWorkPlanes { get; set; }
        public bool HideUnreferencedViewTags { get; set; }
        public bool HideScopeBoxes { get; set; }
        public bool HideCropBoundaries { get; set; }
        public bool ReplaceHalftoneWithThinLines { get; set; }
        public override string ProjectName { get; set; }
    }
}