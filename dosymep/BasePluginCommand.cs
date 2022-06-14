using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace dosymep.Bim4Everyone {
    public abstract class BasePluginCommand : IExternalCommand {
        public BasePluginCommand() {
            PluginName = GetType().Name;
        }

        /// <summary>
        /// Предоставляет доступ к логгеру расширения.
        /// </summary>
        protected ILoggerService PluginLoggerService { get; private set; }

        /// <summary>
        /// Предоставляет доступ к логгеру платформы.
        /// </summary>
        protected ILoggerService LoggerService => GetPlatformService<ILoggerService>();

        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            PluginLoggerService = LoggerService.ForPluginContext(PluginName);
            PluginLoggerService.Information("Запуск команды расширения.");

            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Execute(commandData.Application);
                PluginLoggerService.Information("Выход из команды расширения.");
            } catch(OperationCanceledException) {
                PluginLoggerService.Warning("Отмена выполнения команды расширения.");
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
                PluginLoggerService.Warning("Отмена выполнения команды расширения.");
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            } catch(Exception ex) {
                PluginLoggerService.Warning(ex, "Ошибка в команде расширения.");
#if D2020 || D2021 || D2022
                TaskDialog.Show(PluginName, ex.ToString());
#else
                TaskDialog.Show(PluginName, ex.Message);
#endif
                GetPlatformService<INotificationService>()
                    .CreateFatalNotification(PluginName, "Выполнение скрипта завершено с ошибкой.")
                    .ShowAsync();
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Наименование расширения для логгера
        /// </summary>
        protected string PluginName { get; set; }

        /// <summary>
        /// Метод команды Revit
        /// </summary>
        /// <param name="uiApplication">Приложение Revit.</param>
        protected abstract void Execute(UIApplication uiApplication);

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}