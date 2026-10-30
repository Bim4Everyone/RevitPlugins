using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitStairRailing.Models;
using RevitStairRailing.Services;
using RevitStairRailing.ViewModels;
using RevitStairRailing.Views;

namespace RevitStairRailing;

/// <summary>
/// Команда «Ограждение лестниц»: ограждение КМ из семейств «Каркас несущий» по линиям модели траектории.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RevitStairRailingCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitStairRailingCommand() {
        // имя плагина в журнале и окнах платформы — как на кнопке; в шаблонах платформы задаётся строкой
        PluginName = "Ограждение лестниц";
    }

    /// <summary>
    /// Метод выполнения основного кода плагина.
    /// </summary>
    /// <param name="uiApplication">Интерфейс взаимодействия с Revit.</param>
    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RailingPlacement>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.UseWpfUIThemeUpdater();
        kernel.UseWpfUIMessageBox();

        kernel.BindMainWindow<MainViewModel, MainWindow>();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Окно показывается здесь, а не через Notification(window) шаблона: тот сразу после закрытия окна
        // выдаёт уведомление «завершено успешно», а линии выбираются и ограждение строится после окна
        // (выбор в Revit идёт без открытого модального окна). Уведомление платформы — после построения
        // (Notification(true)); при отмене окна Notification(false) завершает команду отменой.
        var mainWindow = kernel.Get<MainWindow>();
        bool? dialogResult = mainWindow.ShowDialog();
        if(dialogResult != true) {
            Notification(dialogResult);
            return;
        }

        RailingParameters parameters = ((MainViewModel) mainWindow.DataContext).Parameters
                                       ?? throw new InvalidOperationException("Railing parameters are not set.");
        try {
            kernel.Get<RailingPlacement>().Run(parameters);
            Notification(true);
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            // Esc при выборе линий в Revit — обычная отмена пользователем, а не ошибка: ничего не строится
            // и окно ошибки платформы не нужно, поэтому исключение дальше не передаётся.
        } catch(RailingException ex) {
            var localizationService = kernel.Get<ILocalizationService>();
            kernel.Get<IMessageBoxService>().Show(
                localizationService.GetLocalizedString(ex.MessageKey),
                localizationService.GetLocalizedString("MainWindow.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
