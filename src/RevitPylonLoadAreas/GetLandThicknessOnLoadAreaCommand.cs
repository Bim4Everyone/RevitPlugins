using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
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
public class GetLandThicknessOnLoadAreaCommand : BasePluginCommand {
    public GetLandThicknessOnLoadAreaCommand() {
        PluginName = "Импорт LandXML";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<LandXmlImporter>().ToSelf().InSingletonScope();

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
        var dialog = kernel.Get<IOpenFileDialogService>();
        if(!dialog.ShowDialog()) {
            throw new OperationCanceledException();
        }

        var importer = kernel.Get<LandXmlImporter>();
        var repo = kernel.Get<RevitRepository>();
        // TODO
        var polygons = importer.Import(dialog.File.FullName);
        var solid = repo.CreateSolid(polygons);
        using var t = repo.Document.StartTransaction("test");
        repo.CreateDirectShape(solid);
        t.Commit();
    }
}
