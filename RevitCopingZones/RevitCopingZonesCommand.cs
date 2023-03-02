﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using Ninject;

using RevitCopingZones.Models;
using RevitCopingZones.ViewModels;
using RevitCopingZones.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitCopingZones {
    [Transaction(TransactionMode.Manual)]
    public class RevitCopingZonesCommand : BasePluginCommand {
        public RevitCopingZonesCommand() {
            PluginName = "Копирование зон СМР";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = new StandardKernel()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InTransientScope();
                kernel.Bind<Application>()
                    .ToConstant(uiApplication.Application)
                    .InTransientScope();

                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<CopingZonesConfig>()
                    .ToMethod(c => CopingZonesConfig.GetCheckingLevelConfig());

                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());


                Check(kernel);
                ShowDialog(kernel);
            }
        }

        private void Check(IKernel kernel) {
            var revitRepository = kernel.Get<RevitRepository>();
            if(!revitRepository.HasAreaScheme()) {
                TaskDialog.Show(PluginName,
                    $"В документе отсутствует схема зонирования с именем \"{RevitRepository.AreaSchemeName}\".");
                throw new OperationCanceledException();
            }

            if(!revitRepository.IsAreaPlan()) {
                TaskDialog.Show(PluginName, $"Текущий вид не является планом зонирования.");
                throw new OperationCanceledException();
            }

            if(!revitRepository.HasAreas()) {
                TaskDialog.Show(PluginName, "На активном виде нет зон.");
                throw new OperationCanceledException();
            }

            if(revitRepository.HasCorruptedAreas()) {
                TaskDialog.Show(PluginName,
                    "Были обнаружены избыточные и не окруженные зоны, выполнение скрипта было отменено.");
                throw new OperationCanceledException();
            }
        }

        private void ShowDialog(IKernel kernel) {
            MainWindow window = kernel.Get<MainWindow>();
            if(window.ShowDialog() == true) {
                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }
    }
}