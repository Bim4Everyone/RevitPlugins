using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitCopyViews.Models;
using RevitCopyViews.ViewModels;

namespace RevitCopyViews;

[Transaction(TransactionMode.Manual)]
public class UpdateElevationCommand : BasePluginCommand {
    public UpdateElevationCommand() {
        PluginName = "Обновить отметки этажа";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using IKernel kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // используем message box без привязки к окну,
        // потому что он вызывается до запуска основного окна
        kernel.UseWpfUIMessageBox();

        Notification(Execute(kernel));
    }

    private bool Execute(IKernel kernel) {
        var revitRepository = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();

        View[] views = revitRepository.GetViews().ToArray();
        string[] restrictedViewNames = revitRepository.GetViewNames(views).ToArray();

        var errors = new List<View>();
        using var transaction =
            revitRepository.StartTransaction(localizationService.GetLocalizedString("UpdateElevation.TransactionName"));

        views = views
            .Where(item => !item.IsTemplate)
            .Where(item => item.ViewType
                is ViewType.FloorPlan
                or ViewType.CeilingPlan
                or ViewType.AreaPlan
                or ViewType.EngineeringPlan)
            .ToArray();

        foreach(var view in views) {
            var splittedName = Delimiter.SplitViewName(
                view.Name,
                new SplitViewOptions {
                    ReplacePrefix = false,
                    ReplaceSuffix = false
                });

            if(splittedName.HasElevation) {
                splittedName.Elevations = SplittedViewName.GetElevation(view);

                string viewName = Delimiter.CreateViewName(splittedName);
                if(view.Name.Equals(viewName)) {
                    continue;
                }

                if(restrictedViewNames.Any(viewName.Equals)) {
                    errors.Add(view);
                    continue;
                }

                view.Name = viewName;
            }
        }

        transaction.Commit();

        if(errors.Count > 0) {
            string title = localizationService.GetLocalizedString("UpdateElevation.NotSelectedViewsTitle");
            string message = localizationService.GetLocalizedString("UpdateElevation.NotSelectedViewsMessage");
            
            message += Environment.NewLine
                       + " - "
                       + string.Join(
                           Environment.NewLine + " - ",
                           errors.Select(item => $"{item.Id.GetIdValue()} - {item.Name}"));
            
            TaskDialog.Show(title, message, TaskDialogCommonButtons.Ok);

            return false;
        } else {
            return true;
        }
    }
}
