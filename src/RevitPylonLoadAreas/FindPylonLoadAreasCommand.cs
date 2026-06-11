using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
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
        kernel.Bind<SystemConfig>()
            .ToMethod(c => SystemConfig.GetConfig(c.Kernel.Get<IConfigSerializer>()))
            .InSingletonScope();
        kernel.Bind<VoronoiBuilder>().ToSelf().InSingletonScope();
        kernel.Bind<LoadAreasFinder>().ToSelf().InSingletonScope();
        kernel.Bind<FilledRegionDrawer>().ToSelf().InSingletonScope();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Run(kernel);
        kernel.Get<SystemConfig>().SaveProjectConfig();
        Notification(true);
    }

    private void ValidateView(RevitRepository repo, ILocalizationService localization) {
        if(repo.ActiveView is not ViewPlan) {
            TaskDialog.Show(PluginName, localization.GetLocalizedString("Error.ViewNotSupported"));
            throw new OperationCanceledException();
        }
    }

    private Floor GetFloor(RevitRepository repo, ILocalizationService localization) {
        return repo.PickFloor(localization.GetLocalizedString("Pick.Floor"));
    }

    private void Run(IKernel kernel) {
        var repo = kernel.Get<RevitRepository>();
        var localization = kernel.Get<ILocalizationService>();

        ValidateView(repo, localization);
        var floor = GetFloor(repo, localization);
        var pylons = repo.GetPylonsFromView();
        var walls = repo.GetWallsFromView();

        var loadAreasFinder = kernel.Get<LoadAreasFinder>();
        var loadAreas = loadAreasFinder.Process(floor, pylons, walls);

        var drawer = kernel.Get<FilledRegionDrawer>();
        using var t = repo.Document.StartTransaction(localization.GetLocalizedString("Transaction.DrawLoadAreas"));
        foreach(var loadArea in loadAreas) {
            var region = drawer.Draw(loadArea);
            region.SetParamValue(
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                loadArea.Element.Id.GetIdValue().ToString()); // для контроля правильности построения
            double area = repo.GetArea([..loadArea.Circuits]);
            // TODO определить параметр, куда писать площадь
            if(loadArea.ElementIsPylon()) {
                loadArea.Element.SetParamValue(
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                    UnitUtils.ConvertFromInternalUnits(area, UnitTypeId.SquareMeters)
                        .ToString("0.000", CultureInfo.CurrentCulture));
            }
        }

        t.Commit();
    }
}
