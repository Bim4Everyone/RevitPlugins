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
using dosymep.SimpleServices;
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
        var localization = kernel.Get<ILocalizationService>();

        if(!repo.IsViewSupportsLoadAreas(repo.ActiveView)) {
            TaskDialog.Show(PluginName, localization.GetLocalizedString("Error.ViewNotSupported"));
            throw new OperationCanceledException();
        }

        var floor = repo.PickFloor(localization.GetLocalizedString("Pick.Floor"));
        var pylons = repo.PickStructuralColumns(localization.GetLocalizedString("Pick.Pylons"));
        IReadOnlyList<Wall> walls;
        try {
            walls = repo.PickWalls(localization.GetLocalizedString("Pick.Walls"));
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            walls = Array.Empty<Wall>();
        }

        var filledRegionType = repo.GetFirstFilledRegionType()
                               ?? throw new FilledRegionTypeNotFoundException(
                                   localization.GetLocalizedString("Error.FilledRegionTypeNotFound"));

        var finder = kernel.Get<LoadAreasFinder>();
        var loadAreas = finder.Process(floor, pylons, walls);

        var drawer = kernel.Get<FilledRegionDrawer>();
        string transactionName = localization.GetLocalizedString("Transaction.DrawLoadAreas");
        using(var t = repo.Document.StartTransaction(transactionName)) {
            drawer.Draw(repo.ActiveView, loadAreas, filledRegionType);
            foreach(var area in loadAreas) {
                double sqM = area.GetArea() / GeometryTolerance.SqFeetPerSqMeter;
                area.Element.SetParamValue(
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                    sqM.ToString("0.00", CultureInfo.InvariantCulture));
            }

            t.Commit();
        }

        Notification(true);
    }
}
