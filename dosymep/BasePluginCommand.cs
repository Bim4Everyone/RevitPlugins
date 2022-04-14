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