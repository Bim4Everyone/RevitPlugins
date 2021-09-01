﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitBatchPrint.Models {
    public class PrintConfig {
        public List<PrintSettingsConfig> PrintSettings { get; set; } = new List<PrintSettingsConfig>();

        public PrintSettingsConfig GetPrintSettingsConfig(string documentName) {
            return PrintSettings.FirstOrDefault(item => documentName.Equals(item.DocumentName));
        }

        private static string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitBatchPrint", "PrintConfig.json");
        }

        public static PrintConfig GetConfig() {
            if(File.Exists(GetConfigPath())) {
                return JsonConvert.DeserializeObject<PrintConfig>(File.ReadAllText(GetConfigPath()));
            }

            return new PrintConfig();
        }

        public static void SaveConfig(PrintConfig config) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath()));
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(config));
        }
    }

    public class PrintSettingsConfig {
        public string DocumentName { get; set; }

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
    }
}
