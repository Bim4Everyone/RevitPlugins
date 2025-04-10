using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.Services {
    internal class EntitiesTracker {
        private readonly ObservableCollection<SheetModel> _aliveSheets;
        private readonly ObservableCollection<ViewPortModel> _aliveViewPorts;
        private readonly ObservableCollection<ScheduleModel> _aliveSchedules;
        private readonly ObservableCollection<AnnotationModel> _aliveAnnotations;
        private readonly HashSet<ElementId> _removedEntities;

        public EntitiesTracker() {
            _aliveSheets = [];
            _aliveViewPorts = [];
            _aliveSchedules = [];
            _aliveAnnotations = [];
            _removedEntities = [];

            AliveSheets = new ReadOnlyObservableCollection<SheetModel>(_aliveSheets);
            AliveViewPorts = new ReadOnlyObservableCollection<ViewPortModel>(_aliveViewPorts);
            AliveSchedules = new ReadOnlyObservableCollection<ScheduleModel>(_aliveSchedules);
            AliveAnnotations = new ReadOnlyObservableCollection<AnnotationModel>(_aliveAnnotations);
        }


        public ReadOnlyObservableCollection<SheetModel> AliveSheets { get; }

        public ReadOnlyObservableCollection<ViewPortModel> AliveViewPorts { get; }

        public ReadOnlyObservableCollection<ScheduleModel> AliveSchedules { get; }

        public ReadOnlyObservableCollection<AnnotationModel> AliveAnnotations { get; }


        public IReadOnlyCollection<ElementId> GetRemovedEntities() {
            return [.. _removedEntities];
        }

        public bool AddToRemovedEntities(IEntity entity) {
            throw new NotImplementedException();
        }

        public bool AddAliveSheet(SheetModel sheet) {
            throw new NotImplementedException();
        }

        public bool AddAliveViewPort(ViewPortModel viewPort) {
            throw new NotImplementedException();
        }

        public bool AddAliveSchedule(ScheduleModel schedule) {
            throw new NotImplementedException();
        }

        public bool AddAliveAnnotation(AnnotationModel annotation) {
            throw new NotImplementedException();
        }
    }
}
