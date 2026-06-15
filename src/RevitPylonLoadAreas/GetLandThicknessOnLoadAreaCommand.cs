using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Services;

namespace RevitPylonLoadAreas;

[Transaction(TransactionMode.Manual)]
public class GetLandThicknessOnLoadAreaCommand : BasePluginCommand {
    public GetLandThicknessOnLoadAreaCommand() {
        PluginName = "Импорт LandXML";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<SystemConfig>()
            .ToMethod(c => SystemConfig.GetConfig(c.Kernel.Get<IConfigSerializer>()))
            .InSingletonScope();
        kernel.Bind<LandXmlImporter>().ToSelf().InSingletonScope();
        kernel.Bind<LoadAreasFinder>().ToSelf().InSingletonScope();
        kernel.Bind<VoronoiBuilder>().ToSelf().InSingletonScope();
        kernel.Bind<LandThicknessFinder>().ToSelf().InSingletonScope();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var localization = kernel.Get<ILocalizationService>();
        kernel.UseWpfOpenFileDialog(
            title: localization.GetLocalizedString("OpenLandXmlDialog.Title"),
            filter: "LandXML (*.xml)|*.xml");

        Run(kernel);
        Notification(true);
    }

    private void Run(IKernel kernel) {
        var repo = kernel.Get<RevitRepository>();
        var localization = kernel.Get<ILocalizationService>();
        ValidateView(repo, localization);
        ValidateParams(repo, localization);

        var thicknessFinder = kernel.Get<LandThicknessFinder>();
        using var t = repo.Document.StartTransaction(localization.GetLocalizedString("Transaction.LandThickness"));
        thicknessFinder.FindAndSetLandThickness();
        t.Commit();

        // TODO добавить показ ошибок, если они есть в ErrorsService
    }

    private void ValidateView(RevitRepository repo, ILocalizationService localization) {
        if(repo.ActiveView is not ViewPlan) {
            TaskDialog.Show(PluginName, localization.GetLocalizedString("Error.ViewNotSupported"));
            throw new OperationCanceledException();
        }
    }

    private void ValidateParams(RevitRepository repo, ILocalizationService localization) {
        // TODO
    }
}
