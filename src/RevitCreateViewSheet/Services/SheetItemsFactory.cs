using System;
using System.Linq;

using Ninject;
using Ninject.Syntax;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.ViewModels;
using RevitCreateViewSheet.Views;

namespace RevitCreateViewSheet.Services {
    internal class SheetItemsFactory : ISheetItemsFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public SheetItemsFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
        }


        public AnnotationModel CreateAnnotation(SheetModel sheetModel) {
            var window = _resolutionRoot.Get<AnnotationModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var symbol = (window.DataContext as AnnotationModelCreatorViewModel)
                    .SelectedAnnotationSymbolType
                    .AnnotationSymbolType;
                return new AnnotationModel(sheetModel, symbol);
            }
            throw new OperationCanceledException();
        }

        public ScheduleModel CreateSchedule(SheetModel sheetModel) {
            var window = _resolutionRoot.Get<ScheduleModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var creatorView = window.DataContext as ScheduleModelCreatorViewModel;
                var selectedSchedule = creatorView.SelectedViewSchedule;
                creatorView.ViewSchedules.Remove(selectedSchedule);
                creatorView.SelectedViewSchedule = creatorView.ViewSchedules.FirstOrDefault();
                return new ScheduleModel(sheetModel, selectedSchedule.ViewSchedule);
            }
            throw new OperationCanceledException();
        }

        public ViewPortModel CreateViewPort(SheetModel sheetModel) {
            var window = _resolutionRoot.Get<ViewPortModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var creatorView = window.DataContext as ViewPortModelCreatorViewModel;
                var selectedView = creatorView.SelectedView;
                var selectedViewPortType = creatorView.SelectedViewPortType;

                creatorView.Views.Remove(selectedView);
                creatorView.SelectedView = creatorView.Views.FirstOrDefault();
                return new ViewPortModel(sheetModel, selectedView.View, selectedViewPortType.ViewType);
            }
            throw new OperationCanceledException();
        }
    }
}
