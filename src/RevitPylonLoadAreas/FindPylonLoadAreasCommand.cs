using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Services;
using RevitPylonLoadAreas.Services.Core;

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

        kernel.UseWpfUIThemeUpdater();
        kernel.UseWpfWindowsTheme();
        kernel.Bind<IHasTheme>().To<HasTheme>().InSingletonScope();
        kernel.Bind<IHasLocalization>().To<HasLocalization>().InSingletonScope();

        kernel.UseWpfUIMessageBox();

        Run(kernel);
        kernel.Get<SystemConfig>().SaveProjectConfig(); // чтобы json файл появился при первом запуске плагина
        Notification(true);
    }

    private void ValidateView(RevitRepository repo, IMessageBoxService msg, ILocalizationService localization) {
        if(repo.ActiveView is not ViewPlan) {
            ShowError(msg, localization.GetLocalizedString("Error.ViewNotSupported"));
            throw new OperationCanceledException();
        }
    }

    private void Run(IKernel kernel) {
        var repo = kernel.Get<RevitRepository>();
        var localization = kernel.Get<ILocalizationService>();
        var msg = kernel.Get<IMessageBoxService>();

        ValidateView(repo, msg, localization);
        ValidateParams(repo, msg, localization);
        var floor = repo.PickFloor(localization.GetLocalizedString("Pick.Floor"));
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
            double area = loadArea.GetArea();
            if(loadArea.ElementIsPylon()) {
                loadArea.Element.SetParamValue(
                    repo.LoadAreaParam,
                    UnitUtils.ConvertFromInternalUnits(area, UnitTypeId.SquareMeters));
            }
        }

        t.Commit();
    }

    private void ValidateParams(RevitRepository repo, IMessageBoxService msg, ILocalizationService localization) {
        if(!repo.CategoryHasParam(BuiltInCategory.OST_StructuralColumns, repo.LoadAreaParam)) {
            ShowError(msg, localization.GetLocalizedString("Error.ParamNotFound", repo.LoadAreaParam.Name));
            throw new OperationCanceledException();
        }
    }

    private void ShowError(IMessageBoxService msg, string str) {
        msg.Show(str, PluginName, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
