using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

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
            if(sheetModel is null) {
                throw new ArgumentNullException(nameof(sheetModel));
            }

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
            if(sheetModel is null) {
                throw new ArgumentNullException(nameof(sheetModel));
            }

            var window = _resolutionRoot.Get<ScheduleModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var creatorView = window.DataContext as ScheduleModelCreatorViewModel;
                var selectedSchedule = creatorView.SelectedViewSchedule;
                creatorView.SelectedViewSchedule = creatorView.ViewSchedules.FirstOrDefault();
                return new ScheduleModel(sheetModel, selectedSchedule.ViewSchedule);
            }
            throw new OperationCanceledException();
        }

        public ViewPortModel CreateViewPort(SheetModel sheetModel, ICollection<View> disabledViews) {
            if(sheetModel is null) {
                throw new ArgumentNullException(nameof(sheetModel));
            }

            if(disabledViews is null) {
                throw new ArgumentNullException(nameof(disabledViews));
            }

            var creatorView = _resolutionRoot.Get<ViewPortModelCreatorViewModel>();
            creatorView.SetDisabledViews(disabledViews);
            var window = _resolutionRoot.Get<ViewPortModelCreatorWindow>();
            if(window.ShowDialog() ?? false) {
                var selectedView = creatorView.SelectedView;
                var selectedViewPortType = creatorView.SelectedViewPortType;

                creatorView.SelectedView = creatorView.EnabledViews.FirstOrDefault();
                return new ViewPortModel(sheetModel, selectedView.View, selectedViewPortType.ViewType);
            }
            throw new OperationCanceledException();
        }
    }
}
