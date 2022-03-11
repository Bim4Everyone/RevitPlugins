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
        /// Предоставляет доступ к логгеру платформы.
        /// </summary>
        protected ILoggerService PluginLoggerService { get; private set; }
        
        /// <summary>
        /// Предоставляет доступ к логгеру платформы.
        /// </summary>
        protected ILoggerService LoggerService => GetPlatformService<ILoggerService>();


        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            PluginLoggerService = LoggerService.ForPluginContext(PluginName);
            PluginLoggerService.Information("Запуск команды плагина.");
            
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                Execute(commandData.Application);
                return Result.Succeeded;
            } finally {
                PluginLoggerService.Information("Выход из команды плагина.");
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }
        }

        /// <summary>
        /// Наименование плагина для логгера
        /// </summary>
        protected string PluginName { get; } 

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