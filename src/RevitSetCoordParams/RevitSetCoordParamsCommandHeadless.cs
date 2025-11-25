using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;

using Ninject;

using RevitSetCoordParams.Models;
using RevitSetCoordParams.Models.Settings;

namespace RevitSetCoordParams;

[Transaction(TransactionMode.Manual)]
public class RevitSetCoordParamsCommandHeadless : BasePluginCommand {

    public RevitSetCoordParamsCommandHeadless() {
        PluginName = "Заполнить параметры СМР";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using var kernel = uiApplication.CreatePlatformServices();

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
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Основной блок кода для выполнения в Headless-режиме
        try {
            // Загрузка параметров проекта        
            bool isParamChecked = new CheckProjectParams(uiApplication.Application, uiApplication.ActiveUIDocument.Document)
                .CopyProjectParams()
                .GetIsChecked();
            // Создание экземпляра класса настроек плагина по умолчанию
            var defaultConfigSettings = new ConfigSettings();
            defaultConfigSettings.ApplyDefaultValues();

            var localizationService = kernel.Get<ILocalizationService>();
            var revitRepository = kernel.Get<RevitRepository>();

            // Создание основного класса настроек и загрузка в него настроек по умолчанию
            var setCoordParamsSettings = new SetCoordParamsSettings(revitRepository, defaultConfigSettings);
            setCoordParamsSettings.LoadConfigSettings();

            // Создание основного класса процессора
            var processor = new SetCoordParamsProcessor(localizationService, revitRepository, setCoordParamsSettings);

            // Основной метод
            processor.Run();

        } catch {
        }
    }
}
