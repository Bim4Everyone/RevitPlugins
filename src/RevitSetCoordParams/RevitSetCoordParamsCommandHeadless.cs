using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;

using Ninject;

using RevitSetCoordParams.HeadlessMode.Models;
using RevitSetCoordParams.Models;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Services;
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

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Настройка сервиса проверки параметров
        kernel.Bind<IParamAvailabilityService>()
            .To<ParamAvailabilityService>()
            .InSingletonScope();

        // Получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var localizationService = kernel.Get<ILocalizationService>();
        var revitRepository = kernel.Get<RevitRepository>();
        var pluginConfig = kernel.Get<PluginConfig>();

        // Основной блок кода для выполнения в Headless-режиме
        try {
            // Загрузка параметров проекта        
            bool isParamChecked = new CheckProjectParams(uiApplication.Application, uiApplication.ActiveUIDocument.Document)
                .CopyProjectParams()
                .GetIsChecked();

            // Получение настроек проекта
            ConfigSettings defaultSettings;
            ConfigSettings configSettings;
            var projectConfig = pluginConfig.GetSettings(revitRepository.Document);
            if(projectConfig == null) {
                defaultSettings = new ConfigSettings();
                defaultSettings.ApplyDefaultValues(revitRepository);
                configSettings = defaultSettings;
            } else {
                defaultSettings = new ConfigSettings();
                defaultSettings.ApplyDefaultValues(revitRepository);
                configSettings = projectConfig.ConfigSettings;
            }

            // Класс для чтения JournalData для дальнейшего парсинга
            var reader = new JournalDataReader(JournalData);

            var paramService = kernel.Get<IParamAvailabilityService>();
            var paramFactory = kernel.Get<IRevitParamFactory>();

            // Класс для парсинга JournalData
            var resolver = new JournalSettingsResolver(
                configSettings,
                defaultSettings,
                paramService,
                paramFactory,
                revitRepository);

            // Переопределение настроек
            string sourceFile = resolver.ResolveString(reader.SourceFile, x => x.SourceFile);
            var sourceDocument = revitRepository.FindDocumentsByName(sourceFile);

            var finalSettings = new ConfigSettings {
                ElementsProvider = resolver.ResolveEnum(reader.ElementsProvider, x => x.ElementsProvider),
                PositionProvider = resolver.ResolveEnum(reader.PositionProvider, x => x.PositionProvider),
                SourceFile = sourceFile,
                TypeModels = resolver.ResolveListString(reader.TypeModels, x => x.TypeModels),
                ParamMaps = resolver.ResolveParamMaps(reader.ParamMaps, x => x.ParamMaps, sourceDocument),
                Categories = resolver.ResolveListEnum(reader.Categories, x => x.Categories),
                MaxDiameterSearchSphereMm = resolver.ResolveDouble(reader.MaxDiameterSearchSphereMm, x => x.MaxDiameterSearchSphereMm),
                StepDiameterSearchSphereMm = resolver.ResolveDouble(reader.StepDiameterSearchSphereMm, x => x.StepDiameterSearchSphereMm),
                Search = resolver.ResolveBool(reader.Search, x => x.Search)
            };

            var setCoordParamsSettings = new SetCoordParamsSettings(revitRepository, finalSettings);
            IIntersectProcessor processor = new IntersectCurveProcessor(localizationService, revitRepository, setCoordParamsSettings);

            processor.Run();

        } catch(Exception ex) {
            string errorText = localizationService.GetLocalizedString("RevitSetCoordParamsCommandHeadless.Error");
            PluginLoggerService.Error(ex, errorText);
            throw;
        }
    }
}
