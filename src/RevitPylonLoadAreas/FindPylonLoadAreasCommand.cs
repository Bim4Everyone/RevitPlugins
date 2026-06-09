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

using RevitPylonLoadAreas.Models;
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

        ValidateView(repo, localization);
        var floor = PickFloor(repo, localization);
        var pylons = PickPylons(repo, localization);
        var walls = PickWalls(repo, localization);

        var loadAreasFinder = kernel.Get<LoadAreasFinder>();
        var loadAreas = loadAreasFinder.Process(floor, pylons, walls);

        var drawer = kernel.Get<FilledRegionDrawer>();
        using(var t = repo.Document.StartTransaction(localization.GetLocalizedString("Transaction.DrawLoadAreas"))) {
            foreach(var loadArea in loadAreas) {
                drawer.Draw(loadArea);
                double area = repo.GetArea([..loadArea.Circuits]);
                // TODO определить параметр, куда писать площадь
                loadArea.Element.SetParamValue(
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                    UnitUtils.ConvertFromInternalUnits(area, UnitTypeId.SquareMeters)
                        .ToString("0.000", CultureInfo.CurrentCulture));
            }

            t.Commit();
        }

        Notification(true);
    }

    private void ValidateView(RevitRepository repo, ILocalizationService localization) {
        if(repo.ActiveView.ViewType != ViewType.FloorPlan) {
            TaskDialog.Show(PluginName, localization.GetLocalizedString("Error.ViewNotSupported"));
            throw new OperationCanceledException();
        }
    }

    private Floor PickFloor(RevitRepository repo, ILocalizationService localization) {
        return repo.PickFloor(localization.GetLocalizedString("Pick.Floor"));
    }

    private ICollection<FamilyInstance> PickPylons(RevitRepository repo, ILocalizationService localization) {
        return repo.PickStructuralColumns(localization.GetLocalizedString("Pick.Pylons"));
    }

    private ICollection<Wall> PickWalls(RevitRepository repo, ILocalizationService localization) {
        ICollection<Wall> walls;
        try {
            walls = repo.PickWalls(localization.GetLocalizedString("Pick.Walls"));
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            walls = [];
        }

        return walls;
    }
}
