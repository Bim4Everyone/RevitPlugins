using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Models.Interfaces;

using static Autodesk.Revit.DB.SpecTypeId;

namespace RevitPunchingRebar.ViewModels;
internal class PlacementHandler : IExternalEventHandler {
    private readonly RevitRepository _revitRepository;
    public RevitSettings Settings { get; set; }

    public PlacementHandler(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public void Execute(UIApplication app) {

        using(var t = new Transaction(app.ActiveUIDocument.Document, "Расстановка каркасов")) {
            t.Start();

            List<IPylon> pylons = _revitRepository
                .GetPylons<StructColumnPylon>(Settings, e => new StructColumnPylon(e))
                .Cast<IPylon>()
                .ToList();

            FrameParams frameParams = new FrameParams();

            frameParams.FrameFamilyName = Settings.FamilyName;
            frameParams.FrameFamilyTypeName = Settings.FamilyType;
            frameParams.HostSlab = _revitRepository.GetSlab(Settings.SlabId);
            frameParams.SlabThickness = frameParams.HostSlab.Thickness;
            frameParams.RebarCoverTop = UnitUtilsHelper.FromMm(Settings.RebarCoverTop);
            frameParams.RebarCoverBottom = UnitUtilsHelper.FromMm(Settings.RebarCoverBottom);
            frameParams.PlateRebarDiameter = UnitUtilsHelper.FromMm(Settings.SlabRebarDiameter);

            frameParams.StirrupRebarClass = Settings.RebarClass == "А500С" ? RebarClass.A500C : RebarClass.A240;

            frameParams.StirrupRebarDiameter = UnitUtilsHelper.FromMm(Settings.StirrupDiameter);
            frameParams.StirrupStep = UnitUtilsHelper.FromMm(Settings.StirrupStep);
            frameParams.FrameWidth = UnitUtilsHelper.FromMm(Settings.FrameWidth);

            PunchingRebarPlacementService punchingRebarPlacementService = new PunchingRebarPlacementService();
            punchingRebarPlacementService.Run(pylons, frameParams, _revitRepository);
            
            t.Commit();
        }
            
    }
        
    public string GetName() => "PlacementHandler";
}
