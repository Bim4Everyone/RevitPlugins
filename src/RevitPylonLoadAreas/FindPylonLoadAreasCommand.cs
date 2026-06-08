using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.WpfCore.Ninject;

using Ninject;

using RevitPylonLoadAreas.Exceptions;
using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Services;

namespace RevitPylonLoadAreas;

[Transaction(TransactionMode.Manual)]
public class FindPylonLoadAreasCommand : BasePluginCommand {
    public FindPylonLoadAreasCommand() {
        PluginName = "Грузовые площади";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>().ToSelf().InSingletonScope();
        kernel.Bind<SystemConfig>().ToSelf().InSingletonScope();
        kernel.Bind<VoronoiBuilder>().ToSelf().InSingletonScope();
        kernel.Bind<LoadAreasFinder>().ToSelf().InSingletonScope();
        kernel.Bind<FilledRegionDrawer>().ToSelf().InSingletonScope();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var repo = kernel.Get<RevitRepository>();
        if(!repo.IsViewSupportsLoadAreas(repo.ActiveView)) {
            TaskDialog.Show(PluginName, "Активный вид не поддерживает создание грузовых площадей. Откройте план, разрез или фасад.");
            throw new OperationCanceledException();
        }

        var floor = repo.PickFloor("Выберите плиту перекрытия");
        var pylons = repo.PickStructuralColumns("Выберите пилоны (несущие колонны)");
        var walls = TryPickWalls(repo);

        var filledRegionType = repo.GetFirstFilledRegionType()
            ?? throw new FilledRegionTypeNotFoundException();

        var finder = kernel.Get<LoadAreasFinder>();
        var loadAreas = finder.Process(floor, pylons, walls);

        var drawer = kernel.Get<FilledRegionDrawer>();
        using(var t = repo.Document.StartTransaction("BIM: Грузовые площади")) {
            drawer.Draw(repo.ActiveView, loadAreas, filledRegionType);
            foreach(var area in loadAreas) {
                double sqM = GeometryTolerance.SqFeetToSqMeters(area.GetArea());
                area.Element.SetParamValue(
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                    sqM.ToString("0.00", CultureInfo.InvariantCulture));
            }
            t.Commit();
        }

        Notification(true);
    }

    private static IReadOnlyList<Wall> TryPickWalls(RevitRepository repo) {
        try {
            return repo.PickWalls("Выберите стены под плитой (ESC чтобы пропустить)");
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            return Array.Empty<Wall>();
        }
    }
}
